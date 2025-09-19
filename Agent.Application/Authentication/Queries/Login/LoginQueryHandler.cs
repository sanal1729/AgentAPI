// <copyright file="LoginQueryHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Authentication.Commands.Register
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using Agent.Application.Authentication.Common;
    using Agent.Application.Authentication.Queries.Login;
    using Agent.Application.Common.Interfaces.Authentication;
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Application.Common.Interfaces.Services;
    using Agent.Domain.Common.Errors;
    using Agent.Domain.Entities;
    using ErrorOr;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class LoginQueryHandler(
      UserManager<User> userManager,
      SignInManager<User> signInManager,
      IJwtTokenHandler jwtTokenHandler,
      IUnitOfWork unitOfWork,
      IPasswordHasher<User> passwordHasher,

    // IQueryBuilder<User> queryBuilder,
      IQueryBuilderFactory queryBuilderFactory) : IRequestHandler<LoginQuery, ErrorOr<AuthResult>>
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly IJwtTokenHandler _jwtTokenHandler = jwtTokenHandler;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

        // private readonly IQueryBuilder<User> _queryBuilder = queryBuilder;
        private readonly IQueryBuilderFactory _queryBuilderFactory = queryBuilderFactory;

        public async Task<ErrorOr<AuthResult>> Handle(LoginQuery query, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            {
                var userRepository = _unitOfWork.GetRepository<User>();

                // Build the dynamic filter using the factory
                var filter = $"Email == \"{query.Email}\"";
                var queryBuilder = _queryBuilderFactory.Create<User>(filter); // sort is null

                // Fetch users using the parsed predicate and orderBy
                IReadOnlyList<User> users = await userRepository.GetByPredicateAsync(
                    predicate: queryBuilder.Predicate,
                    orderBy: queryBuilder.OrderBy,
                    includeNavigations: true, // enable eager loading
                    cancellationToken: cancellationToken);

                // Get the first matching user
                var user = users.FirstOrDefault();
                if (user is null)
                {
                    return Errors.Authentication.InvalidEmail;
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, query.Password, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    return Errors.Authentication.InvalidCredentials;
                }

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

                // // Fetch user roles
                // var userRoles = await _userManager.GetRolesAsync(user);
                // userClaims.AddRange(await _userManager.GetClaimsAsync(user));
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

                // Save refresh token using UnitOfWork
                var usrTokenRepository = _unitOfWork.GetRepository<UserToken>();
                var rToken = new UserToken
                {
                    UserId = user.Id,
                    LoginProvider = "AgentApp",
                    Name = "RefreshToken",
                    Value = refreshToken,
                    Expires = refreshTokenExpiresTimestamp,
                };
                var jToken = new UserToken
                {
                    UserId = user.Id,
                    LoginProvider = "AgentApp",
                    Name = "jti",
                    Value = jti,
                    Expires = expUnix,
                };

                var tokens = new List<UserToken> { rToken, jToken };

                var rTokenExists = await usrTokenRepository.GetByIdAsync(rToken, false, cancellationToken);
                if (rTokenExists != null)
                {
                    await usrTokenRepository.DeleteByIdAsync(rToken, false, cancellationToken);
                }

                var jTokenExists = await usrTokenRepository.GetByIdAsync(jToken, false, cancellationToken);
                if (jTokenExists != null)
                {
                    await usrTokenRepository.DeleteByIdAsync(jToken, false, cancellationToken);
                }

                // await _userManager.SetAuthenticationTokenAsync(user, "AgentApp", "RefreshToken", refreshToken);
                await usrTokenRepository.AddRangeAsync(tokens, false, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new AuthResult(user, accessToken, refreshToken, refreshTokenExpiresTimestamp);
            }
        }
    }
}
