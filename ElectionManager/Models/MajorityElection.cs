using System.Linq;

namespace ElectionManager.Models
{
    public class MajorityElection : Election
    {
        public override string CalculateResults()
        {
            if (!Ballots.Any())
                return "Голосів ще немає.";

            var winnerId = Ballots
                .GroupBy(b => b.CandidateId)
                .OrderByDescending(g => g.Count())
                .First().Key;

            var winner = Candidates.FirstOrDefault(c => c.Id == winnerId);
            int winnerVotes = Ballots.Count(b => b.CandidateId == winnerId);

            return winner != null
                ? $"Переможець: {winner.FullName} ({winnerVotes} з {Ballots.Count} голосів)"
                : "Помилка підрахунку результатів.";
        }
    }
}