using System;
using System.IO;


namespace Silvtek.ORMEngine.Encryption
{
    /// <summary>
    /// Encryptor Provider
    /// </summary>
    [Serializable]
    public abstract class EncryptionProvider 
    {
        private string password;
        private string salt;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        /// <summary>
        /// Gets or sets the salt.
        /// </summary>
        /// <value>The salt.</value>
        public string Salt
        {
            get { return salt; }
            set { salt = value; }
        }


        /// <summary>
        /// Encrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public abstract string Encrypt(string value);

        /// <summary>
        /// Decrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public abstract string Decrypt(string value);


        /// <summary>
        /// Encrypts the stream.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <param name="encryptedStream">The encrypted stream.</param>
        public abstract void EncryptStream(Stream inputStream, Stream encryptedStream);


        /// <summary>
        /// Decrypts the stream.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <param name="decryptedStream">The decrypted stream.</param>
        public abstract void DecryptStream(Stream encryptedStream, Stream decryptedStream);

       
        
    }
}