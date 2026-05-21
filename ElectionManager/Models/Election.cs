using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ElectionManager.Models
{

    /// <summary>
    /// Базовий абстрактний клас виборчої кампанії. 
    /// Інкапсулює спільний стан (дати, списки кандидатів та бюлетенів) і логіку правомочності голосування.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "ElectionType")]
    [JsonDerivedType(typeof(MajorityElection), "Majority")]
    [JsonDerivedType(typeof(ProportionalElection), "Proportional")]
    public abstract class Election
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Candidate> Candidates { get; set; } = new();
        public List<Ballot> Ballots { get; set; } = new();

        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;

        /// <summary>
        /// Перевіряє, чи брав даний виборець участь у цьому голосуванні.
        /// </summary>
        /// <param name="voterId">Унікальний ідентифікатор виборця.</param>
        /// <returns>Повертає true, якщо голос виборця вже зареєстровано.</returns>
        public bool HasVoted(int voterId)
        {
            return Ballots.Any(b => b.VoterId == voterId);
        }

        /// <summary>
        /// Реєструє бюлетень у поточному голосуванні після перевірки статусу виборів
        /// </summary>
        /// <param name="ballot">Об'єкт бюлетеня з ідентифікаторами виборця та кандидата.</param>
        /// <exception cref="ArgumentNullException">Викидається, якщо бюлетень порожній.</exception>
        /// <exception cref="InvalidOperationException">Викидається, якщо голосування неактивне або виборець вже проголосував.</exception>
        public void RegisterVote(Ballot ballot)
        {
            if (ballot == null)
                throw new ArgumentNullException(nameof(ballot));
            if (!IsActive)
                throw new InvalidOperationException(
                    "Голосування недоступне — завершене або ще не розпочате.");
            if (HasVoted(ballot.VoterId))
                throw new InvalidOperationException(
                    "Ваш голос у цьому голосуванні вже враховано.");

            Ballots.Add(ballot);
        }

        /// <summary>
        /// Абстрактний метод підрахунку результатів голосування. Реалізується поліморфно у класах-нащадках.
        /// </summary>
        /// <returns>Рядок із відформатованими результатами виборів для відображення.</returns>
        public abstract string CalculateResults();
    }
}