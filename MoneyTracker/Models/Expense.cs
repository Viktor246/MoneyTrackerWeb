using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace MoneyTracker.Models
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public float Value { get; set; }

        [Required]
        [DisplayName("Date of expense")]
        public DateTime DateOfExpense { get; set; } = DateTime.Now;

        [ForeignKey("SubCategory")]
        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }

        public string? OwnerId { get; set; }

        [DisplayName("Record Status")]
        public int RecordStatus { get; set; } = 1;

        public DateTime RecordStatusDate { get; set; } = DateTime.Now;

    }
}
