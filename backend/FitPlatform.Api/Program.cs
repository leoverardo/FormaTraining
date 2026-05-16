using System.Text;
using FitPlatform.Api.Middlewares;
using FitPlatform.Api.Services;
using FitPlatform.Application.Configuration;
using FitPlatform.Application.Common;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using FitPlatform.Infrastructure.Data;
using FitPlatform.Infrastructure.ExternalServices;
using FitPlatform.Infrastructure.PaymentProviders;
using FitPlatform.Infrastructure.Seed;
using FitPlatform.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;
using FitPlatform.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Cloudinary:CloudName"] = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
    ["Cloudinary:ApiKey"] = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
    ["Cloudinary:ApiSecret"] = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET"),
    ["Cloudinary:Folder"] = Environment.GetEnvironmentVariable("CLOUDINARY_FOLDER"),
    ["AbacatePay:BaseUrl"] = Environment.GetEnvironmentVariable("ABACATEPAY_BASE_URL"),
    ["AbacatePay:ApiKey"] = Environment.GetEnvironmentVariable("ABACATEPAY_API_KEY"),
    ["AbacatePay:WebhookSecret"] = Environment.GetEnvironmentVariable("ABACATEPAY_WEBHOOK_SECRET"),
    ["AbacatePay:WebhookPublicKey"] = Environment.GetEnvironmentVariable("ABACATEPAY_WEBHOOK_PUBLIC_KEY"),
    ["AbacatePay:SuccessUrl"] = Environment.GetEnvironmentVariable("ABACATEPAY_SUCCESS_URL"),
    ["AbacatePay:ReturnUrl"] = Environment.GetEnvironmentVariable("ABACATEPAY_RETURN_URL"),
    ["AbacatePay:DevMode"] = Environment.GetEnvironmentVariable("ABACATEPAY_DEV_MODE")
});
builder.Services.Configure<AbacatePayOptions>(builder.Configuration.GetSection(AbacatePayOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IDateTimeProvider, SaoPauloDateTimeProvider>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddHttpClient<IPaymentProvider, AbacatePayPaymentProvider>();
builder.Services.AddScoped<IPaymentWebhookValidator, AbacatePayWebhookValidator>();
builder.Services.AddScoped<IEmailService, ConsoleEmailService>();
builder.Services.AddScoped<PasswordSetupService>();
builder.Services.AddScoped<OnboardingService>();
builder.Services.AddScoped<StudentProgressService>();
builder.Services.AddScoped<PrivacyLgpdService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TrainerService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<ExerciseService>();
builder.Services.AddScoped<WorkoutService>();
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<PlatformPlanService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<StudentAreaService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<CheckInService>();
builder.Services.AddScoped<AnamnesisService>();
builder.Services.AddScoped<WorkoutSessionService>();
builder.Services.AddScoped<StudentMonitoringService>();
builder.Services.AddScoped<ExerciseLibraryService>();
builder.Services.AddScoped<InternalNotesService>();
builder.Services.AddScoped<PublicPageService>();
builder.Services.AddScoped<TrainerLeadService>();
builder.Services.AddScoped<ReportsService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<FeedBuilderService>();
builder.Services.AddScoped<FeedSocialService>();
builder.Services.AddScoped<ExploreService>();
builder.Services.AddScoped<OwnerDashboardService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<HabitService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<GamificationService>();
builder.Services.AddScoped<ServiceSalesService>();

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<CloudinaryStorageService>();
builder.Services.AddScoped<ICloudinaryUrlService, CloudinaryUrlService>();

var rateLimitingOptions = builder.Configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new RateLimitingOptions();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        var message = ApiResponse.Fail("Limite de requisicoes excedido. Tente novamente em instantes.");
        await context.HttpContext.Response.WriteAsJsonAsync(message, cancellationToken: token);
    };

    options.AddPolicy("AuthLogin", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => BuildFixedWindow(rateLimitingOptions.Policies.AuthLogin, 15, 60)));

    options.AddPolicy("StudentRegister", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => BuildFixedWindow(rateLimitingOptions.Policies.StudentRegister, 10, 60)));

    options.AddPolicy("TrainerOnboarding", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => BuildFixedWindow(rateLimitingOptions.Policies.TrainerOnboarding, 20, 60)));

    options.AddPolicy("PublicLead", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => BuildFixedWindow(rateLimitingOptions.Policies.PublicLead, 12, 60)));

    options.AddPolicy("ExplorePublicSearch", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => BuildFixedWindow(rateLimitingOptions.Policies.ExplorePublicSearch, 90, 60)));
});

var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(',') ?? ["http://localhost:5173"])
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FitPlatform API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigration");
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    await DatabaseSeeder.SeedAsync(db);
    try
    {
        await DatabaseSeeder.EnrichExistingDataAsync(db);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database enrichment skipped due to schema mismatch or transient issue.");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static FixedWindowRateLimiterOptions BuildFixedWindow(RateLimitingPolicyOptions? policy, int defaultPermitLimit, int defaultWindowSeconds)
{
    return new FixedWindowRateLimiterOptions
    {
        PermitLimit = policy?.PermitLimit > 0 ? policy.PermitLimit : defaultPermitLimit,
        Window = TimeSpan.FromSeconds(policy?.WindowSeconds > 0 ? policy.WindowSeconds : defaultWindowSeconds),
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = policy?.QueueLimit >= 0 ? policy.QueueLimit : 0
    };
}

