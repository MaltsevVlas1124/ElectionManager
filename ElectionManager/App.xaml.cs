using System.Linq;
using System.Windows;
using ElectionManager.Data;
using ElectionManager.Models;
using ElectionManager.Views;

namespace ElectionManager
{
    /// <summary>
    /// Головний клас застосунку. Виконує роль Composition Root: 
    /// перевіряє цілісність БД, керує сесіями та впроваджує залежності у вікна.
    /// </summary>
    public partial class App : Application
    {
        private const string AdminCodeHash = "c0d7c803e01b2ff0f3dce76f540821e0df1a0d7f70d4f9ff35fce173be2b2478";
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Ініціалізація бази даних та перевірка цілісності
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

            // Маршрутизація та авторизація (Автологін або LoginWindow)
            IUser currentUser = new Guest();

            string? token = repository.LoadSession();
            if (token != null)
            {
                var found = voters.FirstOrDefault(v => v.SessionToken == token);
                if (found != null) currentUser = found;
            }

            if (!currentUser.IsAuthenticated)
            {
                var loginWin = new LoginWindow(voters, repository, AdminCodeHash);

                if (loginWin.ShowDialog() == true && loginWin.AuthenticatedVoter is IUser authUser)
                {
                    currentUser = authUser;
                    repository.SaveVoters(voters);
                }
                else
                {
                    Shutdown();
                    return;
                }
            }
            
            // Запуск головного інтерфейсу
            Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            
            var mainWindow = new MainWindow(repository, voters, elections, currentUser);
            mainWindow.Show();
            }
        }
    }
