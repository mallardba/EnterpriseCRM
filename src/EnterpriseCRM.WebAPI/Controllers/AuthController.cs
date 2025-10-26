using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace EnterpriseCRM.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authenticationService, ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// User Login Endpoint
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Login response with token and user details</returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authenticationService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Validate JWT Token Endpoint
    /// </summary>
    /// <returns>Token validation result</returns>
    [HttpPost("validate")]
    [Authorize]
    public async Task<ActionResult<bool>> ValidateToken()
    {
        try
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token not provided" });
            }

            var isValid = await _authenticationService.ValidateTokenAsync(token);
            if (!isValid)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _authenticationService.GetUserFromTokenAsync(token);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            return Ok(new { valid = true, user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token validation");
            return StatusCode(500, new { message = "An error occurred during token validation" });
        }
    }

    /// <summary>
    /// Change Password Endpoint
    /// </summary>
    /// <param name="changePasswordDto">Password change data</param>
    /// <returns>Password change result</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<bool>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _authenticationService.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (!success)
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new { message = "An error occurred during password change" });
        }
    }
}




        
    
    
    
    