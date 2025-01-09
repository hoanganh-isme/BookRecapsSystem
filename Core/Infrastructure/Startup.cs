using BusinessObject.Data;
using BusinessObject.Models;
using Core.Auth.Permissions;
using Core.Auth.Repository;
using Core.Auth.Services;
using Core.Enums;
using Core.Helpers;
using Core.Infrastructure.Exceptions;
using Core.Infrastructure.FileStorage;
using Core.Infrastructure.Hangfire;
using Core.Infrastructure.Middleware;
using Core.Infrastructure.Notifications;
using Core.Infrastructure.reCAPTCHAv3;
using Core.Infrastructure.Serilog;
using Core.Infrastructure.SpeedSMS;
using Core.Infrastructure.Validator;
using Core.Models.Auditing;
using Core.Properties;
using Core.Repository;
using Core.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using Repository.Interface;
using Repository.Repository;
using SendGrid.Helpers.Errors.Model;
using Serilog;
using Services;
using Services.Interface;
using Services.Service;
using Services.Service.Helper;
using System.Configuration;
using System.Text;

namespace Core.Infrastructure
{
    public static class Startup
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpContextAccessor();
            services.AddReCaptchav3(config);
            services.AddValidators();
            services.AddSpeedSMS(config);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.SaveToken = true;
                option.RequireHttpsMetadata = false;
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    SaveSigninToken = true,
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero,
                };
                option.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        if (!context.Response.HasStarted)
                        {
                            throw new UnauthorizedException("Authentication Failed.");
                        }

                        return Task.CompletedTask;
                    },
                    OnForbidden = _ => throw new ForbiddenException("You are not authorized to access this resource."),
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/notifications"))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                foreach (var permission in Permissions.All)
                {
                    options.AddPolicy(permission.Name, policy =>
                        policy.Requirements.Add(new PermissionRequirement(permission.Name)));
                }
            });

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    config.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Core"));
            });

            services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseFilter(new JobFilter(services.BuildServiceProvider()))
            .UseFilter(new LogJobFilter())
            .UseSqlServerStorage(config.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

            services.AddHangfireServer();

            services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                // Not require username
                options.User.AllowedUserNameCharacters = null;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = ".NET-Core-BookRecaps-API-8.0",
                    Version = "v1",
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                },
                });
            });
            // Cors port http request
            //services.AddCors(options =>
            //{
            //    options.AddPolicy(
            //        "CorsPolicy",
            //        builder => builder
            //            .AllowAnyMethod()
            //            .AllowAnyHeader()
            //            .WithOrigins("https://ai.hieuvo.dev", "https://contributor.hieuvo.dev", "http://localhost:5173", "https://localhost:7124", "http://localhost:*", "https://localhost")
            //            .AllowCredentials());
            //});
            services.AddCors(options =>
            {
                options.AddPolicy(
                    "CorsPolicy",
                    builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowAnyOrigin());
            });

            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddNotifications(config);
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IReCAPTCHAv3Service, ReCAPTCHAv3Service>();
            services.AddTransient<IMailService, MailService>();
            services.AddSingleton<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ISerializerService, SerializerService>();
            services.AddScoped<INotificationSender, NotificationSender>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IRecapRepository, RecapRepository>();
            services.AddScoped<IAppealRepository, AppealRepository>();
            services.AddScoped<IRecapVersionRepository, RecapVersionRepository>();
            services.AddScoped<IHighlightRepository, HighlightRepository>();
            services.AddScoped<IKeyIdeaRepository, KeyIdeaRepository>();   
            services.AddScoped<IPlayListItemRepository, PlayListItemRepository>();   
            services.AddScoped<IPlayListRepository, PlayListRepository>();
            services.AddScoped<IReadingPositionRepository, ReadingPositionRepository>();   
            services.AddScoped<IReviewNoteRepository, ReviewNoteRepository>();   
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();   
            services.AddScoped<ISupportTicketRepository, SupportTicketRepository>(); 
            services.AddScoped<IViewTrackingRepository, ViewTrackingRepository>();  
            services.AddScoped<IBookEarningRepository, BookEarningRepository>();
            services.AddScoped<IRecapEarningRepository, RecapEarningRepository>();
            services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IContractAttachmentRepository, ContractAttachmentRepository>();
            services.AddScoped<IContributorPayoutRepository, ContributorPayoutRepository>();
            services.AddScoped<IContributorWithdrawalRepository, ContributorWithdrawalRepository>();
            services.AddScoped<IPublisherPayoutRepository, PublisherPayoutRepository>();
            services.AddScoped<ISubscriptionPackageRepository, SubscriptionPackageRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IPublisherRepository, PublisherRepository>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IKeyIdeaService, KeyIdeaService>();           
            services.AddScoped<IRecapService, RecapService>();
            services.AddScoped<IRecapVersionService, RecapVersionService>();
            services.AddScoped<IPlayListItemService, PlayListItemService>();
            services.AddScoped<IPlayListService, PlayListService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IReviewNoteService, ReviewNoteService>();
            services.AddScoped<IHighlightService, HighlightService>();
            services.AddScoped<ISupportTicketService, SupportTicketService>();
            services.AddScoped<IViewTrackingService, ViewTrackingService>();
            services.AddScoped<IAppealService, AppealService>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<ISubscriptionPackageService, SubscriptionPackageService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IContractAttachmentService, ContractAttachmentService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IPublisherService, PublisherService>();
            services.AddScoped<IRecapEarningService, RecapEarningService>();
            services.AddScoped<ILikeService, LikeService>();
            services.AddScoped<IContributorPayoutService, ContributorPayoutService>();
            services.AddScoped<IBookEarningService, BookEarningService>();
            services.AddScoped<IPublisherPayoutService, PublisherPayoutService>();
            services.AddScoped<IContributorWithdrawalService, ContributorWithdrawalService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<TokenCleanupJob>();
            services.AddTransient<IDummyService, DummyService>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddSingleton<GoogleCloudService>();
            services.AddSingleton<PayOSService>();
            services.AddHttpClient<IRecapVersionService, RecapVersionService>();

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                options.JsonSerializerOptions.MaxDepth = 128;
            });
            services.Configure<GoogleSettings>(config.GetSection("GoogleSettings"));
            services.Configure<ApiSettings>(config.GetSection("TranscriptApi"));
            services.Configure<PayOSOptions>(config.GetSection("PayOS"));
            services.AddHttpClient("NoCertificateValidation")
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                return handler;
            });
            services.AddSingleton<IUriService>(o =>
            {
                var accessor = o.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request;
                var uri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
                return new UriService(uri);
            });
            services.AddProblemDetails();
            return services;
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
            RecurringJob.AddOrUpdate<TokenCleanupJob>("CleanupTokens", job => job.CleanupTokens(), Cron.Daily);
            RecurringJob.AddOrUpdate<IContractService>(
                "auto-update-contracts", // Tên công việc
                service => service.AutoUpdateContractsAsync(), // Phương thức cần thực thi
                Cron.Daily
            );
            RecurringJob.AddOrUpdate<ISubscriptionService>(
                "update-subscription-statuses",
                service => service.UpdateSubscriptionStatusesAsync(),
                Cron.Daily
                );
            app.UseMiddleware<TokenRevokedMiddleware>();
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseCors("CorsPolicy");
            app.UseExceptionHandler();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Files")),
                RequestPath = new PathString("/Files")
            });
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }

        public static void Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
                InitializeRoles(services).Wait();
            }
        }

        private static async Task InitializeRoles(IServiceProvider services)
        {
            var roleNames = Enum.GetNames(typeof(Roles));

            try
            {
                await SeedRole.Initialize(services, roleNames);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred seeding the DB.");
            }
        }
    }
}