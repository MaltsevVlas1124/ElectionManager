using System;
using System.Collections.Generic;
using System.Text;

namespace ElectionManager.Models
{
    public interface IUser
    {
        int Id { get; }
        string FullName { get; }
        bool IsAdmin { get; }
        bool IsAuthenticated { get; }
    }
}
