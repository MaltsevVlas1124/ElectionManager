using System.Windows;
using ElectionManager.Models;

namespace ElectionManager.Views
{
    public partial class CandidateWindow : Window
    {
        public Candidate ResultCandidate { get; private set; }

        public CandidateWindow(Candidate candidate = null)
        {
            InitializeComponent();
            if (candidate != null)
            {
                TxtFullName.Text = candidate.FullName;
                TxtInfo.Text = candidate.Information;
                ResultCandidate = candidate;
            }
            else
            {
                ResultCandidate = new Candidate();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFullName.Text))
            {
                MessageBox.Show("Назва кандидата (ПІБ або партія) не може бути порожньою.",
                    "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResultCandidate.FullName = TxtFullName.Text.Trim();
            ResultCandidate.Information = TxtInfo.Text.Trim();
            this.DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}