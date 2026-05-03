using System;
using System.Linq;

namespace ElectionManager.Models
{
    public class ProportionalElection : Election
    {
        public override string CalculateResults()
        {
            if (!Ballots.Any()) return "Ерор - голосів немає";

            int total = Ballots.Count;
            var results = Ballots.GroupBy(b => b.CandidateId)
                                 .Select(g => new
                                 {
                                     Id = g.Key,
                                     Percent = Math.Round((double)g.Count() / total * 100, 2)
                                 })
                                 .OrderByDescending(r => r.Percent);

            string report = "Результати (у %):\n";
            foreach (var res in results)
            {
                var candidate = Candidates.FirstOrDefault(c => c.Id == res.Id);
                if (candidate != null)
                {
                    report += $"{candidate.FullName} ({candidate.Information}): {res.Percent}%\n";
                }
            }
            return report;
        }
    }
}