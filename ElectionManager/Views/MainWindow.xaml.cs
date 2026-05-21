using ElectionManager.Data;
using ElectionManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Windows.Threading;

namespace ElectionManager.Views
{
    /// <summary>
    /// Головне вікно застосунку. Забезпечує відображення списку виборчих кампаній, 
    /// керування ними (CRUD) та загальну взаємодію користувача з системою.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IRepository _repository;
        private readonly List<Voter> _voters;
        private readonly ObservableCollection<Election> _elections;
        private IUser _currentUser;
        private DispatcherTimer _timer;

        public MainWindow(IRepository repository, List<Voter> voters, 
                          List<Election> elections, IUser currentUser)
        {
            InitializeComponent();

            _repository  = repository;
            _voters      = voters;
            _currentUser = currentUser;

            _elections = new ObservableCollection<Election>(elections);
            ElectionsGrid.ItemsSource = _elections;

            UpdateStatusBar();
            UpdateAdminButtons();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);     
            _timer.Tick += (s, e) =>
            {
                ElectionsGrid.Items.Refresh();
                UpdateVoteButtonState();
            };
            _timer.Start();
        }

        private void UpdateStatusBar()
        {
            TxtStatusBarUser.Text = _currentUser.FullName;

            if (!_currentUser.IsAuthenticated)
            {
                TxtStatusBarUser.Foreground = Brushes.Gray;
                TxtStatusRole.Text = "Гість";
                TxtStatusRole.Foreground = Brushes.Gray;
            }
            else
            {
                TxtStatusBarUser.Foreground = Brushes.DarkGreen;
                TxtStatusRole.Text = _currentUser.IsAdmin ? "Адміністратор" : "Виборець";
                TxtStatusRole.Foreground = _currentUser.IsAdmin ? Brushes.DarkBlue : Brushes.DarkGray;
            }
        }

        private void UpdateAdminButtons()
        {
            bool isAdmin = _currentUser.IsAdmin;

            BtnAddElection.IsEnabled = isAdmin;

            bool hasSelection = ElectionsGrid.SelectedItem is Election;
            bool canEdit = false;

            if (hasSelection)
            {
                var selected = (Election)ElectionsGrid.SelectedItem;
                canEdit = selected.StartDate > DateTime.Now && !selected.Ballots.Any();
            }

            BtnEditElection.IsEnabled = isAdmin && canEdit;           
            BtnDeleteElection.IsEnabled = isAdmin && hasSelection;       

            BtnShowResults.IsEnabled = hasSelection;
        }

        private void ElectionsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ElectionsGrid.SelectedItem is Election selected)
            {
                GrpElectionDetails.IsEnabled = true;
                TxtElectionTitle.Text        = selected.Title;
                TxtElectionDates.Text =
                    $"Період: з {selected.StartDate:dd.MM.yyyy HH:mm} " +
                    $"по {selected.EndDate:dd.MM.yyyy HH:mm}";

                CandidatesGrid.ItemsSource = selected.Candidates;
            }
            else
            {
                GrpElectionDetails.IsEnabled = false;
                CandidatesGrid.ItemsSource   = null;
                TxtElectionTitle.Text        = "Оберіть голосування зліва";
                TxtElectionDates.Text        = string.Empty;
            }

            UpdateAdminButtons();
            UpdateVoteButtonState();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = TxtSearch.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(query))
            {
                ElectionsGrid.ItemsSource = _elections;
            }
            else
            {
                var filtered = _elections.Where(el => el.Title.ToLower().Contains(query)).ToList();
                ElectionsGrid.ItemsSource = filtered;
            }
        }

        #region Обробники натискання кнопок інтерфейсу
        
        private void BtnAddElection_Click(object sender, RoutedEventArgs e)
        {
            var win = new ElectionWindow { Owner = this };
            if (win.ShowDialog() != true) return;

            win.ResultElection.Id = _elections.Any()
                ? _elections.Max(el => el.Id) + 1
                : 1;

            _elections.Add(win.ResultElection);
            _repository.SaveElections(_elections);
        }

        private void BtnEditElection_Click(object sender, RoutedEventArgs e)
        {
            if (ElectionsGrid.SelectedItem is not Election selected) return;

            var win = new ElectionWindow(selected) { Owner = this };
            if (win.ShowDialog() != true) return;

            int index = _elections.IndexOf(selected);
            _elections[index] = win.ResultElection;

            _repository.SaveElections(_elections);
        }

        private void BtnDeleteElection_Click(object sender, RoutedEventArgs e)
        {
            if (ElectionsGrid.SelectedItem is not Election selected) return;

            var confirm = MessageBox.Show(
                $"Видалити голосування «{selected.Title}»?\n" +
                "Всі бюлетені цього голосування також будуть видалені.",
                "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            _elections.Remove(selected);
            _repository.SaveElections(_elections);
        }

        private void BtnVote_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentUser.IsAuthenticated) return;

            if (ElectionsGrid.SelectedItem is not Election selected ||
                CandidatesGrid.SelectedItem is not Candidate candidate)
            {
                MessageBox.Show("Оберіть варіант для голосування.",
                    "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Віддати голос за: «{candidate.FullName}»?\n" +
                "Цю дію неможливо скасувати.",
                "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                selected.RegisterVote(_currentUser.Id, candidate.Id);

                _repository.SaveElections(_elections);

                MessageBox.Show("Ваш голос успішно враховано!",
                    "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                CandidatesGrid.SelectedItem = null;
                UpdateAdminButtons();
                UpdateVoteButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Допоміжний метод для оновлення стану кнопки голосування.
        private void UpdateVoteButtonState()
        {
            if (!_currentUser.IsAuthenticated || ElectionsGrid.SelectedItem is not Election selected)
            {
                BtnVote.IsEnabled = false;
                BtnVote.Content = "Віддати голос";
                return;
            }

            if (!selected.IsActive)
            {
                BtnVote.IsEnabled = false;
                BtnVote.Content = "Голосування закрите";
                return;
            }

            if (selected.HasVoted(_currentUser.Id))
            {
                BtnVote.IsEnabled = false;
                BtnVote.Content = "Ваш голос враховано";
                return;
            }

            BtnVote.IsEnabled = true;
            BtnVote.Content = "Віддати голос";
        }

        private void BtnShowResults_Click(object sender, RoutedEventArgs e)
        {
            if (ElectionsGrid.SelectedItem is not Election selected) return;

            string results = selected.CalculateResults();
            MessageBox.Show(results,
                $"Результати: {selected.Title}",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Обробники натискання кнопок меню

        private void MenuChangeAccount_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Вийти з поточного акаунту?",
                "Вихід", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            if (_currentUser is Voter realVoter)
            {
                realVoter.SessionToken = null;
            }

            _repository.SaveVoters(_voters);
            _repository.ClearSession();

            string? path = Environment.ProcessPath;
            if (path != null)
                System.Diagnostics.Process.Start(path);

            Application.Current.Shutdown();
        }

        private void MenuExportDb_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON файли (*.json)|*.json|Усі файли (*.*)|*.*",
                FileName = "ElectionsBackup.json",
                Title = "Експортувати базу голосувань"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _repository.ExportElectionsTo(saveFileDialog.FileName, _elections);

                    MessageBox.Show($"Базу успішно збережено у файл:\n{saveFileDialog.FileName}",
                        "Експорт завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuImportDb_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON файли (*.json)|*.json|Усі файли (*.*)|*.*",
                Title = "Імпортувати базу голосувань"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var confirm = MessageBox.Show(
                    "УВАГА! Поточні голосування будуть замінені даними з файлу.\nВи дійсно хочете продовжити?",
                    "Підтвердження імпорту", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    var importedElections = _repository.ImportElectionsFrom(openFileDialog.FileName);

                    if (importedElections != null)
                    {
                        _elections.Clear();
                        foreach (var el in importedElections) _elections.Add(el);
                        _repository.SaveElections(_elections);

                        MessageBox.Show("Базу успішно завантажено!", "Імпорт завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка читання файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow { Owner = this }.ShowDialog();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            Application.Current.Shutdown();
        }

        #endregion
    }
}