using System;
using System.Security.Cryptography;
using System.Text;

namespace ITWaves.Systems
{
    public static class CryptoUtils
    {
        // Simple key for HMAC - in production, use more secure key management
        private const string SECRET_KEY = "ITWaves_SecretKey_2024_HorrorSnake";
        
        public static string ComputeHMAC(string data)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SECRET_KEY)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
        
        public static bool VerifyHMAC(string data, string hash)
        {
            string computedHash = ComputeHMAC(data);
            return computedHash == hash;
        }
    }
}

