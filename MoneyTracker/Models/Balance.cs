using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Models
{
    public class Balance
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public float Value { get; set; }
        [Required]
        [DisplayName("Date of balance")]
        public DateTime Date { get; set; } = DateTime.Now;
        public string? OwnerId { get; set; }
        [DisplayName("Record status")]
        public int RecordStatus { get; set; } = 1;
        public DateTime RecordStatusDate { get; set; } = DateTime.Now;
    }
}
