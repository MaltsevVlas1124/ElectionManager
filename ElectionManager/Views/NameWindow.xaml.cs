using System.Windows;

namespace ElectionManager.Views
{
    /// <summary>
    /// Модальне вікно для обов'язкового введення імені або нікнейму
    /// нового користувача під час його першої реєстрації у системі.
    /// </summary>
    public partial class NameWindow : Window
    {
        public string FullName { get; private set; } = string.Empty;

        public NameWindow()
        {
            InitializeComponent();
        }
        
        #region Обробники натискання кнопок інтерфейсу
        
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFullName.Text))
            {
                MessageBox.Show("Поле не може бути порожнім.",
                    "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FullName = TxtFullName.Text.Trim();
            DialogResult = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult != true)
                e.Cancel = true;

            base.OnClosing(e);
        }

        #endregion
    }
}