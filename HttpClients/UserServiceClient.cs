using System.Text.Json;

namespace ApprovalService.API.HttpClients
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;

        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserDto?> GetUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users/{userId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("UserService returned {StatusCode} when fetching user {UserId}", 
                        response.StatusCode, userId);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserResponse>>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (apiResponse?.Data == null) return null;

                var user = apiResponse.Data;
                return new UserDto
                {
                    UserId = user.UserId,
                    Name = user.FullName,
                    Email = user.Email,
                    RoleId = user.RoleId,
                    RoleName = user.RoleName ?? string.Empty,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.DepartmentName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch user {UserId} from UserService", userId);
                return null;
            }
        }

        public async Task<UserDto?> GetDepartmentHeadAsync(int departmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("UserService returned {StatusCode} when fetching users", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserResponse>>>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (apiResponse?.Data == null) return null;

                var deptHead = apiResponse.Data
                    .FirstOrDefault(u => u.DepartmentId == departmentId && 
                                        u.RoleName != null && 
                                        u.RoleName.Equals("DepartmentHead", StringComparison.OrdinalIgnoreCase));

                if (deptHead == null)
                {
                    _logger.LogWarning("No Department Head found for DepartmentId: {DepartmentId}", departmentId);
                    return null;
                }

                return new UserDto
                {
                    UserId = deptHead.UserId,
                    Name = deptHead.FullName,
                    Email = deptHead.Email,
                    RoleId = deptHead.RoleId,
                    RoleName = deptHead.RoleName ?? string.Empty,
                    DepartmentId = deptHead.DepartmentId,
                    DepartmentName = deptHead.DepartmentName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Department Head for DepartmentId: {DepartmentId}", departmentId);
                return null;
            }
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("UserService returned {StatusCode} when fetching users", response.StatusCode);
                    return Enumerable.Empty<UserDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserResponse>>>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (apiResponse?.Data == null) return Enumerable.Empty<UserDto>();

                return apiResponse.Data
                    .Where(u => u.RoleName != null && u.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                    .Select(u => new UserDto
                    {
                        UserId = u.UserId,
                        Name = u.FullName,
                        Email = u.Email,
                        RoleId = u.RoleId,
                        RoleName = u.RoleName ?? string.Empty,
                        DepartmentId = u.DepartmentId,
                        DepartmentName = u.DepartmentName
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch users by role {RoleName} from UserService", roleName);
                return Enumerable.Empty<UserDto>();
            }
        }

        public async Task<UserDto?> GetAuditManagerAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("UserService returned {StatusCode} when fetching Audit Manager", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserResponse>>>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (apiResponse?.Data == null) return null;

                // Find first Audit Manager (RoleId = 4 or RoleName = "AuditManager")
                var auditManager = apiResponse.Data
                    .FirstOrDefault(u => u.RoleId == 4 || 
                                        (u.RoleName != null && u.RoleName.Equals("AuditManager", StringComparison.OrdinalIgnoreCase)));

                if (auditManager == null)
                {
                    _logger.LogWarning("No Audit Manager found");
                    return null;
                }

                return new UserDto
                {
                    UserId = auditManager.UserId,
                    Name = auditManager.FullName,
                    Email = auditManager.Email,
                    RoleId = auditManager.RoleId,
                    RoleName = auditManager.RoleName ?? string.Empty,
                    DepartmentId = auditManager.DepartmentId,
                    DepartmentName = auditManager.DepartmentName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Audit Manager from UserService");
                return null;
            }
        }

        private class ApiResponse<T>
        {
            public T? Data { get; set; }
        }

        private class UserResponse
        {
            public int UserId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public int RoleId { get; set; }
            public string? RoleName { get; set; }
            public int? DepartmentId { get; set; }
            public string? DepartmentName { get; set; }
        }
    }
}
