using System;
using System.Collections.Generic;
using System.Text;

namespace ElectionManager.Models
{
    public class Guest : IUser
    {
        public int Id => -1;
        public string FullName => "Гість (Лише перегляд)";
        public bool IsAdmin => false;
        public bool IsAuthenticated => false;
    }
}