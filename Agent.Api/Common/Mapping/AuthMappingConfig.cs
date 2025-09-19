// <copyright file="AuthMappingConfig.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.Common.Mapping
{
    using Agent.Application.Authentication.Common;
    using Agent.Contracts.Authentication;
    using Mapster;

    public class AuthMappingConfig : IRegister
    {
        public AuthMappingConfig()
        {
        }

        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AuthResult, AuthResponse>()

                // .Map(d => d.Token, s => s.Token)
                .Map(d => d, s => s.User);
        }
    }
}
