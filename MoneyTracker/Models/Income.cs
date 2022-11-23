using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker.Models
{
    public class Income
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public float Value { get; set; }

        [Required]
        [DisplayName("Date of income")]
        public DateTime Date { get; set; } = DateTime.Now;

        public string? OwnerId { get; set; }

        [DisplayName("Record Status")]
        public int RecordStatus { get; set; } = 1;

        public DateTime RecordStatusDate { get; set; } = DateTime.Now;
    }
}
