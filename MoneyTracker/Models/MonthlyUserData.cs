﻿using System.Text.Json.Serialization;

namespace MoneyTracker.Models
{
    public class MonthlyUserData
    {
        public DateTime Date { get; set; }
        public float MonthlyExpense { get; set; }
        public float MonthlyIncome { get; set; }
        [JsonIgnore]
        public string Id { get; set; }
    }
}
