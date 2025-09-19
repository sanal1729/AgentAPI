// <copyright file="AppDbInitializer.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence
{
    using System;
    using System.Linq;
    using Agent.Domain.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class AppDbInitializer
    {
        public static void Initialize(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInit");

            try
            {
                if (!dbContext.Database.CanConnect())
                {
                    // Database doesn't exist or can't connect — create from model
                    logger.LogInformation("Database not found. Creating with EnsureCreated...");
                    dbContext.Database.EnsureCreated();
                }
                else
                {
                    // Database exists — apply pending migrations if any
                    var pendingMigrations = dbContext.Database.GetPendingMigrations();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                        dbContext.Database.Migrate();
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations to apply.");
                    }
                }

                // Seed the database
                SeedData(dbContext, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        private static void SeedData(AppDbContext dbContext, ILogger logger)
        {
            try
            {
                // Seed Roles if not already present
                if (!dbContext.Roles.Any())
                {
                    logger.LogInformation("Seeding Roles...");
                    dbContext.Roles.AddRange(
                        new Role
                        {
                            Id = "0",
                            Name = "Admin",
                            NormalizedName = "ADMIN",
                            ConcurrencyStamp = Guid.CreateVersion7().ToString(),
                        },
                        new Role
                        {
                            Id = "1",
                            Name = "NativeUser",
                            NormalizedName = "NATIVEUSER",
                            ConcurrencyStamp = Guid.CreateVersion7().ToString(),
                        },
                        new Role
                        {
                            Id = "2",
                            Name = "ExternalUser",
                            NormalizedName = "EXTERNALUSER",
                            ConcurrencyStamp = Guid.CreateVersion7().ToString(),
                        });

                    dbContext.SaveChanges();
                }

                // Seed Entitlements if not already present
                if (!dbContext.Entitlements.Any())
                {
                    try
                    {
                        logger.LogInformation("Seeding Entitlements...");

                        dbContext.Entitlements.AddRange(
                            new Entitlement(0, "R", "Read", "Allows user to read resources", "Permission", "CanRead"),
                            new Entitlement(1, "C", "Create", "Allows user to create resources", "Permission", "CanCreate"),
                            new Entitlement(2, "U", "Update", "Allows user to update resources", "Permission", "CanUpdate"),
                            new Entitlement(3, "D", "Delete", "Allows user to delete resources", "Permission", "CanDelete"),
                            new Entitlement(4, "CS", "ConfigureSettings", "Allows user to configure system settings", "Feature", "ConfigureSettings"));

                        dbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred while setting IDENTITY_INSERT for Entitlements.");
                    }
                }

                // Seed Role Claims if not already present
                if (!dbContext.RoleClaims.Any())
                {
                    try
                    {
                        logger.LogInformation("Seeding Role Claims...");
                        dbContext.RoleClaims.AddRange(
                            new IdentityRoleClaim<string>
                            {
                                Id = 1,
                                RoleId = "0",
                                ClaimType = "Permission",
                                ClaimValue = "CanRead",
                            },
                            new IdentityRoleClaim<string>
                            {
                                Id = 2,
                                RoleId = "0",
                                ClaimType = "Permission",
                                ClaimValue = "CanCreate",
                            },
                            new IdentityRoleClaim<string>
                            {
                                Id = 3,
                                RoleId = "0",
                                ClaimType = "Permission",
                                ClaimValue = "CanUpdate",
                            },
                            new IdentityRoleClaim<string>
                            {
                                Id = 4,
                                RoleId = "0",
                                ClaimType = "Permission",
                                ClaimValue = "CanDelete",
                            },
                            new IdentityRoleClaim<string>
                            {
                                Id = 5,
                                RoleId = "1",
                                ClaimType = "Permission",
                                ClaimValue = "CanRead",
                            },
                            new IdentityRoleClaim<string>
                            {
                                Id = 6,
                                RoleId = "1",
                                ClaimType = "Permission",
                                ClaimValue = "CanCreate",
                            },
                            new IdentityRoleClaim<string>
                            {
                                Id = 7,
                                RoleId = "1",
                                ClaimType = "Permission",
                                ClaimValue = "CanUpdate",
                            },
                            new IdentityRoleClaim<string>
                            {
                                Id = 8,
                                RoleId = "2",
                                ClaimType = "Permission",
                                ClaimValue = "CanRead",
                            });

                        dbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred while setting IDENTITY_INSERT for RoleClaims.");
                    }
                }

                // Seed Areas if not already present
                if (!dbContext.Areas.Any())
                {
                    try
                    {
                        logger.LogInformation("Seeding Areas (Continents, Countries, States, Cities)...");

                        var areas = new List<Area>();

                        // Continents (level 0)
                        var northAmerica = new Area(1, "North America", "NA");
                        northAmerica.SetPathAndLevel(string.Empty, -1);
                        areas.Add(northAmerica);

                        var asia = new Area(53, "Asia", "AS");
                        asia.SetPathAndLevel(string.Empty, -1);
                        areas.Add(asia);

                        // Countries (level 1)
                        var usa = new Area(2, "United States", "US", 1);
                        usa.SetPathAndLevel(northAmerica.Path, northAmerica.Level);
                        areas.Add(usa);

                        var india = new Area(54, "India", "IN", 53);
                        india.SetPathAndLevel(asia.Path, asia.Level);
                        areas.Add(india);

                        // US States (level 2)
                        var states = new List<Area>
                                    {
                                        new Area(3, "Alabama", "US-AL", 2),
                                        new Area(4, "Alaska", "US-AK", 2),
                                        new Area(5, "Arizona", "US-AZ", 2),
                                        new Area(6, "Arkansas", "US-AR", 2),
                                        new Area(7, "California", "US-CA", 2),
                                        new Area(8, "Colorado", "US-CO", 2),
                                        new Area(9, "Connecticut", "US-CT", 2),
                                        new Area(10, "Delaware", "US-DE", 2),
                                        new Area(11, "Florida", "US-FL", 2),
                                        new Area(12, "Georgia", "US-GA", 2),
                                        new Area(13, "Hawaii", "US-HI", 2),
                                        new Area(14, "Idaho", "US-ID", 2),
                                        new Area(15, "Illinois", "US-IL", 2),
                                        new Area(16, "Indiana", "US-IN", 2),
                                        new Area(17, "Iowa", "US-IA", 2),
                                        new Area(18, "Kansas", "US-KS", 2),
                                        new Area(19, "Kentucky", "US-KY", 2),
                                        new Area(20, "Louisiana", "US-LA", 2),
                                        new Area(21, "Maine", "US-ME", 2),
                                        new Area(22, "Maryland", "US-MD", 2),
                                        new Area(23, "Massachusetts", "US-MA", 2),
                                        new Area(24, "Michigan", "US-MI", 2),
                                        new Area(25, "Minnesota", "US-MN", 2),
                                        new Area(26, "Mississippi", "US-MS", 2),
                                        new Area(27, "Missouri", "US-MO", 2),
                                        new Area(28, "Montana", "US-MT", 2),
                                        new Area(29, "Nebraska", "US-NE", 2),
                                        new Area(30, "Nevada", "US-NV", 2),
                                        new Area(31, "New Hampshire", "US-NH", 2),
                                        new Area(32, "New Jersey", "US-NJ", 2),
                                        new Area(33, "New Mexico", "US-NM", 2),
                                        new Area(34, "New York", "US-NY", 2),
                                        new Area(35, "North Carolina", "US-NC", 2),
                                        new Area(36, "North Dakota", "US-ND", 2),
                                        new Area(37, "Ohio", "US-OH", 2),
                                        new Area(38, "Oklahoma", "US-OK", 2),
                                        new Area(39, "Oregon", "US-OR", 2),
                                        new Area(40, "Pennsylvania", "US-PA", 2),
                                        new Area(41, "Rhode Island", "US-RI", 2),
                                        new Area(42, "South Carolina", "US-SC", 2),
                                        new Area(43, "South Dakota", "US-SD", 2),
                                        new Area(44, "Texas", "US-TX", 2),
                                        new Area(45, "Utah", "US-UT", 2),
                                        new Area(46, "Vermont", "US-VT", 2),
                                        new Area(47, "Virginia", "US-VA", 2),
                                        new Area(48, "Washington", "US-WA", 2),
                                        new Area(49, "West Virginia", "US-WV", 2),
                                        new Area(50, "Wisconsin", "US-WI", 2),
                                        new Area(51, "Wyoming", "US-WY", 2),
                                    };

                        // Set path and level for each state and add to list
                        foreach (var state in states)
                        {
                            state.SetPathAndLevel(usa.Path, usa.Level);
                            areas.Add(state);
                        }

                        // Cities in Texas (level 3)
                        var texas = states.First(s => s.Id == 44);

                        var texasCities = new List<Area>
                                            {
                                                new Area(1001, "Houston", "US-TX-HOU", 44),
                                                new Area(1002, "Dallas", "US-TX-DAL", 44),
                                                new Area(1003, "Austin", "US-TX-AUS", 44),
                                            };

                        foreach (var city in texasCities)
                        {
                            city.SetPathAndLevel(texas.Path, texas.Level);
                            areas.Add(city);
                        }

                        // Add all areas to DbContext and save
                        dbContext.Areas.AddRange(areas);
                        dbContext.SaveChanges();

                        logger.LogInformation("Areas seeded successfully.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error occurred while seeding Areas.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}
