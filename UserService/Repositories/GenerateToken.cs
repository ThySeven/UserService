using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UserService.Repositories;

public class GenerateToken
{
    private static string Secret { get; set; }
    private static string Issuer { get; set; }

    public static void FillSecrets(string secret, string issuer)
    {
        Secret = secret;
        Issuer = issuer;
    }
    
    public static string GenerateJwtToken(string SerializedLogimModel)
    {
        var securityKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var credentials =
            new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, SerializedLogimModel)
        };
        var token = new JwtSecurityToken(
            Issuer,
            "http://localhost",
            claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}