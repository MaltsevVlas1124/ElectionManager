namespace ElectionManager.Models
{
    public class Admin : Voter
    {
        public Admin()
        {
            IsAdmin = true;
        }
    }
}