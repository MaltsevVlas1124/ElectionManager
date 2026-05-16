using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ElectionManager.Models;
using System.Text.Encodings.Web;

namespace ElectionManager.Data
{
    public class AppRepository : IRepository
    {
        private const string ElectionsFile = "elections.json";
        private const string VotersFile = "voters.json";
        private const string SessionFile = "session.txt";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping    
        };

        private void BackupCorruptedFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string corruptedPath = $"{filePath}_corrupted_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                File.Move(filePath, corruptedPath);
            }
        }

        public List<Election> LoadElections(out bool wasCorrupted)
        {
            wasCorrupted = false;
            try
            {
                if (!File.Exists(ElectionsFile))
                    return new List<Election>();

                string json = File.ReadAllText(ElectionsFile);
                return JsonSerializer.Deserialize<List<Election>>(json, JsonOptions)
                       ?? new List<Election>();
            }
            catch (Exception)
            {
                wasCorrupted = true;
                BackupCorruptedFile(ElectionsFile);
                return new List<Election>();
            }
        }

        public void SaveElections(IEnumerable<Election> elections)
        {
            string json = JsonSerializer.Serialize(elections, JsonOptions);
            File.WriteAllText(ElectionsFile, json);
        }

        public List<Voter> LoadVoters(out bool wasCorrupted)
        {
            wasCorrupted = false;
            try
            {
                if (!File.Exists(VotersFile))
                    return new List<Voter>();

                string json = File.ReadAllText(VotersFile);
                return JsonSerializer.Deserialize<List<Voter>>(json, JsonOptions)
                       ?? new List<Voter>();
            }
            catch (Exception)
            {
                wasCorrupted = true;
                BackupCorruptedFile(VotersFile);
                return new List<Voter>();
            }
        }

        public void SaveVoters(IEnumerable<Voter> voters)
        {
            string json = JsonSerializer.Serialize(voters, JsonOptions);
            File.WriteAllText(VotersFile, json);
        }

        public string? LoadSession()
        {
            if (!File.Exists(SessionFile))
                return null;

            string token = File.ReadAllText(SessionFile).Trim();
            return string.IsNullOrEmpty(token) ? null : token;
        }

        public void SaveSession(string token)
        {
            File.WriteAllText(SessionFile, token);
        }

        public void ClearSession()
        {
            if (File.Exists(SessionFile))
                File.Delete(SessionFile);
        }

        public void ExportElectionsTo(string filePath, IEnumerable<Election> elections)
        {
            string json = JsonSerializer.Serialize(elections, JsonOptions);
            File.WriteAllText(filePath, json);
        }

        public List<Election>? ImportElectionsFrom(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<Election>>(json, options);
        }
    }
}