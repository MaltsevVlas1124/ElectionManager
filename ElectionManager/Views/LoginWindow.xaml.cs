using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ElectionManager.Data;
using ElectionManager.Models;
using System.Windows.Input;

namespace ElectionManager.Views
{
    public partial class LoginWindow : Window
    {
        private readonly List<Voter> _voters;
        private readonly IRepository _repository;

        public IUser? AuthenticatedVoter { get; private set; }

        public LoginWindow(List<Voter> voters, IRepository repository)
        {
            InitializeComponent();
            _voters = voters;
            _repository = repository;
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            AuthenticatedVoter = new Guest();
            DialogResult = true;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заповніть логін та пароль.",
                    "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingVoter = _voters.FirstOrDefault(v => v.Login == login);

            if (existingVoter != null)
                TryLogin(existingVoter, password);
            else
                TryRegister(login, password);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TryLogin(Voter voter, string password)
        {
            if (!voter.VerifyPassword(password))
            {
                MessageBox.Show("Невірний пароль.",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AuthenticatedVoter = voter;
            IssueSessionToken(voter);
            DialogResult = true;
        }

        private void TryRegister(string login, string password)
        {
            var confirm = MessageBox.Show(
                $"Логін «{login}» не знайдено у системі.\nЗареєструвати новий акаунт?",
                "Реєстрація", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            var nameWin = new NameWindow { Owner = this };
            if (nameWin.ShowDialog() != true) return;

            string inputAdminCode = TxtAdminCode.Text.Trim();
            string adminCodeHash = "c0d7c803e01b2ff0f3dce76f540821e0df1a0d7f70d4f9ff35fce173be2b2478";

            Voter newVoter;
            if (!string.IsNullOrEmpty(inputAdminCode) && Voter.HashPassword(inputAdminCode) == adminCodeHash)
                newVoter = new Admin();
            else
                newVoter = new Voter(); ;

            newVoter.Id = _voters.Count > 0 ? _voters.Max(v => v.Id) + 1 : 1;
            newVoter.FullName = nameWin.FullName;
            newVoter.SetCredentials(login, password);

            _voters.Add(newVoter);

            try
            {
                _repository.SaveVoters(_voters);
                IssueSessionToken(newVoter);

                AuthenticatedVoter = newVoter;

                string role = newVoter.IsAdmin ? "адміністратора" : "виборця";
                MessageBox.Show($"Ласкаво просимо, {newVoter.FullName}!\nВас зареєстровано як {role}.",
                    "Вітаємо", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                _voters.Remove(newVoter);

                MessageBox.Show($"Критична помилка бази даних!\nНе вдалося зберегти ваш профіль. Спробуйте ще раз.\nДеталі: {ex.Message}",
                    "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        private int _secretClickCount = 0;

        private void Secret_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PanelSecretAdmin.Visibility == Visibility.Visible) return;
            
            _secretClickCount++;
            if (_secretClickCount == 2)
            {
                TxtSecretHint.Text = "Ну і чого його оце клацать?";
                TxtSecretHint.Visibility = Visibility.Visible;
            }
            else if (_secretClickCount == 3)
            {
                TxtSecretHint.Text = "Ну і, шо далі?";
                TxtSecretHint.Visibility = Visibility.Visible;
            }
            else if (_secretClickCount == 4)
            {
                TxtSecretHint.Text = "Зупинись...";
            }
            else if (_secretClickCount == 5)
            {
                TxtSecretHint.Text = "Розбирайся сам(а). Підсказок не дам.";
                PanelSecretAdmin.Visibility = Visibility.Visible;

            }
        }

        private void IssueSessionToken(Voter voter)
        {
            voter.SessionToken = Guid.NewGuid().ToString();
            _repository.SaveSession(voter.SessionToken);
        }
    }
}