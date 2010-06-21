using System;
using System.IO;


namespace Silvtek.ORMEngine.Encryption
{
    [Serializable]
    public abstract class EncryptionProvider 
    {
        private string password;
        private string salt;

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public string Salt
        {
            get { return salt; }
            set { salt = value; }
        }


        public abstract string Encrypt(string value);
        public abstract string Decrypt(string value);
        
    }
}