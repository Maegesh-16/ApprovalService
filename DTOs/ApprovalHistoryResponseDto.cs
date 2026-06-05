using ApprovalService.API.Enums;

namespace ApprovalService.API.DTOs
{
    public class ApprovalHistoryResponseDto
    {
        public int HistoryId { get; set; }
        public int ApprovalId { get; set; }
        public int ActionByUserId { get; set; }
        public ApprovalAction Action { get; set; }
        public string ActionName => Action.ToString();
        public string? Comments { get; set; }
        public DateTime ActionDate { get; set; }
    }
}
