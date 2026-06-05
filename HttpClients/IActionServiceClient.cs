namespace ApprovalService.API.HttpClients
{
    public class UpdateActionStatusPayload
    {
        public int Status { get; set; } // 3 = Closed, 4 = Reopened
    }

    public class ActionSummaryResponse
    {
        public int ActionId { get; set; }
        public int ObservationId { get; set; }
        public int AssignedToUserId { get; set; }
        public int Status { get; set; }
    }

    public interface IActionServiceClient
    {
        Task UpdateActionStatusAsync(int actionId, UpdateActionStatusPayload payload);
        Task<ActionSummaryResponse?> GetActionAsync(int actionId);
        Task<List<ActionSummaryResponse>> GetActionsByObservationAsync(int observationId);
    }
}
