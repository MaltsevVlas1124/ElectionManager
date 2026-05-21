using System.Windows;
using ElectionManager.Models;

namespace ElectionManager.Views
{
    /// <summary>
    /// Модальне вікно для додавання або редагування інформації про окремого кандидата (варіанту голосування).
    /// </summary>
    public partial class CandidateWindow : Window
    {
        public Candidate ResultCandidate { get; private set; }

        public CandidateWindow(Candidate? candidate = null)
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

        #region Обробники натискання кнопок інтерфейсу

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFullName.Text))
            {
                MessageBox.Show("Назва кандидата не може бути порожньою.",
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
        
        #endregion
    }
}