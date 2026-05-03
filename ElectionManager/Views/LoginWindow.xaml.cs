using System.Collections.Generic;
using System.Windows;
using ElectionManager.Models;

namespace ElectionManager.Views
{
    public partial class LoginWindow : Window
    {
        private List<Voter> _voters;

        public Voter AuthenticatedVoter { get; private set; }

        public LoginWindow(List<Voter> voters)
        {
            InitializeComponent();
            _voters = voters;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string fullName = TxtFullName.Text.Trim();
            string passport = TxtPassport.Password.Trim();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(passport))
            {
                MessageBox.Show("Заповніть усі поля", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newVoter = new Voter
            {
                Id = _voters.Count + 1,
                FullName = fullName
            };
            newVoter.SetPassport(passport);

            AuthenticatedVoter = newVoter;
            this.DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}