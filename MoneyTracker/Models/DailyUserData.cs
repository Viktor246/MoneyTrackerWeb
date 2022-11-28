using System.Text.Json.Serialization;

namespace MoneyTracker.Models
{    public class DailyUserData
    {
        public DateTime Date { get; set; }
        public float DailyExpense { get; set; }
        [JsonIgnore]
        public float DailyIncome { get; set; }
        [JsonIgnore]
        public string Id { get; set; }

    }
}
