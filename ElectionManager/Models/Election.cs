using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectionManager.Models
{
    public abstract class Election
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual List<Candidate> Candidates { get; set; } = new List<Candidate>();
        public virtual List<Ballot> Ballots { get; set; } = new List<Ballot>();

        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;

        public bool HasVoted(int voterId)
        {
            return Ballots.Any(b => b.VoterId == voterId);
        }

        public void RegisterVote(Ballot ballot)
        {
            if (ballot == null) throw new ArgumentNullException(nameof(ballot));
            if (!IsActive) throw new InvalidOperationException("Ерор - закрито");
            if (HasVoted(ballot.VoterId)) throw new InvalidOperationException("Ерор - проголосовано");

            Ballots.Add(ballot);
        }

        public abstract string CalculateResults();
    }
}