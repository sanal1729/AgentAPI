// <copyright file="RenewalQueryHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Authentication.Queries.Renewal
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using Agent.Application.Authentication.Common;
    using Agent.Application.Common.Interfaces.Authentication;
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Application.Common.Interfaces.Services;
    using Agent.Domain.Common.Errors;
    using Agent.Domain.Entities;
    using ErrorOr;
    using MediatR;
    using Microsoft.AspNetCore.Identity;

    public class RenewalQueryHandler(IJwtTokenHandler jwtTokenHandler,
      IUnitOfWork unitOfWork,
      UserManager<User> userManager,
      IQueryBuilderFactory queryBuilderFactory)

    // IQueryBuilder<UserToken> queryBuilder
       : IRequestHandler<RenewalQuery, ErrorOr<AuthResult>>
    {
        private readonly IJwtTokenHandler _jwtTokenHandler = jwtTokenHandler;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        // private readonly IUserRepository _userRepository = userRepository;
        private readonly UserManager<User> _userManager = userManager;

        // private readonly IQueryBuilder<UserToken> _queryBuilder = queryBuilder;
        private readonly IQueryBuilderFactory _queryBuilderFactory = queryBuilderFactory;

        // private readonly IAuthenticationService _authenticationService;
        // private readonly IAuthenticationRepository _authenticationRepository;
        public async Task<ErrorOr<AuthResult>> Handle(RenewalQuery query, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var refreshTokenPredicate = string.Format(
                "Value = \"{0}\" AND Name = \"{1}\" AND Expires > {2}",
                query.RefreshToken,
                nameof(query.RefreshToken),
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            var usrRepository = _unitOfWork.GetRepository<User>();
            var usrTokenRepository = _unitOfWork.GetRepository<UserToken>();

            var queryBuilder = _queryBuilderFactory.Create<UserToken>(refreshTokenPredicate);

            IReadOnlyList<UserToken> refreshTokens =
                        await usrTokenRepository.GetByPredicateAsync(
                           predicate: queryBuilder.Predicate,
                           orderBy: queryBuilder.OrderBy,
                           includeNavigations: true,
                           cancellationToken: cancellationToken);

            var renewalToken = refreshTokens.FirstOrDefault();

            if (renewalToken is not UserToken userToken)
            {
                // throw new Exception("user not found");
                return new[] { Errors.Authentication.InvalidRefreshToken };
            }

            var userRepository = _unitOfWork.GetRepository<User>();
            var user = await userRepository.GetByIdAsync(
                new User { Id = renewalToken.UserId }, true, cancellationToken);

            if (user == null)
            {
                return new[] { Errors.Authentication.InvalidRefreshToken };
            }

            // var userRoles = await _userManager.GetRolesAsync(user);
            // var userClaims = await _userManager.GetClaimsAsync(user);
            var userAreaRole = user.UserRoles?
                      .Where(ur => ur.AreaId == user.DefaultAreaId)
                      .FirstOrDefault();

            IList<string> userRoles = new List<string>();
            var userClaims = new List<Claim>();

            if (userAreaRole is not null)
                {
                    var roleRepository = _unitOfWork.GetRepository<Role>();
                    var role = await roleRepository
                                        .GetByIdAsync(
                                            new Role { Id = userAreaRole.RoleId, },
                                            false,
                                            cancellationToken);
                    if (role is not null)
                    {
                        userRoles.Add(role.Name ?? string.Empty);
                    }

                    var userClaimfilter = $"UserId == \"{userAreaRole.UserId}\" AND AreaId == {userAreaRole.AreaId}";
                    var userClaimQueryBuilder = _queryBuilderFactory.Create<UserClaim>(userClaimfilter); // sort is null

                    if (userClaimQueryBuilder != null)
                    {
                        var qBClaim = userClaimQueryBuilder.GetExpression(userClaimfilter, null);

                        var userClaimRepository = _unitOfWork.GetRepository<UserClaim>();
                        var userClaimList = await userClaimRepository.GetByPredicateAsync(
                            predicate: qBClaim.Predicate,
                            orderBy: qBClaim.OrderBy,
                            includeNavigations: true,
                            cancellationToken);

                        if (userClaimList is not null)
                        {
                            foreach (var item in userClaimList)
                            {
                                // Add user claims directly to a list of claims instead of userRoles
                                var claimType = item.ClaimType ?? string.Empty;
                                var claimValue = item.ClaimValue ?? string.Empty;
                                userClaims.Add(new Claim(claimType, claimValue));
                            }
                        }
                    }
                }

            var claims = userRoles.Select(role => new Claim(ClaimTypes.Role, role))
                    .Union(userClaims);

            // Generate access & refresh tokens
            var accessToken = _jwtTokenHandler.GenerateAccessToken(user, claims);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);

            // Extract 'jti'
            var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            // Extract 'exp' as UNIX timestamp
            var expUnixStr = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
            long? expUnix = null;
            if (long.TryParse(expUnixStr, out var expUnixVal))
            {
                expUnix = expUnixVal;
            }

            var (refreshToken, refreshTokenExpires) = _jwtTokenHandler.GenerateRefreshToken();
            var refreshTokenExpiresTimestamp = new DateTimeOffset(refreshTokenExpires, TimeSpan.Zero).ToUnixTimeSeconds();

            var usrRefreshToken = new UserToken
            {
                UserId = user.Id,
                LoginProvider = "AgentApp",
                Name = "RefreshToken",
                Value = refreshToken,
                Expires = refreshTokenExpiresTimestamp,
            };
            var usrJtiToken = new UserToken
            {
                UserId = user.Id,
                LoginProvider = "AgentApp",
                Name = "jti",
                Value = jti,
                Expires = expUnix,
            };

            await usrTokenRepository.UpdateAsync(usrRefreshToken, false, cancellationToken);
            await usrTokenRepository.UpdateAsync(usrJtiToken, false, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResult(user, accessToken, refreshToken, refreshTokenExpiresTimestamp);
        }
    }
}