using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace ElectionManager.Models
{
    /// <summary>
    /// Модель користувача-виборця. Відповідає за зберігання облікових даних та безпечну автентифікацію.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "VoterType")]
    [JsonDerivedType(typeof(Voter), "Voter")]
    [JsonDerivedType(typeof(Admin), "Admin")]
    public class Voter : IUser
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? SessionToken { get; set; }
        public bool IsAdmin { get; set; } = false;
        
        [JsonIgnore]
        public bool IsAuthenticated => true;

        /// <summary>
        /// Встановлює облікові дані користувача, перетворюючи відкритий пароль на хеш методом HashPassword.
        /// </summary>
        /// <param name="login">Унікальний логін користувача у системі.</param>
        /// <param name="rawPassword">Оригінальний пароль у відкритому вигляді.</param>
        /// <exception cref="ArgumentException">Викидається, якщо логін або пароль порожні.</exception>
        public void SetCredentials(string login, string rawPassword)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(rawPassword))
                throw new ArgumentException("Логін та пароль не можуть бути порожніми.");

            Login = login;
            PasswordHash = HashPassword(rawPassword);
        }

        /// <summary>
        /// Перевіряє правильність введеного пароля шляхом порівняння його хешу зі збереженим у системі.
        /// </summary>
        /// <param name="rawPassword">Пароль у відкритому вигляді для перевірки.</param>
        /// <returns>Повертає true, якщо пароль вірний, інакше - false.</returns>
        public bool VerifyPassword(string rawPassword)
        {
            return PasswordHash == HashPassword(rawPassword);
        }

        /// <summary>
        /// Перетворює рядок на незворотній хеш за алгоритмом SHA-256 для безпечного зберігання паролів.
        /// </summary>
        /// <param name="rawData">Оригінальний пароль або секретний ключ у відкритому вигляді.</param>
        /// <returns>Рядок, що містить хеш-суму (64 символи).</returns>
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