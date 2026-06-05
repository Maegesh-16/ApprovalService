using ApprovalService.API.DTOs;
using ApprovalService.API.Enums;
using ApprovalService.API.HttpClients;
using ApprovalService.API.Models;
using ApprovalService.API.Repositories;

namespace ApprovalService.API.Services
{
    public class ApprovalServiceImpl : IApprovalService
    {
        private readonly IApprovalRepository _repository;
        private readonly ILogger<ApprovalServiceImpl> _logger;
        private readonly IActionServiceClient _actionServiceClient;
        private readonly INotificationServiceClient _notificationServiceClient;
        private readonly IObservationServiceClient _observationServiceClient;
        private readonly IUserServiceClient _userServiceClient;
        private readonly IAuditServiceClient _auditServiceClient;
        private readonly IReportingServiceClient _reportingServiceClient;

        // =======================================================================
        // All service clients implemented:
        //   - AuditService: to update audit status on approval/rejection ✅ IMPLEMENTED
        //   - ObservationService: to get observation details ✅ IMPLEMENTED
        //   - UserService: to validate users and get Audit Manager ✅ IMPLEMENTED
        //   - NotificationService: to notify requester of approval result ✅ IMPLEMENTED
        // =======================================================================

        public ApprovalServiceImpl(
            IApprovalRepository repository,
            ILogger<ApprovalServiceImpl> logger,
            IActionServiceClient actionServiceClient,
            INotificationServiceClient notificationServiceClient,
            IObservationServiceClient observationServiceClient,
            IUserServiceClient userServiceClient,
            IAuditServiceClient auditServiceClient,
            IReportingServiceClient reportingServiceClient)
        {
            _repository = repository;
            _logger = logger;
            _actionServiceClient = actionServiceClient;
            _notificationServiceClient = notificationServiceClient;
            _observationServiceClient = observationServiceClient;
            _userServiceClient = userServiceClient;
            _auditServiceClient = auditServiceClient;
            _reportingServiceClient = reportingServiceClient;
        }

        public async Task<IEnumerable<ApprovalRequestResponseDto>> GetAllApprovalsAsync()
        {
            var approvals = await _repository.GetAllAsync();
            return approvals.Select(MapToResponseDto);
        }

        public async Task<ApprovalRequestResponseDto?> GetApprovalByIdAsync(int approvalId)
        {
            var approval = await _repository.GetByIdAsync(approvalId);
            return approval == null ? null : MapToResponseDto(approval);
        }

