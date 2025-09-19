// <copyright file="AuthController.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.Controllers
{
    using Agent.Application.Authentication.Commands.Register;
    using Agent.Application.Authentication.Common;
    using Agent.Application.Authentication.Queries.Login;
    using Agent.Application.Authentication.Queries.Renewal;
    using Agent.Contracts.Authentication;
    using Agent.Domain.Common.Errors;
    using ErrorOr;
    using MapsterMapper;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("auth")]
    [AllowAnonymous]

    public class AuthController : ApiController
    {
        private const string RefreshTokenCookieName = "refreshToken";

        private readonly ISender _mediator;

        private readonly IMapper _mapper;

        public AuthController(ISender mediator, IMapper mapper) => (this._mediator, this._mapper) = (mediator, mapper);

        [HttpPost("register")]

        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var command = this._mapper.Map<RegisterCommand>(request);
            ErrorOr<AuthResult> registerResult = await this._mediator.Send(command);

            if (registerResult.IsError && registerResult.FirstError.Type == ErrorType.Validation)
            {
                return this.Problem(statusCode: StatusCodes.Status400BadRequest, title: registerResult.FirstError.Description);
            }

            if (registerResult.IsError && registerResult.FirstError == Errors.Authentication.DuplicateEmailError)
            {
                return this.Problem(statusCode: StatusCodes.Status409Conflict, title: registerResult.FirstError.Description);
            }

            return registerResult.Match(
                registerResult => this.Ok(this._mapper.Map<AuthResponse>(registerResult)),
                error => this.Problem(error));
        }

        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
            {
                return this.BadRequest("Request body cannot be null.");
            }

            var query = this._mapper.Map<LoginQuery>(request);

            ErrorOr<AuthResult> loginResult = await this._mediator.Send(query);

            if (loginResult.IsError && loginResult.FirstError.Type == ErrorType.Validation)
            {
                return this.Problem(statusCode: StatusCodes.Status400BadRequest, title: loginResult.FirstError.Description);
            }

            if (loginResult.IsError && loginResult.FirstError == Errors.Authentication.InvalidCredentials)
            {
                return this.Problem(statusCode: StatusCodes.Status401Unauthorized, title: loginResult.FirstError.Description);
            }

            if (!loginResult.IsError && loginResult.Value.RefreshToken != null)
            {
                // Append refresh token to HttpOnly cookie
                SetRefreshTokenCookie(loginResult.Value.RefreshToken, loginResult.Value.RefreshTokenExpires ?? 0);
            }

            return loginResult.Match(
                authResult => this.Ok(this._mapper.Map<AuthResponse>(authResult)),
                error => this.Problem(error));
        }

        [HttpPost("renewal")]
        public async Task<IActionResult> RefreshTokens()
        {
            if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken))
            {
                return Unauthorized("Refresh token not found.");
            }

            var query = new RenewalQuery(refreshToken); // Define this query in your app layer
            var result = await _mediator.Send(query);

            if (result.IsError)
            {
                return this.Problem(statusCode: StatusCodes.Status401Unauthorized, title: result.FirstError.Description);
            }

            if (!result.IsError && result.Value.RefreshToken != null)
            {
                // Append refresh token to HttpOnly cookie
                SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpires ?? 0);
            }

            return result.Match(
                authResult => this.Ok(this._mapper.Map<AuthResponse>(authResult)),
                error => this.Problem(error));
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            });

            return NoContent(); // 204
        }

        private void SetRefreshTokenCookie(string refreshToken, long expiresAt)
        {
            var unixTimestamp = DateTimeOffset.FromUnixTimeSeconds(expiresAt);
            Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = unixTimestamp,
            });
        }
    }
}
