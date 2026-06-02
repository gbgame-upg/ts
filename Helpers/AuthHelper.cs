using System.Security.Cryptography;
using System.Text;

namespace QLHangTonKho.Helpers
{
    public static class AuthHelper
    {
        /// <summary>Hash mật khẩu bằng SHA-256, trả về hex lowercase.</summary>
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}