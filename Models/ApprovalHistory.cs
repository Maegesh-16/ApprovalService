using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApprovalService.API.Enums;

namespace ApprovalService.API.Models
{
    public class ApprovalHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public int ApprovalId { get; set; }

        [Required]
        public int ActionByUserId { get; set; }

        [Required]
        public ApprovalAction Action { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(ApprovalId))]
        public ApprovalRequest ApprovalRequest { get; set; } = null!;
    }
}
