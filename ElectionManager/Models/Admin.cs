namespace ElectionManager.Models
{
    /// <summary>
    /// Модель адміністратора системи. Розширює базові права виборця можливістю керувати виборчими кампаніями.
    /// </summary>
    public class Admin : Voter
    {
        public Admin()
        {
            IsAdmin = true;
        }
    }
}