using System;
using System.Security.Cryptography;
using System.Text;

namespace ITWaves.Systems
{
    /// <summary>
    /// Cryptographic utilities for save data integrity.
    /// </summary>
    public static class CryptoUtils
    {
        // Simple key for HMAC - in production, use more secure key management
        private const string SECRET_KEY = "ITWaves_SecretKey_2024_HorrorSnake";
        
        /// <summary>
        /// Compute HMAC-SHA256 hash of data.
        /// </summary>
        public static string ComputeHMAC(string data)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SECRET_KEY)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
        
        /// <summary>
        /// Verify HMAC hash matches data.
        /// </summary>
        public static bool VerifyHMAC(string data, string hash)
        {
            string computedHash = ComputeHMAC(data);
            return computedHash == hash;
        }
    }
}

