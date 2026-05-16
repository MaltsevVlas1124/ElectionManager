using System;
using System.Text.Json.Serialization;

namespace ElectionManager.Models
{
    public class Ballot
    {
        public int Id { get; set; }
        public int ElectionId { get; set; }
        public int VoterId { get; init; }
        public int CandidateId { get; init; }
        public DateTime VotingTime { get; init; }

        [JsonConstructor]
        public Ballot() { }

        public Ballot(int electionId, int voterId, int candidateId)
        {
            ElectionId = electionId;
            VoterId = voterId;
            CandidateId = candidateId;
            VotingTime = DateTime.Now;
        }
    }
}