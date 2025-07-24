using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MicroJack.API.Data;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoleService _roleService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            ApplicationDbContext context, 
            IRoleService roleService, 
            IConfiguration configuration,
            ILogger<AuthenticationService> logger)
        {
            _context = context;
            _roleService = roleService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            try
            {
                // Find guard by username
                var guard = await _context.Guards
                    .FirstOrDefaultAsync(g => g.Username == username && g.IsActive);

                if (guard == null)
                {
                    _logger.LogWarning("Login attempt failed: Guard not found or inactive - {Username}", username);
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Invalid credentials"
                    };
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(password, guard.PasswordHash))
                {
                    _logger.LogWarning("Login attempt failed: Invalid password - {Username}", username);
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Invalid credentials"
                    };
                }

                // Get guard roles
                var roles = await _roleService.GetGuardRolesAsync(guard.Id);
                var roleNames = roles.Select(r => r.Name).ToList();

                // Generate JWT token
                var token = GenerateJwtToken(guard, roleNames);

                _logger.LogInformation("Guard {Username} logged in successfully", username);

                return new AuthenticationResult
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    Guard = guard,
                    Roles = roleNames
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for {Username}", username);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<bool> LogoutAsync(int guardId)
        {
            try
            {
                // In a more sophisticated implementation, you might want to maintain
                // a blacklist of revoked tokens or store active sessions
                _logger.LogInformation("Guard {GuardId} logged out", guardId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for guard {GuardId}", guardId);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int guardId, string currentPassword, string newPassword)
        {
            try
            {
                var guard = await _context.Guards.FindAsync(guardId);
                if (guard == null)
                {
                    return false;
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, guard.PasswordHash))
                {
                    _logger.LogWarning("Password change failed: Invalid current password - Guard {GuardId}", guardId);
                    return false;
                }

                // Hash and update new password
                guard.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                guard.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Password changed successfully for guard {GuardId}", guardId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for guard {GuardId}", guardId);
                return false;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(GetJwtSecret());

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var guardIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "GuardId");

                if (guardIdClaim != null && int.TryParse(guardIdClaim.Value, out int guardId))
                {
                    // Verify guard still exists and is active
                    var guard = await _context.Guards.FindAsync(guardId);
                    return guard != null && guard.IsActive;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateJwtToken(Guard guard, List<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetJwtSecret());

            var claims = new List<Claim>
            {
                new Claim("GuardId", guard.Id.ToString()),
                new Claim("Username", guard.Username),
                new Claim("FullName", guard.FullName)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(GetTokenExpirationHours()),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GetJwtSecret()
        {
            return _configuration["JWT:Secret"] ?? "MicroJack-DefaultSecret-ChangeInProduction-2024";
        }

        private int GetTokenExpirationHours()
        {
            return int.TryParse(_configuration["JWT:ExpirationHours"], out int hours) ? hours : 8;
        }
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public Guard? Guard { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}