using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BA.Server.Entities
{
    public class CashExpense
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public required string UserId { get; set; }
        
        public int Year { get; set; }
        
        [Range(1, 12)]
        public int Month { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(200)]
        public required string Description { get; set; }
        
        [MaxLength(50)]
        public required string Category { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        // 新增 UpdatedDate 屬性
        public DateTime? UpdatedDate { get; set; }
    }
}