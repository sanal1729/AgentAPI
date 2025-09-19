// <copyright file="JwtTokenHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Authentication
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using Agent.Application.Common.Interfaces.Authentication;
    using Agent.Application.Common.Interfaces.Services;
    using Agent.Domain.Entities;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    public class JwtTokenHandler(IDateTimeProvider dateTimeProvider, IOptionsMonitor<JwtSettings> jwtOptions) : IJwtTokenHandler
    {
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
        private readonly JwtSettings _jwtSettings = jwtOptions.CurrentValue;

        public string GenerateAccessToken(User user, IEnumerable<Claim>? roleClaims = null)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(_jwtSettings.Secret))
            {
                throw new InvalidOperationException("JWT Secret is not configured.");
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };
            if (roleClaims != null)
            {
                foreach (var role in roleClaims)
                {
                    claims.Add(new Claim(role.Type, role.Value ?? string.Empty));
                }
            }

            var token = new JwtSecurityToken(
                        issuer: _jwtSettings.Issuer,
                        audience: _jwtSettings.Audience,
                        claims: claims,
                        expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryInMinutes),
                        signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string RefreshToken, DateTime RefreshTokenExpires) GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var refreshToken = Convert.ToBase64String(randomBytes);
            var refreshTokenExpires = _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpiryInMinutes);
            return (refreshToken, refreshTokenExpires);
        }
    }
}
