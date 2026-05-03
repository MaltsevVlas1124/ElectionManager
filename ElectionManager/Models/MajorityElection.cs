using System.Linq;

namespace ElectionManager.Models
{
    public class MajorityElection : Election
    {
        public override string CalculateResults()
        {
            if (!Ballots.Any()) return "Ерор - голосів немає";

            var winnerId = Ballots.GroupBy(b => b.CandidateId)
                                  .OrderByDescending(g => g.Count())
                                  .First().Key;

            var winner = Candidates.FirstOrDefault(c => c.Id == winnerId);
            return winner != null ? $"Переможець: {winner.FullName}" : "Ерор - помилка підрахунку";
        }
    }
}