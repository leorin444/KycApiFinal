using BCrypt.Net;
using KycApi.Data;
using KycApi.DTOs;
using KycApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace KycApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AccountController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // -----------------------------------------
        // 🔹 REGISTER USER
        // -----------------------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username already exists");

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("User registered successfully");
        }

        // -----------------------------------------
        // 🔹 LOGIN (Admin + User)
        // -----------------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password");

            var jwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = jwtToken,
                refreshToken = refreshToken.Token,
                role = user.Role
            });
        }

        // -----------------------------------------
        // 🔹 REQUEST PASSWORD RESET
        // -----------------------------------------
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound("User not found");

            var resetToken = new PasswordResetToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddHours(1),
                UserId = user.Id
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // Send via email in production
            return Ok(new { resetToken = resetToken.Token });
        }

        // -----------------------------------------
        // 🔹 RESET PASSWORD
        // -----------------------------------------
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var tokenEntry = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == request.Token && !t.IsUsed && t.Expires > DateTime.UtcNow);

            if (tokenEntry == null) return BadRequest("Invalid or expired token");

            tokenEntry.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            tokenEntry.IsUsed = true;

            await _context.SaveChangesAsync();

            return Ok("Password reset successfully");
        }

        // -----------------------------------------
        // 🔹 REFRESH JWT TOKEN
        // -----------------------------------------
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string token)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);

            if (refreshToken == null || refreshToken.Expires < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            var jwtToken = GenerateJwtToken(refreshToken.User);

            return Ok(new { token = jwtToken });
        }

        // -----------------------------------------
        // 🔹 JWT GENERATOR
        // -----------------------------------------
        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(5),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // -----------------------------------------
        // 🔹 REFRESH TOKEN GENERATOR
        // -----------------------------------------
        private RefreshToken GenerateRefreshToken(int userId)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = userId,
                IsRevoked = false
            };
        }
    }
}
