using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Models
{
    [Index(nameof(Name), IsUnique = true, Name = "CATEGOR_UNIQUE_NAME")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        [MaxLength(150)]
        public string Description { get; set; }

        [DisplayName("Display Order")]
        [Range(1,100,ErrorMessage = "Display Order must be between 1 and 100 only!!")]
        public int DisplayOrder { get; set; }

        public ICollection<SubCategory> SubCategories { get; set; }

        [DisplayName("Record Status")]
        public int RecordStatus { get; set; } = 1;

        public DateTime RecordStatusDate { get; set;} = DateTime.Now;

    }
}
