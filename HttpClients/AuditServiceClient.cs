using System.Text;
using System.Text.Json;

namespace ApprovalService.API.HttpClients
{
    public class AuditServiceClient : IAuditServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuditServiceClient> _logger;

        public AuditServiceClient(HttpClient httpClient, ILogger<AuditServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AuditDetailsResponse?> GetAuditAsync(int auditId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/audits/{auditId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get audit {AuditId}. Status: {StatusCode}", 
                        auditId, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<AuditDetailsResponse>(content, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit {AuditId} from AuditService", auditId);
                return null;
            }
        }

        public async Task UpdateAuditStatusAsync(int auditId, UpdateAuditStatusPayload payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"/api/audits/{auditId}/status", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to update audit {AuditId} status. Status: {StatusCode}", 
                        auditId, response.StatusCode);
                }
                else
                {
                    _logger.LogInformation("Successfully updated audit {AuditId} status to {StatusId}", 
                        auditId, payload.StatusId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating audit {AuditId} status in AuditService", auditId);
                throw;
            }
        }
    }
}
