using System.Collections.Generic;
using ElectionManager.Models;

namespace ElectionManager.Data
{
    /// <summary>
    /// Контракт для роботи зі сховищем даних. 
    /// Абстрагує бізнес-логіку від конкретної реалізації бази даних (Dependency Inversion).
    /// </summary>
    public interface IRepository
    {
        List<Election> LoadElections(out bool wasCorrupted);
        void SaveElections(IEnumerable<Election> elections);
        
        List<Voter> LoadVoters(out bool wasCorrupted);
        void SaveVoters(IEnumerable<Voter> voters);

        string? LoadSession();
        void SaveSession(string token);
        void ClearSession();
        void ExportElectionsTo(string filePath, IEnumerable<Election> elections);
        
        List<Election>? ImportElectionsFrom(string filePath);
    }
}