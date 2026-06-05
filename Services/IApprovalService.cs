using ApprovalService.API.DTOs;
using ApprovalService.API.Enums;

namespace ApprovalService.API.Services
{
    public interface IApprovalService
    {
        Task<IEnumerable<ApprovalRequestResponseDto>> GetAllApprovalsAsync();
        Task<ApprovalRequestResponseDto?> GetApprovalByIdAsync(int approvalId);
        Task<IEnumerable<ApprovalRequestResponseDto>> GetApprovalsByEntityAsync(EntityType entityType, int entityId);
        Task<IEnumerable<ApprovalRequestResponseDto>> GetPendingApprovalsAsync();
        Task<IEnumerable<ApprovalRequestResponseDto>> GetApprovalsByRequesterAsync(int userId);
        Task<IEnumerable<ApprovalRequestResponseDto>> GetApprovalsByApproverAsync(int userId);
        Task<ApprovalRequestResponseDto> CreateApprovalRequestAsync(CreateApprovalRequestDto dto);
        Task<ApprovalRequestResponseDto?> ProcessApprovalAsync(int approvalId, ProcessApprovalDto dto);
        Task<IEnumerable<ApprovalHistoryResponseDto>> GetApprovalHistoryAsync(int approvalId);
    }
}
