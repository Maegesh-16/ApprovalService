using System.Text;
using System.Text.Json;

namespace ApprovalService.API.HttpClients
{
    public class ReportingServiceClient : IReportingServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReportingServiceClient> _logger;

        public ReportingServiceClient(HttpClient httpClient, ILogger<ReportingServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task UpdateActionStatusAsync(int actionId, int status)
        {
            try
            {
                var payload = new { Status = status };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync($"api/report/actions/{actionId}/status", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Synced action status to ReportingService | ActionId: {ActionId} | Status: {Status}", actionId, status);
                }
                else
                {
                    _logger.LogWarning("ReportingService returned {StatusCode} when syncing ActionId: {ActionId} status. Skipping - non-critical.", response.StatusCode, actionId);
                }
            }
            catch (Exception ex)
            {
                // Non-critical: don't fail the whole approval flow if reporting sync fails
                _logger.LogError(ex, "Failed to sync action status to ReportingService for ActionId: {ActionId}. Approval already processed.", actionId);
            }
        }
    }
}
