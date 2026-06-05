using System.Text.Json;

namespace ApprovalService.API.HttpClients
{
    public class ObservationServiceClient : IObservationServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ObservationServiceClient> _logger;

        public ObservationServiceClient(HttpClient httpClient, ILogger<ObservationServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ObservationDetailsResponse?> GetObservationAsync(int observationId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/observations/{observationId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("ObservationService returned {StatusCode} for ObservationId: {ObservationId}",
                        response.StatusCode, observationId);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ObservationDetailsResponse>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get observation {ObservationId}", observationId);
                return null;
            }
        }

        public async Task<List<ObservationDetailsResponse>> GetObservationsByAuditAsync(int auditId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/observations/audit/{auditId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("ObservationService returned {StatusCode} for AuditId: {AuditId}",
                        response.StatusCode, auditId);
                    return new List<ObservationDetailsResponse>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ObservationDetailsResponse>>(json, new JsonSerializerOptions 
                {
                    PropertyNameCaseInsensitive = true 
                }) ?? new List<ObservationDetailsResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get observations for audit {AuditId}", auditId);
                return new List<ObservationDetailsResponse>();
            }
        }
    }
}
