using System;
using System.Text.Json.Serialization;

namespace ElectionManager.Models
{
    /// <summary>
    /// Immutable запис факту відданого голосу.
    /// Використовує параметризований конструктор для гарантії цілісності даних.
    /// </summary>
    public class Ballot
    {
        public int Id { get; init; }
        public int ElectionId { get; init; }
        public int CandidateId { get; init; }
        public DateTime VotingTime { get; init; }

        /// <summary>
        /// Основний конструктор, який використовується парсером JSON для десеріалізації існуючого бюлетеня з файлу.
        /// Також гарантує, що бюлетень неможливо створити без повного набору даних.
        /// </summary>
        [JsonConstructor]
        public Ballot(int id, int electionId, int candidateId, DateTime votingTime)
        {
            Id = id;
            ElectionId = electionId;
            CandidateId = candidateId;
            VotingTime = votingTime;
        }

        /// <summary>
        /// Фабричний метод для безпечного створення нового бюлетеня під час голосування.
        /// Автоматично фіксує поточний час відданого голосу.
        /// </summary>
        public static Ballot CreateNewVote(int id, int electionId, int candidateId)
        {
            return new Ballot(id, electionId, candidateId, DateTime.Now);
        }
    }
}