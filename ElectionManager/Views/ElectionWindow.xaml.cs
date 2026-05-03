using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ElectionManager.Models;

namespace ElectionManager.Views
{
    public partial class ElectionWindow : Window
    {
        public Election ResultElection { get; private set; }
        private ObservableCollection<Candidate> _candidates = new ObservableCollection<Candidate>();
        private bool _isEditMode;

        public ElectionWindow(Election electionToEdit = null)
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
                DpStart.SelectedDate = DateTime.Today;
                DpEnd.SelectedDate = DateTime.Today.AddDays(1);
                CmbType.SelectedIndex = 0;
            }
        }

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

                if (!_isEditMode)
                {
                    if (CmbType.SelectedIndex == 0) ResultElection = new MajorityElection();
                    else ResultElection = new ProportionalElection();
                }

                ResultElection.Title = TxtTitle.Text;
                ResultElection.StartDate = start;
                ResultElection.EndDate = end;
                ResultElection.Candidates = _candidates.ToList();

                this.DialogResult = true;
            }
            catch (FormatException)
            {
                MessageBox.Show("Ерор", "Ерор", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}