using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FitPlatform.Api.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public string GenerateToken(User user, Guid? trainerId, Guid? studentId, Guid? studentProfileId = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (trainerId.HasValue) claims.Add(new("TrainerId", trainerId.Value.ToString()));
        if (studentId.HasValue) claims.Add(new("StudentId", studentId.Value.ToString()));
        if (studentProfileId.HasValue) claims.Add(new("StudentProfileId", studentProfileId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
