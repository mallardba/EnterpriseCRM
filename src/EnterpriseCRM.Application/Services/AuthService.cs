using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.Core.Entities;
using EnterpriseCRM.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EnterpriseCRM.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AuthenticationService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto)
    {
        try
        {
            // Validate credentials
            var isValid = await _unitOfWork.Users.ValidateCredentialsAsync(loginDto.Username, loginDto.Password);
            if (!isValid)
            {
                _logger.LogWarning("Invalid username or password");
                // return new LoginResponseDto { Token = string.Empty, ExpiresAt = DateTime.UtcNow, User = new UserDto() };
                throw new Exception("Invalid username or password");
            }

            // Get user details
            var user = await _unitOfWork.Users.GetByUsernameAsync(loginDto.Username);
            if (user == null || user.Status != UserStatus.Active)
            {
                _logger.LogWarning("User not found or inactive: {Username}", loginDto.Username);
                // return new LoginResponseDto { Token = string.Empty, ExpiresAt = DateTime.UtcNow, User = new UserDto() };
                throw new Exception("User not found or inactive");
            }

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            // Generate JWT token
            var token = await GenerateTokenAsync(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpiryInMinutes"));

            return new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Username = user.Username,
                    Role = user.Role,
                    Status = user.Status,
                    LastLoginDate = user.LastLoginDate,
                    Phone = user.Phone,
                    JobTitle = user.JobTitle,
                    Department = user.Department,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    CreatedBy = user.CreatedBy,
                    UpdatedBy = user.UpdatedBy
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", loginDto.Username);
            // return new LoginResponseDto { Token = string.Empty, ExpiresAt = DateTime.UtcNow, User = new UserDto() };
            throw new Exception("Error during login", ex);
        }
    }

    public Task<string> GenerateTokenAsync(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName),
            new Claim("Department", user.Department ?? ""),
            new Claim("JobTitle", user.JobTitle ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpiryInMinutes")),
            signingCredentials: credentials
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            var key = Encoding.UTF8.GetBytes(secretKey!);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<UserDto?> GetUserFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;

            var userId = int.Parse(userIdClaim.Value);
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role,
                Status = user.Status,
                LastLoginDate = user.LastLoginDate,
                Phone = user.Phone,
                JobTitle = user.JobTitle,
                Department = user.Department,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                CreatedBy = user.CreatedBy,
                UpdatedBy = user.UpdatedBy
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            // Validate current password
            if (user.PasswordHash != currentPassword) return false;

            // Update password (in production, hash the new password)
            user.PasswordHash = newPassword;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user ID: {UserId}", userId);
            return false;
        }
    }
}