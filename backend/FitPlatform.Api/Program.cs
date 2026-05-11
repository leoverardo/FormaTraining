using System.Text;
using FitPlatform.Api.Middlewares;
using FitPlatform.Api.Services;
using FitPlatform.Application.Configuration;
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

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Cloudinary:CloudName"] = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
    ["Cloudinary:ApiKey"] = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
    ["Cloudinary:ApiSecret"] = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET"),
    ["Cloudinary:Folder"] = Environment.GetEnvironmentVariable("CLOUDINARY_FOLDER"),
    ["MercadoPago:AccessToken"] = Environment.GetEnvironmentVariable("MERCADOPAGO_ACCESS_TOKEN"),
    ["MercadoPago:PublicKey"] = Environment.GetEnvironmentVariable("MERCADOPAGO_PUBLIC_KEY"),
    ["MercadoPago:WebhookSecret"] = Environment.GetEnvironmentVariable("MERCADOPAGO_WEBHOOK_SECRET"),
    ["MercadoPago:NotificationUrl"] = Environment.GetEnvironmentVariable("MERCADOPAGO_NOTIFICATION_URL"),
    ["MercadoPago:SuccessUrl"] = Environment.GetEnvironmentVariable("MERCADOPAGO_SUCCESS_URL"),
    ["MercadoPago:FailureUrl"] = Environment.GetEnvironmentVariable("MERCADOPAGO_FAILURE_URL"),
    ["MercadoPago:PendingUrl"] = Environment.GetEnvironmentVariable("MERCADOPAGO_PENDING_URL")
});
builder.Services.Configure<MercadoPagoOptions>(builder.Configuration.GetSection(MercadoPagoOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddHttpClient<IPaymentProvider, MercadoPagoPaymentProvider>();
builder.Services.AddScoped<IMercadoPagoWebhookValidator, MercadoPagoWebhookValidator>();
builder.Services.AddScoped<IEmailService, ConsoleEmailService>();
builder.Services.AddScoped<PasswordSetupService>();
builder.Services.AddScoped<OnboardingService>();
builder.Services.AddScoped<StudentProgressService>();

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

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<CloudinaryStorageService>();
builder.Services.AddScoped<ICloudinaryUrlService, CloudinaryUrlService>();

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

    await EnsureTrainerExploreColumnsAsync(db, logger);

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task EnsureTrainerExploreColumnsAsync(AppDbContext db, ILogger logger)
{
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
IF COL_LENGTH('Trainers', 'AcceptingStudents') IS NULL
    ALTER TABLE [Trainers] ADD [AcceptingStudents] bit NOT NULL CONSTRAINT [DF_Trainers_AcceptingStudents] DEFAULT(1);
IF COL_LENGTH('Trainers', 'PublicSearchEnabled') IS NULL
    ALTER TABLE [Trainers] ADD [PublicSearchEnabled] bit NOT NULL CONSTRAINT [DF_Trainers_PublicSearchEnabled] DEFAULT(0);
IF COL_LENGTH('Trainers', 'Latitude') IS NULL
    ALTER TABLE [Trainers] ADD [Latitude] float NULL;
IF COL_LENGTH('Trainers', 'Longitude') IS NULL
    ALTER TABLE [Trainers] ADD [Longitude] float NULL;
IF COL_LENGTH('Trainers', 'ServiceMode') IS NULL
    ALTER TABLE [Trainers] ADD [ServiceMode] nvarchar(50) NULL;
");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not ensure explore columns in Trainers table at startup.");
    }
}
