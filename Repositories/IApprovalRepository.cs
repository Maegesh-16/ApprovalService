using ApprovalService.API.Enums;
using ApprovalService.API.Models;

namespace ApprovalService.API.Repositories
{
    public interface IApprovalRepository
    {
        Task<IEnumerable<ApprovalRequest>> GetAllAsync();
        Task<ApprovalRequest?> GetByIdAsync(int approvalId);
        Task<IEnumerable<ApprovalRequest>> GetByEntityAsync(EntityType entityType, int entityId);
        Task<IEnumerable<ApprovalRequest>> GetByStatusAsync(ApprovalStatus status);
        Task<IEnumerable<ApprovalRequest>> GetByRequestedByUserIdAsync(int userId);
        Task<IEnumerable<ApprovalRequest>> GetByApproverUserIdAsync(int userId);
        Task<ApprovalRequest> CreateAsync(ApprovalRequest request);
        Task UpdateAsync(ApprovalRequest request);
        Task AddHistoryAsync(ApprovalHistory history);
        Task<IEnumerable<ApprovalHistory>> GetHistoryByApprovalIdAsync(int approvalId);
    }
}
