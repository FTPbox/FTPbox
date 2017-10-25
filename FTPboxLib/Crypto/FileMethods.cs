using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FTPboxLib
{
    internal static class Crypto
    {
        /// <summary>
        /// Calculate hash of the specified file
        /// </summary>
        public static string GetFileHash(string fileName, HashingAlgorithm algorithm)
        {
            var file = new FileStream(fileName, FileMode.Open);

            var hashProvider = CryptoProvider(algorithm);

            byte[] retVal = hashProvider.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private static HashAlgorithm CryptoProvider(HashingAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case HashingAlgorithm.MD5:
                    return new MD5CryptoServiceProvider();
                case HashingAlgorithm.SHA1:
                    return new SHA1CryptoServiceProvider();
                case HashingAlgorithm.SHA256:
                    return new SHA256CryptoServiceProvider();
                case HashingAlgorithm.SHA512:
                    return new SHA512CryptoServiceProvider();
                default:
                    return new SHA1CryptoServiceProvider();
            }
        }
    }
}
