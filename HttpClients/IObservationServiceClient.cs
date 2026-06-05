namespace ApprovalService.API.HttpClients
{
    public class ObservationDetailsResponse
    {
        public int ObservationId { get; set; }
        public int AuditId { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public interface IObservationServiceClient
    {
        Task<ObservationDetailsResponse?> GetObservationAsync(int observationId);
        Task<List<ObservationDetailsResponse>> GetObservationsByAuditAsync(int auditId);
    }
}
