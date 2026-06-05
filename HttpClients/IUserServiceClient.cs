namespace ApprovalService.API.HttpClients
{
    public interface IUserServiceClient
    {
        Task<UserDto?> GetUserAsync(int userId);
        Task<UserDto?> GetDepartmentHeadAsync(int departmentId);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName);
        Task<UserDto?> GetAuditManagerAsync(); // Get any Audit Manager (RoleId = 4)
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
}
