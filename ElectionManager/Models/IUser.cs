using System;
using System.Collections.Generic;
using System.Text;

namespace ElectionManager.Models
{
    /// <summary>
    /// Загальний інтерфейс для всіх користувачів системи.
    /// Визначає базові параметри для автентифікації та перевірки прав доступу (RBAC).
    /// </summary>
    public interface IUser
    {
        int Id { get; }
        string FullName { get; }
        bool IsAdmin { get; }
        bool IsAuthenticated { get; }
    }
}
