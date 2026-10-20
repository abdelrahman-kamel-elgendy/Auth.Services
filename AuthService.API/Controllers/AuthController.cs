using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AuthService.Core.Entities;
using AuthService.Core.DTOs;
using AuthService.Core.Interfaces;
using AuthService.Infrastructure.Data;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _context;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IJwtService jwtService,
            ILogger<AuthController> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _logger = logger;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

                var user = new ApplicationUser
                {
                    UserName = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    BirthDate = request.BirthDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EmailConfirmed = false,
                    IsBlocked = false
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                // Assign default role
                await _userManager.AddToRoleAsync(user, "User");

                _logger.LogInformation("User registered successfully: {Email}", request.Email);

                return Ok(new { message = "User registered successfully", userId = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || user.IsBlocked)
                {
                    return Unauthorized(new { message = "Invalid credentials or account blocked" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                var accessToken = await _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Store refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    TokenId = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = refreshToken,
                    TokenExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                };

                await _context.RefreshTokens.AddAsync(refreshTokenEntity);
                await _context.SaveChangesAsync();

                var roles = await _userManager.GetRolesAsync(user);

                _logger.LogInformation("User logged in successfully: {Email}", request.Email);

                return Ok(new AuthResponse
                {
                    UserId = user.Id.ToString(),
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    Roles = roles.ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
                var userId = Guid.Parse(principal.FindFirst("userId")?.Value ?? throw new InvalidOperationException());

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.IsBlocked)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var storedRefreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId && rt.IsActive);

                if (storedRefreshToken == null || storedRefreshToken.TokenExpiryDate < DateTime.UtcNow)
                {
                    return Unauthorized(new { message = "Invalid or expired refresh token" });
                }

                var newAccessToken = await _jwtService.GenerateAccessToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // Rotate refresh token
                storedRefreshToken.RevokedAt = DateTime.UtcNow;
                storedRefreshToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();

                var newRefreshTokenEntity = new RefreshToken
                {
                    TokenId = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = newRefreshToken,
                    TokenExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    ReplacedByTokenId = storedRefreshToken.TokenId
                };

                await _context.RefreshTokens.AddAsync(newRefreshTokenEntity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    accessToken = newAccessToken,
                    refreshToken = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return Unauthorized(new { message = "Invalid token" });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            try
            {
                var token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsActive);

                if (token != null)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "An error occurred during logout" });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { message = "An error occurred while changing password" });
            }
        }
    }
}