using System.Linq;

namespace ElectionManager.Models
{
    public class MajorityElection : Election
    {
        public override string CalculateResults()
        {
            if (!Ballots.Any()) return "Голосів ще немає.";

            var groupedVotes = Ballots.GroupBy(b => b.CandidateId)
                .Select(g => new { CandidateId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            int maxVotes = groupedVotes.First().Count;
            var winners = groupedVotes.Where(x => x.Count == maxVotes).ToList();

            if (winners.Count > 1)
            {
                return $"Нічия! По {maxVotes} голосів набрала наступна кількість кандидатів: {winners.Count}";
            }

            var winner = Candidates.FirstOrDefault(c => c.Id == winners.First().CandidateId);
            return winner != null
                ? $"Переможець: {winner.FullName} ({maxVotes} з {Ballots.Count} голосів)"
                : "Помилка підрахунку результатів.";
        }
    }
}