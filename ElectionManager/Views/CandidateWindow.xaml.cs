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
            ResultCandidate.FullName = TxtFullName.Text;
            ResultCandidate.Information = TxtInfo.Text;
            this.DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}