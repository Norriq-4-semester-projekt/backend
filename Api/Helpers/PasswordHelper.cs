using System;
using System.Security.Cryptography;
using System.Text;

namespace HairForceOne.WebService.Util
{
    /// <summary>
    /// This class contains the generate salt and compute hash methods
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// This method generate a unique salt with a length of 32
        /// </summary>
        /// <returns></returns>
        public static string GenerateSalt()
        {
            int length = 32;
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[length];
            rng.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// This method computes the hashed password with the input password and the generated salt
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string ComputeHash(string password, string salt)
        {
            SHA512Managed sHA512ManagedString = new SHA512Managed();
            byte[] bytes = sHA512ManagedString.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            { builder.Append(bytes[i].ToString("X2")); }
            return builder.ToString();
        }

        /// <summary>
        /// This method compares the input password with the computed PasswordHash the included unique salt
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordHash"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static bool ComparePass(string password, string passwordHash, string salt)
        {
            string newHashedPin = ComputeHash(password, salt);
            return newHashedPin.Equals(passwordHash);
        }
    }
}