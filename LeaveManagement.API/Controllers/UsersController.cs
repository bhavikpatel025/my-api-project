using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.API.DTOs;
using LeaveManagement.API.Services;
using System.Security.Claims;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                // Users can only access their own data unless they're admin
                var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && currentUserId != id)
                {
                    return Forbid("You can only access your own profile");
                }

                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                return Ok(new { success = true, data = user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}/leave-balance")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserLeaveBalance(int id, [FromBody] List<LeaveBalanceDto> balances)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.UpdateUserLeaveBalanceAsync(id, balances);

                if (!result)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                return Ok(new { success = true, message = "Leave balance updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        //Update

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.UpdateUserAsync(id, updateUserDto);

                if (result == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                return Ok(new { success = true, data = result, message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        //Delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);

                if (!result)
                {
                    return NotFound(new { success = false, message = "User not found or cannot be deleted" });
                }

                return Ok(new { success = true, message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Upload Profile Picture
        [HttpPost("{id}/profile-picture")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePicture(int id, [FromForm] UploadProfilePictureDto model)
        {
            try
            {
                // Check if user is updating their own profile or admin
                var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && currentUserId != id)
                {
                    return Forbid("You can only update your own profile picture");
                }

                if (model.File == null || model.File.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No file provided" });
                }

                var result = await _userService.UploadProfilePictureAsync(id, model.File);

                return Ok(new { success = true, data = result, message = "Profile picture updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Failed to upload profile picture" });
            }
        }


        [HttpDelete("{id}/profile-picture")]
        [Authorize]
        public async Task<IActionResult> DeleteProfilePicture(int id)
        {
            try
            {
                // Check if user is updating their own profile or admin
                var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && currentUserId != id)
                {
                    return Forbid("You can only delete your own profile picture");
                }

                var result = await _userService.DeleteProfilePictureAsync(id);

                if (!result)
                {
                    return NotFound(new { success = false, message = "Profile picture not found" });
                }

                return Ok(new { success = true, message = "Profile picture deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to delete profile picture" });
            }
        }

        [HttpGet("{id}/profile-picture")]
        [Authorize]
        public async Task<IActionResult> GetProfilePicture(int id)
        {
            try
            {
                var result = await _userService.GetProfilePictureAsync(id);

                if (result == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to get profile picture" });
            }
        }

    }
}