using System.Reflection;
using System.Security.Claims;
using BusinessObject.Data;
using BusinessObject.Models;
using Core.Auth.Permissions;
using Core.Auth.Services;
using Core.Infrastructure.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Core.Enums
{
    public static class SeedRole
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string[] roleNames)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }
            await SeedRoleClaimsAsync(roleManager);
            await SeedBasicUserAsync(userManager, roleManager);
            await InitializeAsync(serviceProvider);
        }

        public static async Task SeedBasicUserAsync(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            var rootPassword = "123Pa$$word!";
            var image_url = "";

            var admin = new User
            {
                    Id = Guid.Parse("9B1E5C73-7803-40A2-1819-08DCEF1B7345"),
                    FullName = "Admin",
                    UserName = "admin",
                    BirthDate = new DateOnly(2001, 1, 1),
                    PhoneNumber = "0942705605",
                    Email = "admin@root.com",
                    EmailConfirmed = true,
                    ImageUrl = image_url
                };
            
            var staff = new User
                {
                    Id = Guid.Parse("AF40D552-188E-464D-181A-08DCEF1B7345"),
                    FullName = "Staff",
                    UserName = "staff",
                    BirthDate = new DateOnly(2001, 1, 1),
                    PhoneNumber = "0942705605",
                    Email = "staff@root.com",
                    EmailConfirmed = true,
                    ImageUrl = image_url
                };
            
            var contributor = new User
                {
                    Id = Guid.Parse("9F3B6EEA-B51B-4E1F-181B-08DCEF1B7345"),
                    FullName = "contributor",
                    UserName = "contributor",
                    BirthDate = new DateOnly(2001, 1, 1),
                    PhoneNumber = "0942705605",
                    Email = "contributor@root.com",
                    EmailConfirmed = true,
                    ImageUrl = image_url
                };
            var publisher = new User
            {
                Id = Guid.Parse("7479F909-F6DA-41F6-181C-08DCEF1B7345"),
                FullName = "publisher",
                UserName = "publisher",
                BirthDate = new DateOnly(2001, 1, 1),
                PhoneNumber = "0942705605",
                Email = "publisher@root.com",
                EmailConfirmed = true,
                ImageUrl = image_url
            };

            var customer = new User
                {
                    Id = Guid.Parse("12B1CDC3-EDD3-4D10-181D-08DCEF1B7345"),
                    FullName = "Customer",
                    UserName = "customer",
                    BirthDate = new DateOnly(2001, 1, 1),
                    PhoneNumber = "0942705605",
                    Email = "customer@root.com",
                    EmailConfirmed = true,
                    ImageUrl = image_url
                };

            var result = await userManager.CreateAsync(admin, rootPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.SuperAdmin.ToString());
            } else {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create admin user. Errors: {errors}");
            }

            result = await userManager.CreateAsync(staff, rootPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(staff, Roles.Staff.ToString());
            } else {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create staff user. Errors: {errors}");
            }

            result = await userManager.CreateAsync(contributor, rootPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(contributor, Roles.Contributor.ToString());
            } else {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create contributor user. Errors: {errors}");
            }
            result = await userManager.CreateAsync(publisher, rootPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(publisher, Roles.Publisher.ToString());
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create publisher user. Errors: {errors}");
            }

            result = await userManager.CreateAsync(customer, rootPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customer, Roles.Customer.ToString());
            } else {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create customer user. Errors: {errors}");
            }
        }

        public static async Task SeedRoleClaimsAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            foreach (var role in Enum.GetValues(typeof(Roles)).Cast<Roles>())
            {
                var roleEntity = await roleManager.FindByNameAsync(role.ToString());
                if (roleEntity == null)
                {
                    continue; // Skip if role doesn't exist
                }

                // Get the permissions for the current role
                IReadOnlyList<Permission> rolePermissions;
                switch (role)
                {
                    case Roles.SuperAdmin:
                        rolePermissions = Permissions.All;
                        break;
                    case Roles.Staff:
                        rolePermissions = Permissions.Staff;
                        break;
                    case Roles.Contributor:
                        rolePermissions = Permissions.Contributor;
                        break;
                    case Roles.Publisher:
                        rolePermissions = Permissions.Publisher;
                        break;
                    case Roles.Customer:
                        rolePermissions = Permissions.Customer;
                        break;
                    case Roles.Guest:
                        rolePermissions = Permissions.Guest;
                        break;
                    default:
                        rolePermissions = new List<Permission>().AsReadOnly(); // Empty list for unknown roles
                        break;
                }

                // Remove existing claims for this role
                var existingClaims = await roleManager.GetClaimsAsync(roleEntity);
                foreach (var claim in existingClaims)
                {
                    await roleManager.RemoveClaimAsync(roleEntity, claim);
                }

                // Add new claims based on the role's permissions
                foreach (var permission in rolePermissions)
                {
                    await roleManager.AddClaimAsync(roleEntity, new Claim(CustomClaimTypes.Permission, permission.Name));
                }
            }
        }

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path!, "Infrastructure");
            string dataPath = Path.Combine(path!, "Notifications", "NotificationData.json");
            var _logger = serviceProvider.GetRequiredService<ILogger<NotificationSeeder>>();
            var _serializerService = serviceProvider.GetRequiredService<ISerializerService>();
            var _db = serviceProvider.GetRequiredService<AppDbContext>();
            if (!_db.Notifications.Any())
            {
                _logger.LogInformation("Started to Seed Notifications.");
                string notificationData = await File.ReadAllTextAsync(dataPath);
                var notifications = _serializerService.Deserialize<List<Notification>>(notificationData);
                var users = await _db.Users.Where(u => u.UserName == "admin").FirstOrDefaultAsync();
                foreach (var notification in notifications)
                {
                    notification.UserId = users.Id;
                    _db.Notifications.Add(notification);
                }
            }
            await _db.SaveChangesAsync();

            _logger.LogInformation("Seeded Notifications.");
        }
    }
}