using System.Text;
using System.Text.Json;

namespace ApprovalService.API.HttpClients
{
    public class ActionServiceClient : IActionServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ActionServiceClient> _logger;

        public ActionServiceClient(HttpClient httpClient, ILogger<ActionServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task UpdateActionStatusAsync(int actionId, UpdateActionStatusPayload payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/correctiveactions/{actionId}", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ActionService returned {StatusCode} when updating status for ActionId: {ActionId}",
                    response.StatusCode, actionId);
                throw new HttpRequestException($"ActionService call failed with status {response.StatusCode}");
            }
        }

        public async Task<ActionSummaryResponse?> GetActionAsync(int actionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/correctiveactions/{actionId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("ActionService returned {StatusCode} for ActionId: {ActionId}",
                        response.StatusCode, actionId);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ActionSummaryResponse>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get action {ActionId}", actionId);
                return null;
            }
        }

        public async Task<List<ActionSummaryResponse>> GetActionsByObservationAsync(int observationId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/correctiveactions/by-observation/{observationId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("ActionService returned {StatusCode} for ObservationId: {ObservationId}",
                        response.StatusCode, observationId);
                    return new List<ActionSummaryResponse>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ActionSummaryResponse>>(json, new JsonSerializerOptions 
                {
                    PropertyNameCaseInsensitive = true 
                }) ?? new List<ActionSummaryResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get actions for observation {ObservationId}", observationId);
                return new List<ActionSummaryResponse>();
            }
        }
    }
}
