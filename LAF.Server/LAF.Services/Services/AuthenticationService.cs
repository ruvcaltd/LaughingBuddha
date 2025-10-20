namespace LAF.Services.Services;

using LAF.DataAccess.Data;
using LAF.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthenticationService : IAuthenticationService
{
    private readonly LAFDbContext _context;
    private readonly string _jwtKey;

    public AuthenticationService(LAFDbContext context, IConfiguration configuration)
    {
        _context = context;
        _jwtKey = configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT secret key is not configured");
    }

    public async Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || request.Password != "Admin")
        {
            return null;
        }

        var token = GenerateJwtToken(user.Email, user.Id);

        return new AuthenticationResponse
        {
            Token = token,
            Email = user.Email,
            DisplayName = user.DisplayName,
            UserId = user.Id
        };
    }

    private string GenerateJwtToken(string email, int userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}