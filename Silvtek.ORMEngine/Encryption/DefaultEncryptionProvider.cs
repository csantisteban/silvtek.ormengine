using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Silvtek.ORMEngine.Configs;

/******************************************************************************
 * DO NOT MODIFIED THE ENCRYPTION ALGORITHM.
 * DOING SO WILL DISABLE THE DECRYPTION OF RECORDS
 * FOR EXTENDING OR USING DIFFERENT ALGORITHG, PROVIDE DIFFERENT PROVIDERS
 * ****************************************************************************/
namespace Silvtek.ORMEngine.Encryption
{
    public class DefaultEncryptionProvider : EncryptionProvider
    {
        private const int _PASSWORD_ITERATIONS = 2;                // can be any number
        private const string _INIT_VECTOR = "t$XYtfBQMCTAoOLG";    // must be 16 bytes
        private const int _KEY_SIZE = 128;                         // can be 192 or 128

        private byte[] saltValueBytes;
        private byte[] initVectorBytes;
#if NET11
        private const string hashName = "SHA1";
#endif
        /// <summary>
        /// Encrypts specified plaintext using Rijndael symmetric key algorithm
        /// and returns a base64-encoded result.
        /// </summary>
        /// <param name="value">
        /// Plaintext value to be encrypted.
        /// </param>
        /// <returns>
        /// Encrypted value formatted as a base64-encoded string.
        /// </returns>
        public override string Encrypt(string value)
        {
            ValidateSalt(Salt);
            // Convert our plaintext into a byte array.
            // Let us assume that plaintext contains UTF8-encoded characters.
            byte[] plainTextBytes = Encoding.ASCII.GetBytes(value);

            byte[] keyBytes = GetKeyBytes();

            // Create uninitialized Rijndael encryption object.
            RijndaelManaged symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;

            // Generate encryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            MemoryStream memoryStream = new MemoryStream();

            // Define cryptographic stream (always use Write mode for encryption).
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            // Start encrypting.
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);

            // Finish encrypting.
            cryptoStream.FlushFinalBlock();

            // Convert our encrypted data from a memory stream into a byte array.
            byte[] cipherTextBytes = memoryStream.ToArray();

            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            // Convert encrypted data into a base64-encoded string.
            string cipherText = Convert.ToBase64String(cipherTextBytes);

            // Return encrypted string.
            return cipherText;
        }

        /// <summary>
        /// Decrypts specified ciphertext using Rijndael symmetric key algorithm.
        /// </summary>
        /// <param name="value">
        /// Base64-formatted ciphertext value.
        /// </param>
        public override string Decrypt(string value)
        {
            ValidateSalt(Salt);
            // Convert our ciphertext into a byte array.
            byte[] cipherTextBytes = Convert.FromBase64String(value);
            byte[] keyBytes = GetKeyBytes();

            // Create uninitialized Rijndael encryption object.
            RijndaelManaged symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;

            // Generate decryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);

            // Define cryptographic stream (always use Read mode for encryption).
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            // Since at this point we don't know what the size of decrypted data
            // will be, allocate the buffer long enough to hold ciphertext;
            // plaintext is never longer than ciphertext.
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            // Start decrypting.
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            // Convert decrypted data into a string. 
            // Let us assume that the original plaintext string was UTF8-encoded.
            string plainText = Encoding.ASCII.GetString(plainTextBytes, 0, decryptedByteCount);

            // Return decrypted string.   
            return plainText;
        }

        public override void EncryptStream(Stream inputStream, Stream encryptedStream)
        {
            ValidateSalt(Salt);

            byte[] keyBytes = GetKeyBytes();

            // Create uninitialized Rijndael encryption object.
            RijndaelManaged symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;

            // Generate encryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

            // Define cryptographic stream (always use Write mode for encryption).
            CryptoStream cryptoStream = new CryptoStream(inputStream, encryptor, CryptoStreamMode.Write);
            // Start encrypting.

            Utility.CopyStream(cryptoStream, encryptedStream, ORMConfigurator.BufferSize, true);

            // Finish encrypting.
            cryptoStream.FlushFinalBlock();
            cryptoStream.Close();

        }

        public override void DecryptStream(Stream encryptedStream, Stream decryptedStream)
        {
            ValidateSalt(Salt);
            byte[] keyBytes = GetKeyBytes();

            // Create uninitialized Rijndael encryption object.
            RijndaelManaged symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;

            // Generate decryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

            // Define cryptographic stream (always use Read mode for encryption).
            CryptoStream cryptoStream = new CryptoStream(encryptedStream, decryptor, CryptoStreamMode.Read);

            Utility.CopyStream(cryptoStream, decryptedStream, ORMConfigurator.BufferSize, true);

            cryptoStream.Close();
        }

        private byte[] GetSaltAsBytes()
        {
            if (saltValueBytes == null)
            {
                saltValueBytes = Encoding.ASCII.GetBytes(Salt);
            }
            return saltValueBytes;
        }

        private void GetVectorAsBytes()
        {
            if (initVectorBytes == null)
            {
                initVectorBytes = Encoding.ASCII.GetBytes(_INIT_VECTOR);
            }
        }

        private byte[] GetKeyBytes()
        {
            GetVectorAsBytes();
            byte[] saltBytes = GetSaltAsBytes();

            // First, we must create a password, from which the key will be derived.
            // This password will be generated from the specified passphrase and 
            // salt value. The password will be created using the specified hash 
            // algorithm. Password creation can be done in several iterations.


#if NET11
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Password, saltBytes, hashName, PASSWORD_ITERATIONS);
            byte[] keyBytes = passwordDeriveBytes.GetBytes(_KEY_SIZE / 8);
#endif
#if NET20
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(Password, saltBytes, _PASSWORD_ITERATIONS);

            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
            byte[] keyBytes = rfc2898DeriveBytes.GetBytes(_KEY_SIZE / 8);
#endif
            return keyBytes;
        }

        private static void ValidateSalt(string salt)
        {
            if (salt == null || salt.Length < 8)
            {
                throw new CryptographicException("invalid salt string. Salt must be 8 character or greater.");
            }
        }
    }
}
