using System.ComponentModel.DataAnnotations;
using ApprovalService.API.Enums;

namespace ApprovalService.API.Models
{
    public class ApprovalRequest
    {
        [Key]
        public int ApprovalId { get; set; }

        [Required]
        public EntityType EntityType { get; set; }

        [Required]
        public int EntityId { get; set; }

        [Required]
        public ApprovalType ApprovalType { get; set; }

        [Required]
        public int RequestedByUserId { get; set; }

        [Required]
        public int ApproverUserId { get; set; }

        [Required]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<ApprovalHistory> ApprovalHistories { get; set; } = new List<ApprovalHistory>();
    }
}
