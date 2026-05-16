using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace ElectionManager.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "VoterType")]
    [JsonDerivedType(typeof(Voter), "Voter")]
    [JsonDerivedType(typeof(Admin), "Admin")]
    public class Voter
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? SessionToken { get; set; }
        public bool IsAdmin { get; set; } = false;

        public void SetCredentials(string login, string rawPassword)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(rawPassword))
                throw new ArgumentException("Логін та пароль не можуть бути порожніми.");

            Login = login;
            PasswordHash = HashPassword(rawPassword);
        }

        public bool VerifyPassword(string rawPassword)
        {
            return PasswordHash == HashPassword(rawPassword);
        }

        public static string HashPassword(string rawData)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            var builder = new StringBuilder();
            foreach (byte b in bytes)
                builder.Append(b.ToString("x2"));

            return builder.ToString();
        }
    }
}