        public async Task<IEnumerable<ApprovalRequestResponseDto>> GetApprovalsByEntityAsync(EntityType entityType, int entityId)
        {
            var approvals = await _repository.GetByEntityAsync(entityType, entityId);
            return approvals.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<ApprovalRequestResponseDto>> GetPendingApprovalsAsync()
        {
            var approvals = await _repository.GetByStatusAsync(ApprovalStatus.Pending);
            return approvals.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<ApprovalRequestResponseDto>> GetApprovalsByRequesterAsync(int userId)
        {
            var approvals = await _repository.GetByRequestedByUserIdAsync(userId);
            return approvals.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<ApprovalRequestResponseDto>> GetApprovalsByApproverAsync(int userId)
        {
            var approvals = await _repository.GetByApproverUserIdAsync(userId);
            return approvals.Select(MapToResponseDto);
        }

        public async Task<ApprovalRequestResponseDto> CreateApprovalRequestAsync(CreateApprovalRequestDto dto)
        {
            // =======================================================================
            // DUMMY: Validate RequestedByUserId exists via UserService API
            // var user = await _userServiceClient.GetUserAsync(dto.RequestedByUserId);
            // if (user == null) throw new ArgumentException("User not found");
            _logger.LogWarning("[DUMMY] Skipping RequestedByUserId validation for UserId: {UserId}. " +
                "Replace with actual UserService API call.", dto.RequestedByUserId);
            // =======================================================================

            // =======================================================================
            // DUMMY: Validate EntityId exists via respective service API
            // Based on dto.EntityType:
            //   Audit -> AuditService.GetAuditAsync(dto.EntityId)
            //   Observation -> ObservationService.GetObservationAsync(dto.EntityId)
            //   Action -> ActionService.GetActionAsync(dto.EntityId)
            _logger.LogWarning("[DUMMY] Skipping EntityId validation for EntityType: {EntityType}, EntityId: {EntityId}. " +
                "Replace with actual service API call.", dto.EntityType, dto.EntityId);
            // =======================================================================

            var request = new ApprovalRequest
            {
                EntityType = dto.EntityType,
                EntityId = dto.EntityId,
                ApprovalType = dto.ApprovalType,
                RequestedByUserId = dto.RequestedByUserId,
                ApproverUserId = dto.ApproverUserId,
                Status = ApprovalStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(request);

            // =======================================================================
            // Notify approver via NotificationService API
            await _notificationServiceClient.SendApprovalRequestNotificationAsync(
                dto.ApproverUserId, // Notify the designated approver
                dto.EntityType.ToString(),
                dto.EntityId);
            // =======================================================================

            return MapToResponseDto(created);
        }

        public async Task<ApprovalRequestResponseDto?> ProcessApprovalAsync(int approvalId, ProcessApprovalDto dto)
        {
            var approval = await _repository.GetByIdAsync(approvalId);
            if (approval == null) return null;

            if (approval.Status != ApprovalStatus.Pending)
            {
                throw new InvalidOperationException("Only pending approvals can be processed.");
            }

            // =======================================================================
            // DUMMY: Validate ActionByUserId exists and has approver role via UserService API
            // var user = await _userServiceClient.GetUserAsync(dto.ActionByUserId);
            // if (user == null || !user.HasApproverRole) throw new UnauthorizedAccessException("Not authorized");
            _logger.LogWarning("[DUMMY] Skipping ActionByUserId validation for UserId: {UserId}. " +
                "Replace with actual UserService API call.", dto.ActionByUserId);
            // =======================================================================

            // Update approval status
            approval.Status = dto.Action == ApprovalAction.Approved
                ? ApprovalStatus.Approved
                : ApprovalStatus.Rejected;
            await _repository.UpdateAsync(approval);

            // Record history
            var history = new ApprovalHistory
            {
                ApprovalId = approvalId,
                ActionByUserId = dto.ActionByUserId,
                Action = dto.Action,
                Comments = dto.Comments,
                ActionDate = DateTime.UtcNow
            };
            await _repository.AddHistoryAsync(history);

            // Handle CorrectiveActionClosure approvals (3-Level: Employee → Dept Head → Audit Manager)
            if (approval.ApprovalType == ApprovalType.CorrectiveActionClosure)
            {
                // Check if this is Level 1 (Dept Head) or Level 2 (Audit Manager) approval
                var existingApprovals = await _repository.GetByEntityAsync(approval.EntityType, approval.EntityId);
                var previousApprovals = existingApprovals
                    .Where(a => a.ApprovalId != approvalId && 
                               a.ApprovalType == ApprovalType.CorrectiveActionClosure &&
                               a.Status == ApprovalStatus.Approved)
                    .OrderBy(a => a.CreatedAt)
                    .ToList();

                bool isLevel1 = previousApprovals.Count == 0; // No previous approvals = Dept Head (Level 1)
                bool isLevel2 = previousApprovals.Count == 1; // 1 previous approval = Audit Manager (Level 2)

                if (dto.Action == ApprovalAction.Rejected)
                {
                    // REJECTION: Reopen the action and notify requester
                    _logger.LogInformation("{Level} rejected closure for ActionId: {ActionId}. Reopening action.",
                        isLevel1 ? "Dept Head" : "Audit Manager", approval.EntityId);

                    await _actionServiceClient.UpdateActionStatusAsync(approval.EntityId, new UpdateActionStatusPayload
                    {
                        Status = 4  // Reopened
                    });

                    // Sync updated status to ReportingService (best-effort, non-critical)
                    // ReportingService has no "Reopened" status, map to InProgress(1)
                    await _reportingServiceClient.UpdateActionStatusAsync(approval.EntityId, 1);

                    await _notificationServiceClient.SendApprovalResultNotificationAsync(
                        approval.RequestedByUserId,
                        approvalId,
                        false,  // rejected
                        approval.EntityType.ToString());
                    
                    _logger.LogInformation("Action {ActionId} reopened and user {UserId} notified.",
                        approval.EntityId, approval.RequestedByUserId);
                }
                else
                {
                    // APPROVAL
                    if (isLevel1)
                    {
                        // LEVEL 1 (DEPARTMENT HEAD): Close action, check if all actions closed, create FinalAuditClosure if needed
                        _logger.LogInformation("Level 1: Dept Head approved closure for ActionId: {ActionId}. Closing action...",
                            approval.EntityId);

                        // First, get the action to find which audit it belongs to
                        var action = await _actionServiceClient.GetActionAsync(approval.EntityId);
                        if (action == null)
                        {
                            _logger.LogError("Could not retrieve ActionId: {ActionId}. Cannot proceed with closure.",
                                approval.EntityId);
                            return MapToResponseDto(approval);
                        }

                        // Close the action
                        await _actionServiceClient.UpdateActionStatusAsync(approval.EntityId, new UpdateActionStatusPayload
                        {
                            Status = 3  // Closed
                        });

                        // Sync updated status to ReportingService (best-effort, non-critical)
                        await _reportingServiceClient.UpdateActionStatusAsync(approval.EntityId, 3);

                        _logger.LogInformation("Action {ActionId} closed successfully. ObservationId: {ObservationId}",
                            approval.EntityId, action.ObservationId);

                        // Get the observation to find the audit
                        var observation = await _observationServiceClient.GetObservationAsync(action.ObservationId);
                        if (observation == null)
                        {
                            _logger.LogError("Could not retrieve ObservationId: {ObservationId}. Cannot check audit closure.",
                                action.ObservationId);
                            return MapToResponseDto(approval);
                        }

                        _logger.LogInformation("Action belongs to AuditId: {AuditId}. Checking if all actions are closed...",
                            observation.AuditId);

                        // Get all observations for this audit
                        var allObservations = await _observationServiceClient.GetObservationsByAuditAsync(observation.AuditId);
                        if (allObservations == null || !allObservations.Any())
                        {
                            _logger.LogWarning("No observations found for AuditId: {AuditId}.", observation.AuditId);
                            return MapToResponseDto(approval);
                        }

                        // Check if ALL actions across all observations are closed
                        bool allActionsClosed = true;
                        int totalActions = 0;
                        int closedActions = 0;

                        foreach (var obs in allObservations)
                        {
                            var actionsForObs = await _actionServiceClient.GetActionsByObservationAsync(obs.ObservationId);
                            if (actionsForObs != null && actionsForObs.Any())
                            {
                                totalActions += actionsForObs.Count;
                                closedActions += actionsForObs.Count(a => a.Status == 3); // 3 = Closed
                                
                                if (actionsForObs.Any(a => a.Status != 3))
                                {
                                    allActionsClosed = false;
                                }
                            }
                        }

                        _logger.LogInformation("Audit {AuditId} action status: {Closed}/{Total} actions closed. AllClosed: {AllClosed}",
                            observation.AuditId, closedActions, totalActions, allActionsClosed);

                        // If all actions are closed, create FinalAuditClosure approval for Audit Manager
                        if (allActionsClosed && totalActions > 0)
                        {
                            _logger.LogInformation("All actions closed for Audit {AuditId}. Creating FinalAuditClosure approval for Audit Manager...",
                                observation.AuditId);

                            // Get Audit Manager from UserService
                            var auditManager = await _userServiceClient.GetAuditManagerAsync();
                            if (auditManager == null)
                            {
                                _logger.LogError("No Audit Manager found in UserService. Cannot create FinalAuditClosure approval for Audit {AuditId}",
                                    observation.AuditId);
                                return MapToResponseDto(approval);
                            }

                            // Check if FinalAuditClosure approval already exists for this audit
                            var existingFinalApprovals = await _repository.GetByEntityAsync(EntityType.Audit, observation.AuditId);
                            if (existingFinalApprovals.Any(a => a.ApprovalType == ApprovalType.FinalAuditClosure && a.Status == ApprovalStatus.Pending))
                            {
                                _logger.LogWarning("FinalAuditClosure approval already exists for Audit {AuditId}. Skipping creation.",
                                    observation.AuditId);
                                return MapToResponseDto(approval);
                            }

                            // Create FinalAuditClosure approval
                            var finalClosureApproval = new ApprovalRequest
                            {
                                EntityType = EntityType.Audit,  // Entity is the Audit, not Action
                                EntityId = observation.AuditId,
                                ApprovalType = ApprovalType.FinalAuditClosure,
                                RequestedByUserId = dto.ActionByUserId,  // Dept Head is requester
                                ApproverUserId = auditManager.UserId,
                                Status = ApprovalStatus.Pending,
                                CreatedAt = DateTime.UtcNow
                            };

                            await _repository.CreateAsync(finalClosureApproval);

                            _logger.LogInformation("FinalAuditClosure approval created (ApprovalId: {ApprovalId}) for AuditId: {AuditId}, assigned to AuditManager {AuditManagerId}",
                                finalClosureApproval.ApprovalId, observation.AuditId, auditManager.UserId);

                            // Notify Audit Manager
                            try
                            {
                                await _notificationServiceClient.SendApprovalRequestNotificationAsync(
                                    auditManager.UserId,
                                    "Audit",
                                    observation.AuditId);

                                _logger.LogInformation("Audit Manager {AuditManagerId} notified of final audit closure request for AuditId: {AuditId}",
                                    auditManager.UserId, observation.AuditId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send notification to Audit Manager {AuditManagerId} for AuditId: {AuditId}. Approval created but notification failed.",
                                    auditManager.UserId, observation.AuditId);
                            }

                            // Update audit status to PendingClosure
                            try
                            {
                                await _auditServiceClient.UpdateAuditStatusAsync(observation.AuditId, new UpdateAuditStatusPayload
                                {
                                    StatusId = 6  // PendingClosure
                                });

                                _logger.LogInformation("Audit {AuditId} status updated to PendingClosure.", observation.AuditId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to update Audit {AuditId} status to PendingClosure. Approval exists but audit status not updated.",
                                    observation.AuditId);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Not all actions closed yet for Audit {AuditId}. {Remaining} actions remaining.",
                                observation.AuditId, totalActions - closedActions);
                        }

                        // Notify the employee who requested closure
                        try
                        {
                            await _notificationServiceClient.SendApprovalResultNotificationAsync(
                                approval.RequestedByUserId,
                                approvalId,
                                true,  // approved
                                approval.EntityType.ToString());

                            _logger.LogInformation("Employee {EmployeeId} notified of action closure approval for ActionId: {ActionId}",
                                approval.RequestedByUserId, approval.EntityId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send notification to Employee {EmployeeId} for ActionId: {ActionId}",
                                approval.RequestedByUserId, approval.EntityId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Unexpected approval level for CorrectiveActionClosure. ActionId: {ActionId}. {Count} previous approvals found. This should not happen with the current workflow.",
                            approval.EntityId, previousApprovals.Count);
                    }
                }
            }
            // Handle FinalAuditClosure approvals (Phase 3: Audit Manager → Close Audit)
            else if (approval.ApprovalType == ApprovalType.FinalAuditClosure)
            {
                if (dto.Action == ApprovalAction.Rejected)
                {
                    // REJECTION BY AUDIT MANAGER: Get all actions for this audit and reopen them
                    _logger.LogInformation("Audit Manager rejected final closure for AuditId: {AuditId}. Reopening all actions...",
                        approval.EntityId);

                    // Get audit details
                    var audit = await _auditServiceClient.GetAuditAsync(approval.EntityId);
                    if (audit == null)
                    {
                        _logger.LogWarning("Could not retrieve AuditId: {AuditId}. Cannot reopen actions.",
                            approval.EntityId);
                        return MapToResponseDto(approval);
                    }

                    // Get all observations for this audit
                    var allObservations = await _observationServiceClient.GetObservationsByAuditAsync(approval.EntityId);
                    var affectedEmployees = new HashSet<int>(); // Track unique employee IDs
                    
                    if (allObservations != null && allObservations.Any())
                    {
                        // Reopen all actions and collect employee IDs
                        foreach (var obs in allObservations)
                        {
                            var actions = await _actionServiceClient.GetActionsByObservationAsync(obs.ObservationId);
                            if (actions != null && actions.Any())
                            {
                                foreach (var action in actions)
                                {
                                    // Reopen the action
                                    await _actionServiceClient.UpdateActionStatusAsync(action.ActionId, new UpdateActionStatusPayload
                                    {
                                        Status = 4  // Reopened
                                    });
                                    
                                    _logger.LogInformation("Action {ActionId} reopened due to Audit Manager rejection.", action.ActionId);
                                    
                                    // Track the employee who owns this action
                                    affectedEmployees.Add(action.AssignedToUserId);
                                }
                            }
                        }
                    }

                    // Phase 4: Notify ALL stakeholders of rejection
                    try
                    {
                        // 1. Notify Department Head who requested the final closure
                        await _notificationServiceClient.SendApprovalResultNotificationAsync(
                            approval.RequestedByUserId,
                            approvalId,
                            false,  // rejected
                            approval.EntityType.ToString());

                        _logger.LogInformation("Department Head {DeptHeadId} notified of final closure rejection for AuditId: {AuditId}",
                            approval.RequestedByUserId, approval.EntityId);

                        // 2. Notify all employees whose actions were reopened
                        foreach (var employeeId in affectedEmployees)
                        {
                            try
                            {
                                await _notificationServiceClient.SendApprovalResultNotificationAsync(
                                    employeeId,
                                    approvalId,
                                    false,  // rejected
                                    EntityType.Action.ToString());

                                _logger.LogInformation("Employee {EmployeeId} notified that their actions were reopened for AuditId: {AuditId}",
                                    employeeId, approval.EntityId);
                            }
                            catch (Exception empEx)
                            {
                                _logger.LogError(empEx, "Failed to send rejection notification to Employee {EmployeeId}.", employeeId);
                            }
                        }

                        _logger.LogInformation("Rejection notifications sent to {Count} employees for AuditId: {AuditId}",
                            affectedEmployees.Count, approval.EntityId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send rejection notification to Department Head {DeptHeadId}.",
                            approval.RequestedByUserId);
                    }
                }
                else
                {
                    // APPROVAL BY AUDIT MANAGER: Close the audit
                    _logger.LogInformation("Audit Manager approved final closure for AuditId: {AuditId}. Closing audit...",
                        approval.EntityId);

                    // Get audit details to notify all stakeholders
                    var audit = await _auditServiceClient.GetAuditAsync(approval.EntityId);
                    if (audit == null)
                    {
                        _logger.LogWarning("Could not retrieve AuditId: {AuditId}. Cannot close audit.",
                            approval.EntityId);
                        return MapToResponseDto(approval);
                    }

                    // Close the audit
                    try
                    {
                        await _auditServiceClient.UpdateAuditStatusAsync(approval.EntityId, new UpdateAuditStatusPayload
                        {
                            StatusId = 3  // Closed
                        });

                        _logger.LogInformation("Audit {AuditId} closed successfully by Audit Manager {AuditManagerId}.",
                            approval.EntityId, dto.ActionByUserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to close audit {AuditId} in AuditService.", approval.EntityId);
                        throw;
                    }

                    // Notify all stakeholders: Department Head, Auditor
                    try
                    {
                        // Notify Department Head who requested final closure
                        await _notificationServiceClient.SendApprovalResultNotificationAsync(
                            approval.RequestedByUserId,
                            approvalId,
                            true,  // approved
                            approval.EntityType.ToString());

                        _logger.LogInformation("Department Head {DeptHeadId} notified of audit closure for AuditId: {AuditId}",
                            approval.RequestedByUserId, approval.EntityId);

                        // Notify Auditor
                        await _notificationServiceClient.SendApprovalResultNotificationAsync(
                            audit.AuditorId,
                            approvalId,
                            true,  // approved
                            approval.EntityType.ToString());

                        _logger.LogInformation("Auditor {AuditorId} notified of audit closure for AuditId: {AuditId}",
                            audit.AuditorId, approval.EntityId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notifications for audit closure. Audit {AuditId} closed but notifications failed.",
                            approval.EntityId);
                    }
                }
            }
            else
            {
                // For other approval types, use simple single-level workflow
                await _notificationServiceClient.SendApprovalResultNotificationAsync(
                    approval.RequestedByUserId,
                    approvalId,
                    dto.Action == ApprovalAction.Approved,
                    approval.EntityType.ToString());
            }

            return MapToResponseDto(approval);
        }

        public async Task<IEnumerable<ApprovalHistoryResponseDto>> GetApprovalHistoryAsync(int approvalId)
        {
            var histories = await _repository.GetHistoryByApprovalIdAsync(approvalId);
            return histories.Select(h => new ApprovalHistoryResponseDto
            {
                HistoryId = h.HistoryId,
                ApprovalId = h.ApprovalId,
                ActionByUserId = h.ActionByUserId,
                Action = h.Action,
                Comments = h.Comments,
                ActionDate = h.ActionDate
            });
        }

        private static ApprovalRequestResponseDto MapToResponseDto(ApprovalRequest request)
        {
            return new ApprovalRequestResponseDto
            {
                ApprovalId = request.ApprovalId,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                ApprovalType = request.ApprovalType,
                RequestedByUserId = request.RequestedByUserId,
                ApproverUserId = request.ApproverUserId,
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            };
        }
    }
}
