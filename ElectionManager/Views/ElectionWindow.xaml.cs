using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ElectionManager.Models;

namespace ElectionManager.Views
{
    /// <summary>
    /// Модальне вікно для створення та редагування виборчої кампанії. 
    /// Містить логіку валідації часових рамок та управління списком кандидатів.
    /// </summary>
    public partial class ElectionWindow : Window
    {
        public Election? ResultElection { get; private set; }
        private ObservableCollection<Candidate> _candidates = new ObservableCollection<Candidate>();
        private bool _isEditMode;

        public ElectionWindow(Election? electionToEdit = null)
        {
            InitializeComponent();
            LstCandidates.ItemsSource = _candidates;

            if (electionToEdit != null)
            {
                _isEditMode = true;
                TxtTitle.Text = electionToEdit.Title;
                DpStart.SelectedDate = electionToEdit.StartDate.Date;
                TxtStartTime.Text = electionToEdit.StartDate.ToString("HH:mm");
                DpEnd.SelectedDate = electionToEdit.EndDate.Date;
                TxtEndTime.Text = electionToEdit.EndDate.ToString("HH:mm");

                if (electionToEdit is MajorityElection) CmbType.SelectedIndex = 0;
                else CmbType.SelectedIndex = 1;
                CmbType.IsEnabled = false;

                foreach (var c in electionToEdit.Candidates) _candidates.Add(c);
                ResultElection = electionToEdit;
            }
            else
            {
                DateTime now = DateTime.Now;
                DateTime nextHour = now.Date.AddHours(now.Hour + 1);

                DpStart.SelectedDate = nextHour.Date;
                TxtStartTime.Text = nextHour.ToString("HH:00");

                DpEnd.SelectedDate = nextHour.Date.AddDays(1);
                TxtEndTime.Text = "20:00";

                CmbType.SelectedIndex = 0;
            }
        }

        private void DpStart_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DpStart.SelectedDate.HasValue && DpEnd.SelectedDate.HasValue)
            {
                if (DpStart.SelectedDate.Value > DpEnd.SelectedDate.Value)
                {
                    DpEnd.SelectedDate = DpStart.SelectedDate.Value;
                }
            }
        }

        #region Обробники натискання кнопок інтерфейсу

        private void BtnAddCandidate_Click(object sender, RoutedEventArgs e)
        {
            var win = new CandidateWindow { Owner = this };
            if (win.ShowDialog() == true)
            {
                win.ResultCandidate.Id = _candidates.Count > 0 ? _candidates.Max(c => c.Id) + 1 : 1;
                _candidates.Add(win.ResultCandidate);
            }
        }

        private void BtnEditCandidate_Click(object sender, RoutedEventArgs e)
        {
            if (LstCandidates.SelectedItem is Candidate selected)
            {
                var win = new CandidateWindow(selected) { Owner = this };
                if (win.ShowDialog() == true)
                    LstCandidates.Items.Refresh();
            }
        }

        private void BtnRemoveCandidate_Click(object sender, RoutedEventArgs e)
        {
            if (LstCandidates.SelectedItem is Candidate selected)
                _candidates.Remove(selected);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime start = DateTime.Parse($"{DpStart.SelectedDate:yyyy-MM-dd} {TxtStartTime.Text}");
                DateTime end = DateTime.Parse($"{DpEnd.SelectedDate:yyyy-MM-dd} {TxtEndTime.Text}");

                if (string.IsNullOrWhiteSpace(TxtTitle.Text))
                {
                    MessageBox.Show("Будь ласка, введіть назву виборчої кампанії.",
                        "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (end <= start)
                {
                    MessageBox.Show("Дата завершення голосування має бути пізнішою за дату початку.",
                        "Логічна помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!_isEditMode && start < DateTime.Now)
                {
                    MessageBox.Show("Дата початку голосування не може бути у минулому.",
                        "Логічна помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_candidates.Count < 2)
                {
                    MessageBox.Show("Для проведення голосування необхідно додати щонайменше двох кандидатів (варіантів).",
                        "Недостатньо кандидатів", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!_isEditMode)
                {
                    if (CmbType.SelectedIndex == 0) ResultElection = new MajorityElection();
                    else ResultElection = new ProportionalElection();
                }

                ResultElection.Title = TxtTitle.Text.Trim();
                ResultElection.StartDate = start;
                ResultElection.EndDate = end;
                ResultElection.Candidates = _candidates.ToList();

                this.DialogResult = true;
            }
            catch (FormatException)
            {
                MessageBox.Show("Неправильний формат часу. Використовуйте формат ГГ:ХХ (наприклад, 23:59)",
                    "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        #endregion
    }
}