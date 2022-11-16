using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using MoneyTracker.Areas.Identity.Data;

namespace MoneyTracker.Models
{
    [Index(nameof(Name), IsUnique = true, Name = "SUBCATEGORY_UNIQUE_NAME")]
    public class SubCategory
    {
        [Key]
        public int SubCategoryId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        [MaxLength(150)]
        public string Description { get; set; }

        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order must be between 1 and 100 only!!")]
        public int DisplayOrder { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public Category Category { get; set; }

        public string? OwnerId { get; set; }

        [DisplayName("Record Status")]
        public int RecordStatus { get; set; } = 1;

        public DateTime RecordStatusDate { get; set; } = DateTime.Now;
    }
}
