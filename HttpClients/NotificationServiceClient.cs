using System.Text;
using System.Text.Json;

namespace ApprovalService.API.HttpClients
{
    public class NotificationServiceClient : INotificationServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationServiceClient> _logger;

        public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendApprovalRequestNotificationAsync(int recipientUserId, string entityTypeName, int entityId)
        {
            try
            {
                var payload = new
                {
                    RecipientUserId = recipientUserId,
                    Title = "Approval Requested",
                    Type = 0, // ApprovalRequested
                    EntityType = GetEntityTypeValue(entityTypeName),
                    EntityId = entityId,
                    Message = $"New approval request for {entityTypeName} (ID: {entityId}) requires your review."
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/notification", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("NotificationService returned {StatusCode} when sending approval request notification for {EntityType} ID: {EntityId}",
                        response.StatusCode, entityTypeName, entityId);
                }
                else
                {
                    _logger.LogInformation("Approval request notification sent successfully to UserId: {UserId} for {EntityType} ID: {EntityId}",
                        recipientUserId, entityTypeName, entityId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send approval request notification for {EntityType} ID: {EntityId}", entityTypeName, entityId);
            }
        }

        public async Task SendApprovalResultNotificationAsync(int recipientUserId, int approvalId, bool isApproved, string entityTypeName)
        {
            try
            {
                var payload = new
                {
                    RecipientUserId = recipientUserId,
                    Title = isApproved ? "Approved" : "Rejected",
                    Type = isApproved ? 1 : 2, // ApprovalApproved (1) or ApprovalRejected (2)
                    EntityType = GetEntityTypeValue(entityTypeName),
                    EntityId = approvalId,
                    Message = isApproved 
                        ? $"Your approval request for {entityTypeName} (ID: {approvalId}) has been approved."
                        : $"Your approval request for {entityTypeName} (ID: {approvalId}) has been rejected."
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/notification", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("NotificationService returned {StatusCode} when sending approval result notification for ApprovalId: {ApprovalId}",
                        response.StatusCode, approvalId);
                }
                else
                {
                    _logger.LogInformation("Approval result notification sent successfully to UserId: {UserId} for ApprovalId: {ApprovalId}",
                        recipientUserId, approvalId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send approval result notification for ApprovalId: {ApprovalId}", approvalId);
            }
        }

        private static int GetEntityTypeValue(string entityTypeName)
        {
            return entityTypeName.ToLower() switch
            {
                "audit" => 0,
                "observation" => 1,
                "action" => 2,
                _ => 2 // Default to Action
            };
        }
    }
}
