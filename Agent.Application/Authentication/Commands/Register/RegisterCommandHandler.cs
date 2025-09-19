// <copyright file="RegisterCommandHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Authentication.Commands.Register
{
    using System.Security.Claims;
    using Agent.Application.Authentication.Common;
    using Agent.Application.Common.Interfaces.Authentication;
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Domain.Common.Errors;
    using Agent.Domain.Entities;
    using ErrorOr;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;

    public class RegisterCommandHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IJwtTokenHandler jwtTokenHandler,
        IUnitOfWork unitOfWork,
        ILogger<RegisterCommandHandler> logger)
        : IRequestHandler<RegisterCommand, ErrorOr<AuthResult>>
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly RoleManager<Role> _roleManager = roleManager;
        private readonly IJwtTokenHandler _jwtTokenHandler = jwtTokenHandler;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<RegisterCommandHandler> _logger = logger;

        public async Task<ErrorOr<AuthResult>> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            const string defaultRole = "NativeUser";
            var defaultArea = 1;

            var userRoleRepository = _unitOfWork.GetRepository<UserRole>();
            var userClaimRepository = _unitOfWork.GetRepository<UserClaim>();

            var existingUser = await _userManager.FindByEmailAsync(command.Email);
            if (existingUser is not null)
            {
                return new[] { Errors.Authentication.DuplicateEmailError };
            }

            var user = new User
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                UserName = command.Email,
                PhoneNumber = command.PhoneNumber,
                DefaultAreaId = defaultArea,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var identityResult = await _userManager.CreateAsync(user, command.Password);
                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Failed to create user: {Errors}", string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Error.Unexpected(description: "Failed to create user.");
                }

                if (!await _roleManager.RoleExistsAsync(defaultRole))
                {
                    var newRole = new Role { Name = defaultRole };
                    var roleCreation = await _roleManager.CreateAsync(newRole);
                    if (!roleCreation.Succeeded)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Error.Unexpected(description: "Failed to create role.");
                    }
                }

                var role = await _roleManager.FindByNameAsync(defaultRole);
                if (role != null)
                {
                    // Assign claims manually
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var userClaims = claims.Select(claim => new UserClaim
                    {
                        UserId = user.Id,
                        ClaimType = claim.Type,
                        ClaimValue = claim.Value,
                        AreaId = defaultArea, // This can be dynamic if needed
                    }).ToList();

                    foreach (var userClaim in userClaims)
                    {
                        var exists = await userClaimRepository.ExistsAsync(
                            uc => uc.UserId == userClaim.UserId &&
                                  uc.ClaimType == userClaim.ClaimType &&
                                  uc.ClaimValue == userClaim.ClaimValue &&
                                  uc.AreaId == userClaim.AreaId,
                            cancellationToken);

                        if (!exists)
                        {
                            await userClaimRepository.AddAsync(userClaim, cancellationToken);
                        }
                    }

                    // Assign role manually
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        AreaId = defaultArea, // This can be dynamic if needed
                    };

                    await userRoleRepository.AddAsync(userRole, cancellationToken);
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return new AuthResult(user, null, null, null);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "An exception occurred during user registration.");
                throw;
            }
        }
    }
}
