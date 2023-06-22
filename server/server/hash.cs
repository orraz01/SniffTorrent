using System;
using System.Security.Cryptography;
using System.Text;

namespace server
{
    public class PasswordHashing
    {
        // Generate a random salt
        private static byte[] GenerateSalt(int length)
        {
            byte[] salt = new byte[length];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(salt);
            }
            return salt;
        }

        // Calculate the hash of a password+salt using SHA256
        private static byte[] CalculateHash(string password, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltedPasswordBytes = new byte[passwordBytes.Length + salt.Length];
            Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, saltedPasswordBytes, passwordBytes.Length, salt.Length);

            using (SHA256Managed sha256 = new SHA256Managed())
            {
                return sha256.ComputeHash(saltedPasswordBytes);
            }
        }

        // Generate the hash+salt string for a given password
        public static string GeneratePasswordHash(string password)
        {
            byte[] salt = GenerateSalt(16);
            byte[] hash = CalculateHash(password, salt);

            string saltString = Convert.ToBase64String(salt);
            string hashString = Convert.ToBase64String(hash);

            return hashString + ":" + saltString;
        }

        // Check if a given password matches the provided hash+salt string
        public static bool VerifyPassword(string password, string passwordHash)
        {
            string[] parts = passwordHash.Split(':');
            string hashString = parts[0];
            string saltString = parts[1];

            byte[] hash = Convert.FromBase64String(hashString);
            byte[] salt = Convert.FromBase64String(saltString);

            byte[] calculatedHash = CalculateHash(password, salt);
            string calculatedHashString = Convert.ToBase64String(calculatedHash);

            return hashString.Equals(calculatedHashString);
        }
    }
}
