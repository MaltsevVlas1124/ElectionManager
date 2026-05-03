using System;

namespace ElectionManager.Models
{
    public class Voter
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;

        private string _passportNumber = string.Empty;

        public void SetPassport(string passport)
        {
            if (string.IsNullOrWhiteSpace(passport))
                throw new ArgumentException("Ерор - де паспор?");

            _passportNumber = passport;
        }

        public bool VerifyPassport(string passportToMatch)
        {
            return _passportNumber == passportToMatch;
        }
    }
}