using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using project.Models.DTO;
using project.Models.Entities;
using project.Services.Interfaces;

namespace project.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserService userService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userService = userService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        var user = await _userService.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        var isValidPassword = await _userService.VerifyPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            _logger.LogWarning("Login failed: Invalid password for user - {Username}", request.Username);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        var token = await GenerateJwtTokenAsync(user);
        _logger.LogInformation("Login successful for user: {Username}", request.Username);

        return new LoginResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60") * 60
        };
    }

    public async Task<string> GenerateJwtTokenAsync(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);
        var expiryMinutes = int.Parse(jwtSection["ExpiryMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        if (!string.IsNullOrEmpty(user.Permissions))
        {
            var permissions = System.Text.Json.JsonSerializer.Deserialize<string[]>(user.Permissions);
            if (permissions != null)
            {
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }
            }
        }

        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
