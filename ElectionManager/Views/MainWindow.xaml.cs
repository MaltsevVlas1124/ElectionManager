using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ElectionManager.Models;

namespace ElectionManager.Views
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Election> _elections;
        private ObservableCollection<Voter> _voters;
        private Voter _currentUser = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeData();

            ElectionsGrid.ItemsSource = _elections;
            UpdateSessionUI();
        }

        private void InitializeData()
        {
            _voters = new ObservableCollection<Voter>();
            _elections = new ObservableCollection<Election>();
        }

        private void CandidatesGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void UpdateSessionUI()
        {
            if (_currentUser != null)
            {
                TxtStatusBarUser.Text = _currentUser.FullName;
                TxtStatusBarUser.Foreground = Brushes.DarkGreen;
            }
            else
            {
                TxtStatusBarUser.Text = "Не авторизовано";
                TxtStatusBarUser.Foreground = Brushes.DarkRed;

                GrpElectionDetails.IsEnabled = false;
                ElectionsGrid.SelectedItem = null;
            }
        }

        private void BtnChangeAccount_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                var result = MessageBox.Show("Вийти з аккаунту?", "Вихід", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _currentUser = null;
                    UpdateSessionUI();
                }
                return;
            }

            var loginWin = new LoginWindow(_voters.ToList()) { Owner = this };
            if (loginWin.ShowDialog() == true)
            {
                _currentUser = loginWin.AuthenticatedVoter;

                if (!_voters.Any(v => v.Id == _currentUser.Id))
                {
                    _voters.Add(_currentUser);
                }
                UpdateSessionUI();
            }
        }

        private void ElectionsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ElectionsGrid.SelectedItem is Election selectedElection)
            {
                BtnEditElection.IsEnabled = true;
                BtnShowResults.IsEnabled = true;

                if (_currentUser != null)
                {
                    GrpElectionDetails.IsEnabled = true;
                    TxtElectionTitle.Text = selectedElection.Title;
                    TxtElectionDates.Text = $"Період: з {selectedElection.StartDate:dd.MM.yyyy HH:mm} по {selectedElection.EndDate:dd.MM.yyyy HH:mm}";
                    CandidatesGrid.ItemsSource = selectedElection.Candidates;
                }
            }
            else
            {
                BtnEditElection.IsEnabled = false;
                BtnShowResults.IsEnabled = false;
                GrpElectionDetails.IsEnabled = false;
                CandidatesGrid.ItemsSource = null;
            }
        }

        private void BtnAddElection_Click(object sender, RoutedEventArgs e)
        {
            var win = new ElectionWindow { Owner = this };
            if (win.ShowDialog() == true)
            {
                win.ResultElection.Id = _elections.Any() ? _elections.Max(el => el.Id) + 1 : 1;
                _elections.Add(win.ResultElection);
                ElectionsGrid.Items.Refresh();
            }
        }

        private void BtnEditElection_Click(object sender, RoutedEventArgs e)
        {
            if (ElectionsGrid.SelectedItem is Election selectedElection)
            {
                var win = new ElectionWindow(selectedElection) { Owner = this };
                if (win.ShowDialog() == true)
                {
                    ElectionsGrid.Items.Refresh();
                    ElectionsGrid_SelectionChanged(null, null);
                }
            }
        }

        private void BtnVote_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            if (ElectionsGrid.SelectedItem is not Election selectedElection ||
                CandidatesGrid.SelectedItem is not Candidate selectedCandidate)
            {
                MessageBox.Show("Оберіть кандидата", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Віддати голос за: {selectedCandidate.FullName}?\nНескасовуємо.",
                "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    var ballot = new Ballot(selectedElection.Id, _currentUser.Id, selectedCandidate.Id);
                    selectedElection.RegisterVote(ballot);

                    MessageBox.Show("Голос враховано", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    CandidatesGrid.SelectedItem = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ерор", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnShowResults_Click(object sender, RoutedEventArgs e)
        {
            if (ElectionsGrid.SelectedItem is Election selectedElection)
            {
                string results = selectedElection.CalculateResults();
                MessageBox.Show(results, $"Результати: {selectedElection.Title}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWin = new AboutWindow { Owner = this };
            aboutWin.ShowDialog();
        }
    }
}