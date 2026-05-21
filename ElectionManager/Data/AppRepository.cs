using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ElectionManager.Models;
using System.Text.Encodings.Web;

namespace ElectionManager.Data
{
    /// <summary>
    /// Реалізація бази даних на основі локальних JSON-файлів.
    /// Відповідає за поліморфну серіалізацію об'єктів та забезпечує карантин пошкоджених файлів.
    /// </summary>
    public class AppRepository : IRepository
    {
        public AppRepository()
        {
            Directory.CreateDirectory("Databases");
            Directory.CreateDirectory("Userdata");
        }
        private const string ElectionsFile = "Databases/elections.json";
        private const string VotersFile = "Databases/voters.json";
        private const string SessionFile = "Userdata/session.token";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping    
        };

        #region Логіки завантаження тазбереження виборців і кампаній

        /// <summary>
        /// Завантажує список виборчих кампаній із файлу бази даних.
        /// При виявленні пошкодженого файлу створює його резервну копію та повертає порожній список.
        /// </summary>
        /// <param name="wasCorrupted">Вихідний параметр (out), що набуває значення true, якщо файл був пошкоджений.</param>
        /// <returns>Список об'єктів типу Election.</returns>
        public List<Election> LoadElections(out bool wasCorrupted) => LoadData<Election>(ElectionsFile, out wasCorrupted);

        /// <summary>
        /// Завантажує список виборців із файлу бази даних.
        /// При виявленні пошкодженого файлу створює його резервну копію та повертає порожній список.
        /// </summary>
        /// <param name="wasCorrupted">Вихідний параметр (out), що набуває значення true, якщо файл був пошкоджений.</param>
        /// <returns>Список об'єктів типу Voter.</returns>
        public List<Voter> LoadVoters(out bool wasCorrupted) => LoadData<Voter>(VotersFile, out wasCorrupted);

        // Generic метод для безпечного завантаження колекцій із файлу.
        private List<T> LoadData<T>(string filePath, out bool wasCorrupted)
        {
            wasCorrupted = false;
            try
            {
                if (!File.Exists(filePath))
                    return new List<T>();

                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
            }
            catch (Exception)
            {
                wasCorrupted = true;
                BackupCorruptedFile(filePath);
                return new List<T>();
            }
        }

        // Зберігає пошкоджений файл бази даних в "карантин", перейменовуючи його з додаванням мітки часу.
        private void BackupCorruptedFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string corruptedPath = $"{filePath}_corrupted_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                File.Move(filePath, corruptedPath);
            }
        }

        /// <summary>
        /// Зберігає список усіх виборчих кампаній у базу даних.
        /// </summary>
        /// <param name="elections">Колекція виборчих кампаній для збереження.</param>
        public void SaveElections(IEnumerable<Election> elections)
        {
            string json = JsonSerializer.Serialize(elections, JsonOptions);
            File.WriteAllText(ElectionsFile, json);
        }

        /// <summary>
        /// Зберігає список усіх користувачів у базу даних.
        /// </summary>
        /// <param name="voters">Колекція користувачів для збереження.</param>
        public void SaveVoters(IEnumerable<Voter> voters)
        {
            string json = JsonSerializer.Serialize(voters, JsonOptions);
            File.WriteAllText(VotersFile, json);
        }

        #endregion

        #region Логіка завантаження, збереження та очищення сесії користувача

        /// <summary>
        /// Зчитує токен збереженої сесії користувача.
        /// </summary>
        /// <returns>Рядок із токеном сесії, або null, якщо файл відсутній чи порожній.</returns>
        public string? LoadSession()
        {
            if (!File.Exists(SessionFile))
                return null;

            string token = File.ReadAllText(SessionFile).Trim();
            return string.IsNullOrEmpty(token) ? null : token;
        }

        /// <summary>
        /// Зберігає токен поточної сесії користувача у файл.
        /// </summary>
        /// <param name="token">Унікальний ідентифікатор сесії (GUID).</param>
        public void SaveSession(string token)
        {
            File.WriteAllText(SessionFile, token);
        }

        /// <summary>
        /// Видаляє файл сесії, деавторизуючи поточного користувача при виході з акаунту.
        /// </summary>
        public void ClearSession()
        {
            if (File.Exists(SessionFile))
                File.Delete(SessionFile);
        }

        #endregion

        #region Логіка імпорту-екпорту БД виборів

        /// <summary>
        /// Експортує базу виборчих кампаній у вказаний користувачем JSON-файл.
        /// </summary>
        /// <param name="filePath">Шлях до файлу призначення (обраний через діалогове вікно).</param>
        /// <param name="elections">Колекція виборчих кампаній для експорту.</param>
        public void ExportElectionsTo(string filePath, IEnumerable<Election> elections)
        {
            string json = JsonSerializer.Serialize(elections, JsonOptions);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Імпортує базу виборчих кампаній із вказаного JSON-файлу.
        /// </summary>
        /// <param name="filePath">Шлях до файлу джерела (обраний через діалогове вікно).</param>
        /// <returns>Відновлений список виборчих кампаній або null у разі помилки читання.</returns>
        public List<Election>? ImportElectionsFrom(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Election>>(json, JsonOptions);
        }
        
        #endregion
    }
}