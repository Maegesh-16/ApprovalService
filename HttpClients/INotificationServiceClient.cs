namespace ApprovalService.API.HttpClients
{
    public interface INotificationServiceClient
    {
        Task SendApprovalRequestNotificationAsync(int recipientUserId, string entityTypeName, int entityId);
        Task SendApprovalResultNotificationAsync(int recipientUserId, int approvalId, bool isApproved, string entityTypeName);
    }
}
