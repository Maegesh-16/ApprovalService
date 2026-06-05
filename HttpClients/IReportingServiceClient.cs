namespace ApprovalService.API.HttpClients
{
    public interface IReportingServiceClient
    {
        /// <summary>
        /// Updates only the status of an action in the ReportingService snapshot DB.
        /// Called after ApprovalService changes action status in ActionService.
        /// </summary>
        Task UpdateActionStatusAsync(int actionId, int status);
    }
}
