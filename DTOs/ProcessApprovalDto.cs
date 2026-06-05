using ApprovalService.API.Enums;

namespace ApprovalService.API.DTOs
{
    public class ProcessApprovalDto
    {
        public int ActionByUserId { get; set; }
        public ApprovalAction Action { get; set; }
        public string? Comments { get; set; }
    }
}
