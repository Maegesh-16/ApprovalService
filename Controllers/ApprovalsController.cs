using Microsoft.AspNetCore.Mvc;
using ApprovalService.API.DTOs;
using ApprovalService.API.Enums;
using ApprovalService.API.Services;

namespace ApprovalService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalsController : ControllerBase
    {
        private readonly IApprovalService _service;

        public ApprovalsController(IApprovalService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var approvals = await _service.GetAllApprovalsAsync();
            return Ok(approvals);
        }

        [HttpGet("{approvalId}")]
        public async Task<IActionResult> GetById(int approvalId)
        {
            var approval = await _service.GetApprovalByIdAsync(approvalId);
            if (approval == null) return NotFound();
            return Ok(approval);
        }

        [HttpGet("by-entity/{entityType}/{entityId}")]
        public async Task<IActionResult> GetByEntity(EntityType entityType, int entityId)
        {
            var approvals = await _service.GetApprovalsByEntityAsync(entityType, entityId);
            return Ok(approvals);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var approvals = await _service.GetPendingApprovalsAsync();
            return Ok(approvals);
        }

        [HttpGet("by-requester/{userId}")]
        public async Task<IActionResult> GetByRequester(int userId)
        {
            var approvals = await _service.GetApprovalsByRequesterAsync(userId);
            return Ok(approvals);
        }

        [HttpGet("by-approver/{userId}")]
        public async Task<IActionResult> GetByApprover(int userId)
        {
            var approvals = await _service.GetApprovalsByApproverAsync(userId);
            return Ok(approvals);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApprovalRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _service.CreateApprovalRequestAsync(dto);
            return CreatedAtAction(nameof(GetById), new { approvalId = created.ApprovalId }, created);
        }

        [HttpPost("{approvalId}/process")]
        public async Task<IActionResult> Process(int approvalId, [FromBody] ProcessApprovalDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.ProcessApprovalAsync(approvalId, dto);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{approvalId}/history")]
        public async Task<IActionResult> GetHistory(int approvalId)
        {
            var history = await _service.GetApprovalHistoryAsync(approvalId);
            return Ok(history);
        }
    }
}
