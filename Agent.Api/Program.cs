// <copyright file="Program.cs" company="Agent">
// © Agent 2025
// </copyright>

using Agent.Api;
using Agent.Application;
using Agent.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
{
    builder.Services
        .AddPresentation(builder.Configuration)
        .AddApplication()
        .AddInfrastructure(builder.Configuration);

    // Configure CORS to allow Angular app running on localhost:4200
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAgentUI", policy =>
        {
            policy.WithOrigins("https://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // Swagger/OpenAPI setup
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(static opt =>
    {
        opt.SwaggerDoc("v1", new OpenApiInfo { Title = "AgentAPI", Version = "v1" });

        // Optional: XML comments
        // var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        // opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer",
        });

        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        });
    });
}

var app = builder.Build();
{
    // Initialize DB (migrations, seeding)
    Agent.Infrastructure.Persistence.AppDbInitializer.Initialize(app.Services);

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Global exception handler
    // app.UseMiddleware<ErrorHandler>();
    app.UseExceptionHandler("/error");

    // app.MapHealthChecks("/health");

    // app.Map("/error", (HttpContext httpContext) =>
    // {
    //    Exception? exception = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
    //    return Results.Problem();
    // }
    // );
    app.UseHttpsRedirection();

    app.UseStaticFiles();

    // ✅ Add Custom Security Headers Middleware
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Frame-Options"] = "DENY"; // Prevents clickjacking
        context.Response.Headers["X-Content-Type-Options"] = "nosniff"; // Blocks MIME-type sniffing
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin"; // Limits referrer info
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()"; // Restrict browser features

        // ✅ Secure Content-Security-Policy (CSP)
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " + // Allow only same-origin content
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " + // Restrict JavaScript sources
            "style-src 'self' 'unsafe-inline'; " + // Restrict CSS sources
            "img-src 'self' data:; " + // Allow images from same origin and data URIs
            "frame-ancestors 'none';"; // Prevent embedding in iframes

        await next();
    });

    // Middleware order: UseRouting -> UseCors -> Authentication -> Authorization
    app.UseRouting();

    app.UseCors("AllowAgentUI");

    app.UseAuthentication();

    app.UseMiddleware<JtiValidationMiddleware>();

    app.UseAuthorization();

    app.UseHttpVerbPolicyMiddleware();

    app.UseResponseCompression();

    app.MapControllers();

    // 👉 Add this to support Angular deep links
    app.MapFallbackToFile("index.html");

    app.Run();
}
