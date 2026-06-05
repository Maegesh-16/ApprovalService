using ApprovalService.API.Enums;

namespace ApprovalService.API.DTOs
{
    public class CreateApprovalRequestDto
    {
        public EntityType EntityType { get; set; }
        public int EntityId { get; set; }
        public ApprovalType ApprovalType { get; set; }
        public int RequestedByUserId { get; set; }
        public int ApproverUserId { get; set; }
    }
}
