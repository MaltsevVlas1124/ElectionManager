using System;

namespace ElectionManager.Models
{
    public class Ballot
    {
        public int Id { get; set; }
        public int ElectionId { get; set; }
        public int VoterId { get; private set; }
        public int CandidateId { get; private set; }
        public DateTime VotingTime { get; private set; }

        public Ballot(int electionId, int voterId, int candidateId)
        {
            ElectionId = electionId;
            VoterId = voterId;
            CandidateId = candidateId;
            VotingTime = DateTime.Now;
        }
    }
}