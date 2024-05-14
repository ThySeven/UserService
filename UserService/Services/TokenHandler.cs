using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.IO;
using Newtonsoft.Json.Linq;
using UserService.Models;

namespace UserService.Services;

public class TokenHandler
{
    private static string Secret { get; set; }
    private static string Issuer { get; set; }

    public static void FillSecrets(string secret, string issuer)
    {
        Secret = secret;
        Issuer = issuer;
    }
    
    public static string GenerateJwtToken(string SerializedLoginModel)
    {
        var securityKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var credentials =
            new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, SerializedLoginModel)
        };
        var token = new JwtSecurityToken(
            Issuer,
            "http://localhost",
            claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [AllowAnonymous]
    public static LoginModel DecodeToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
    
        // Access token payload
        var payload = jsonToken.Payload;

        var tempValue = payload.Values.ToList();
        string firstTempValue = (string)tempValue[0];

        JObject jsonObject = JObject.Parse(firstTempValue);
        
        string username = (string)jsonObject["Username"];

        // Create a new LoginModel instance with the retrieved username
        var loginModel = new LoginModel
        {
            Username = username
        };

        return loginModel;
    }

    public static string GenerateNewJwtToken(UserModelDTO user)
    {
        string newToken = TokenHandler.GenerateJwtToken(JsonSerializer.Serialize(user));

        return newToken;
    }
}