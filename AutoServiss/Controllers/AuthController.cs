using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AutoServiss.Database;
using AutoServiss.Services.Auth;

namespace AutoServiss.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppSettings _settings;
        private readonly IAuthService _service;

        public AuthController(
            IHttpContextAccessor httpContextAccessor, 
            IOptions<AppSettings> settings, 
            IAuthService service)
        {
            _httpContextAccessor = httpContextAccessor;
            _settings = settings.Value;
            _service = service;
        }

        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> Token()
        {
            if (!_httpContextAccessor.HttpContext.Request.HasFormContentType)
            {
                throw new BadRequestException("Content-Type jābūt application/x-www-form-urlencoded");
            }

            var grantType = _httpContextAccessor.HttpContext.Request.Form["grant_type"];
            if (grantType.Count == 0)
            {
                throw new BadRequestException("Nav norādīts parametrs grant_type");
            }

            Darbinieks user = null;

            if (grantType == "password")
            {
                var username = _httpContextAccessor.HttpContext.Request.Form["username"];
                if (username.Count == 0)
                {
                    throw new BadRequestException("Nav norādīts parametrs username");
                }
                var password = _httpContextAccessor.HttpContext.Request.Form["password"];
                if (password.Count == 0)
                {
                    throw new BadRequestException("Nav norādīts parametrs password");
                }

                user = await _service.ValidateCredentialsAsync(username, password);
                if (user == null)
                {
                    return StatusCode(400, new { messages = new List<string> { "Nepareizs lietotājvārds vai parole" } });
                }
                if (!user.Aktivs)
                {
                    return StatusCode(400, new { messages = new List<string> { "Jūsu konts ir bloķēts!" } });
                }
            }
            else if (grantType == "refresh_token")
            {
                var refreshToken = _httpContextAccessor.HttpContext.Request.Form["refresh_token"];
                if (refreshToken.Count == 0)
                {
                    throw new BadRequestException("Nav norādīts parametrs refresh_token");
                }

                user = await _service.ValidateRefreshTokenAsync(refreshToken);
                if (user == null)
                {
                    throw new BadRequestException("Nepareizs refresh_token");
                }
            }
            else
            {
                throw new BadRequestException("Nezināms grant_type");
            }

            var now = DateTime.UtcNow;
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.SecretKey));
            var expires = TimeSpan.FromMinutes(_settings.Expiration);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64),
            new Claim("name", user.PilnsVards),
            new Claim("admin", user.Administrators ? "true" : "false")
        };

            var jwt = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expires),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return StatusCode(200,
                new
                {
                    access_token = encodedJwt,
                    expires_in = expires.TotalSeconds,
                    refresh_token = user.RefreshToken,
                    name = user.PilnsVards,
                    admin = user.Administrators
                });           
        }

        /// <summary>
        /// Get this datetime as a Unix epoch timestamp (seconds since Jan 1, 1970, midnight UTC).
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>Seconds since Unix epoch.</returns>
        private static long ToUnixEpochDate(DateTime date) => new DateTimeOffset(date).ToUniversalTime().ToUnixTimeSeconds();
    }
}
