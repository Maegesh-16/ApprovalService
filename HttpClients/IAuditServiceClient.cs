namespace ApprovalService.API.HttpClients
{
    public class AuditDetailsResponse
    {
        public int AuditId { get; set; }
        public string? Title { get; set; }
        public int AuditorId { get; set; }
        public int DepartmentId { get; set; }
        public int StatusId { get; set; }
    }

    public class UpdateAuditStatusPayload
    {
        public int StatusId { get; set; } // 3 = Closed
    }

    public interface IAuditServiceClient
    {
        Task<AuditDetailsResponse?> GetAuditAsync(int auditId);
        Task UpdateAuditStatusAsync(int auditId, UpdateAuditStatusPayload payload);
    }
}
