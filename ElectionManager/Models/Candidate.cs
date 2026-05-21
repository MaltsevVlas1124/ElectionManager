namespace ElectionManager.Models
{
    /// <summary>
    /// Сутність, що описує варіант для голосування (тобто кандидата)
    /// </summary>
    public class Candidate
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Information { get; set; } = string.Empty;
    }
}