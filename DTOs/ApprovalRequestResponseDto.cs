using ApprovalService.API.Enums;

namespace ApprovalService.API.DTOs
{
    public class ApprovalRequestResponseDto
    {
        public int ApprovalId { get; set; }
        public EntityType EntityType { get; set; }
        public string EntityTypeName => EntityType.ToString();
        public int EntityId { get; set; }
        public ApprovalType ApprovalType { get; set; }
        public string ApprovalTypeName => ApprovalType.ToString();
        public int RequestedByUserId { get; set; }
        public int ApproverUserId { get; set; }
        public ApprovalStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
