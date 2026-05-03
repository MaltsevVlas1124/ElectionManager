namespace ElectionManager.Models
{
    public class Candidate
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Information { get; set; } = string.Empty;
    }
}