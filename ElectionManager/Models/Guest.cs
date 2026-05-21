using System;
using System.Collections.Generic;
using System.Text;

namespace ElectionManager.Models
{
    /// <summary>
    /// Клас-заглушка для неавторизованих користувачів (реалізація патерну Null Object).
    /// Забезпечує безпечний доступ до системи у режимі "лише перегляд".
    /// </summary>
    public class Guest : IUser
    {
        public int Id => -1;
        public string FullName => "Гість (Лише перегляд)";
        public bool IsAdmin => false;
        public bool IsAuthenticated => false;
    }
}