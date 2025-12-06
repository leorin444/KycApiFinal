using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KycApi.Data;
using KycApi.Models;
using KycApi.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;


[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AdminController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AdminLoginRequest model)
    {
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Username == model.Username);

        if (admin == null)
            return Unauthorized("Invalid username");

        if (!BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
            return Unauthorized("Invalid password");

        // Generate JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, admin.Username),
                new Claim(ClaimTypes.Role, "Admin")
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        return Ok(new { token = jwt });
    }

    [HttpGet("generate-hash/{password}")]
    public IActionResult GenerateHash(string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return Ok(hash);
    }

}
