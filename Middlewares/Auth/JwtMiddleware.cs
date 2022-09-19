
using todoapp.Services;

namespace todoapp.Middlewares.Auth;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using todoapp.Config;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JWTSettings _jwtSettings;

    public JwtMiddleware(RequestDelegate next, IOptions<JWTSettings> jwtSettings)
    {
        _next = next;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task Invoke(HttpContext context, IUserService userService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
            await attachUserToContext(context, userService, token);

        await _next(context);
    }

    private async Task attachUserToContext(HttpContext context, IUserService userService, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userEmail = jwtToken.Claims.First(x => x.Type == "email").Value;
            
            // attach user to context on successful jwt validation
            context.Items["User"] = await userService.GetByEmailAsync(userEmail);
        }
        catch
        {
            // do nothing if jwt validation fails
            // user is not attached to context so request won't have access to secure routes
        }
    }
}