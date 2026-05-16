using System.Linq;
using System.Windows;
using ElectionManager.Data;
using ElectionManager.Models;
using ElectionManager.Views;

namespace ElectionManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var repository = new AppRepository();
            var voters = repository.LoadVoters(out bool votersDbCorrupted);
            var elections = repository.LoadElections(out bool electionsDbCorrupted);

            if (votersDbCorrupted || electionsDbCorrupted)
            {
                MessageBox.Show(
                    "Увага! Файли бази даних пошкоджені.\n\n" +
                    "Запуск програми скасовано.\n\n" +
                    "Необхідне ручне відновлення бази даних адміністратором.",
                    "Пошкодження бази даних",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                Shutdown();
                return;
            }

            IUser currentUser = new Guest();

            string? token = repository.LoadSession();
            if (token != null)
            {
                var found = voters.FirstOrDefault(v => v.SessionToken == token);
                if (found != null) currentUser = found;
            }

            if (!currentUser.IsAuthenticated)
            {
                var loginWin = new LoginWindow(voters, repository);

                if (loginWin.ShowDialog() == true)
                {
                    currentUser = loginWin.AuthenticatedVoter;
                    repository.SaveVoters(voters);
                }
                else
                {
                    Shutdown();
                    return;
                }
            }

            Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            var mainWindow = new MainWindow(repository, voters, elections, currentUser);
            mainWindow.Show();
            }
        }
    }
