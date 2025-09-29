using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.API.DTOs;
using LeaveManagement.API.Services;
using System.Security.Claims;
using ClosedXML.Excel;



namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeavesController : ControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeavesController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveDto applyLeaveDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                var result = await _leaveService.ApplyLeaveAsync(currentUserId, applyLeaveDto);

                return Ok(new { success = true, data = result, message = "Leave applied successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("my-leaves")]
        public async Task<IActionResult> GetMyLeaves()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                var leaves = await _leaveService.GetUserLeavesAsync(currentUserId);

                return Ok(new { success = true, data = leaves });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllLeaves([FromQuery] int? userId = null)
        {
            try
            {
                var leaves = await _leaveService.GetAllLeavesAsync(userId);

                return Ok(new { success = true, data = leaves });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelLeave(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                var result = await _leaveService.CancelLeaveAsync(currentUserId, id);

                if (!result)
                {
                    return NotFound(new { success = false, message = "Leave not found" });
                }

                return Ok(new { success = true, message = "Leave cancelled successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("update-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLeaveStatus([FromBody] UpdateLeaveStatusDto updateLeaveStatusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _leaveService.UpdateLeaveStatusAsync(updateLeaveStatusDto);

                if (!result)
                {
                    return NotFound(new { success = false, message = "Leave not found" });
                }

                return Ok(new { success = true, message = "Leave status updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetLeaveTypes()
        {
            try
            {
                var leaveTypes = await _leaveService.GetLeaveTypesAsync();

                return Ok(new { success = true, data = leaveTypes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }



        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportLeaves([FromQuery] int? userId = null)
        {
            try
            {
                var leaves = await _leaveService.GetAllLeavesAsync(userId);

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Leave Reports");

                // Headers
                worksheet.Cell(1, 1).Value = "Employee Name";
                worksheet.Cell(1, 2).Value = "Leave Type";
                worksheet.Cell(1, 3).Value = "Start Date";
                worksheet.Cell(1, 4).Value = "End Date";
                worksheet.Cell(1, 5).Value = "Date of Request";
                worksheet.Cell(1, 6).Value = "Leave Days";
                worksheet.Cell(1, 7).Value = "Reason";
                worksheet.Cell(1, 8).Value = "Status";

                // Style headers
                var headerRange = worksheet.Range(1, 1, 1, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;

                // Data
                for (int i = 0; i < leaves.Count; i++)
                {
                    var leave = leaves[i];
                    var row = i + 2;

                    worksheet.Cell(row, 1).Value = leave.UserName;
                    worksheet.Cell(row, 2).Value = leave.LeaveTypeName;
                    worksheet.Cell(row, 3).Value = leave.StartDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 4).Value = leave.EndDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 5).Value = leave.DateOfRequest.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 6).Value = leave.LeaveDays;
                    worksheet.Cell(row, 7).Value = leave.ReasonForLeave;
                    worksheet.Cell(row, 8).Value = leave.Status;
                }

                // Auto-fit columns
                worksheet.ColumnsUsed().AdjustToContents();

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = userId.HasValue
                    ? $"Employee_{userId}_Leaves_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                    : $"All_Leaves_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
