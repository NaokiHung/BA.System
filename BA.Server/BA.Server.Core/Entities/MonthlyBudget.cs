using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BA.Server.Entities
{
    public class MonthlyBudget
    {
        [Required]
        public required string UserId { get; set; }
        
        public int Year { get; set; }
        
        [Range(1, 12)]
        public int Month { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedDate { get; set; }
    }
}