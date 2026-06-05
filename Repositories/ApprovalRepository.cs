using Microsoft.EntityFrameworkCore;
using ApprovalService.API.Data;
using ApprovalService.API.Enums;
using ApprovalService.API.Models;

namespace ApprovalService.API.Repositories
{
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly ApprovalDbContext _context;

        public ApprovalRepository(ApprovalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApprovalRequest>> GetAllAsync()
        {
            return await _context.ApprovalRequests
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<ApprovalRequest?> GetByIdAsync(int approvalId)
        {
            return await _context.ApprovalRequests
                .FirstOrDefaultAsync(a => a.ApprovalId == approvalId);
        }

        public async Task<IEnumerable<ApprovalRequest>> GetByEntityAsync(EntityType entityType, int entityId)
        {
            return await _context.ApprovalRequests
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApprovalRequest>> GetByStatusAsync(ApprovalStatus status)
        {
            return await _context.ApprovalRequests
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApprovalRequest>> GetByRequestedByUserIdAsync(int userId)
        {
            return await _context.ApprovalRequests
                .Where(a => a.RequestedByUserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApprovalRequest>> GetByApproverUserIdAsync(int userId)
        {
            return await _context.ApprovalRequests
                .Where(a => a.ApproverUserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<ApprovalRequest> CreateAsync(ApprovalRequest request)
        {
            _context.ApprovalRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task UpdateAsync(ApprovalRequest request)
        {
            request.UpdatedAt = DateTime.UtcNow;
            _context.ApprovalRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task AddHistoryAsync(ApprovalHistory history)
        {
            _context.ApprovalHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ApprovalHistory>> GetHistoryByApprovalIdAsync(int approvalId)
        {
            return await _context.ApprovalHistories
                .Where(h => h.ApprovalId == approvalId)
                .OrderByDescending(h => h.ActionDate)
                .ToListAsync();
        }
    }
}
