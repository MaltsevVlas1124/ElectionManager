using System;
using System.Linq;
using System.Text;

namespace ElectionManager.Models
{
    public class ProportionalElection : Election
    {
        public override string CalculateResults()
        {
            if (!Ballots.Any())
                return "Голосів ще немає.";

            int total = Ballots.Count;
            var report = new StringBuilder();
            report.AppendLine($"Результати (всього голосів: {total}):\n");

            var results = Ballots
                .GroupBy(b => b.CandidateId)
                .Select(g => new
                {
                    Id = g.Key,
                    Count = g.Count(),
                    Percent = Math.Round((double)g.Count() / total * 100, 2)
                })
                .OrderByDescending(r => r.Percent);

            foreach (var res in results)
            {
                var candidate = Candidates.FirstOrDefault(c => c.Id == res.Id);
                if (candidate != null)
                    report.AppendLine(
                        $"{candidate.FullName}: {res.Percent}% ({res.Count} голосів)");
            }

            return report.ToString();
        }
    }
}