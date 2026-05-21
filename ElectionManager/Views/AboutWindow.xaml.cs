using System.Windows;

namespace ElectionManager.Views
{
    /// <summary>
    /// Інформаційне вікно, що містить базові відомості про програму, 
    /// дисципліну та її розробника.
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}