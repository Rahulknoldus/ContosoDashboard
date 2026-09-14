using Microsoft.EntityFrameworkCore;
using ContosoDashboard.Data;
using ContosoDashboard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add authentication state provider for Blazor
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Mock Authentication (Cookie-based for training purposes)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Employee", policy => policy.RequireRole("Employee", "TeamLead", "ProjectManager", "Administrator"));
    options.AddPolicy("TeamLead", policy => policy.RequireRole("TeamLead", "ProjectManager", "Administrator"));
    options.AddPolicy("ProjectManager", policy => policy.RequireRole("ProjectManager", "Administrator"));
    options.AddPolicy("Administrator", policy => policy.RequireRole("Administrator"));
});

// Register application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IScanQueueService, AzureQueueScanService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddControllers();

// Add HttpContextAccessor for accessing user claims
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated(); // For development - use migrations in production
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Documents (
                DocumentId INTEGER NOT NULL CONSTRAINT PK_Documents PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT NULL,
                Category TEXT NOT NULL,
                ProjectId INTEGER NULL,
                TaskId INTEGER NULL,
                UploadedByUserId INTEGER NOT NULL,
                FileName TEXT NOT NULL,
                StoredFilePath TEXT NOT NULL,
                FileSizeBytes INTEGER NOT NULL,
                MimeType TEXT NOT NULL,
                UploadedAtUtc TEXT NOT NULL,
                UpdatedAtUtc TEXT NOT NULL,
                IsDeleted INTEGER NOT NULL,
                Status INTEGER NOT NULL,
                ScanStatus INTEGER NOT NULL,
                CONSTRAINT FK_Documents_Users_UploadedByUserId FOREIGN KEY (UploadedByUserId) REFERENCES Users (UserId) ON DELETE RESTRICT,
                CONSTRAINT FK_Documents_Projects_ProjectId FOREIGN KEY (ProjectId) REFERENCES Projects (ProjectId) ON DELETE SET NULL,
                CONSTRAINT FK_Documents_Tasks_TaskId FOREIGN KEY (TaskId) REFERENCES Tasks (TaskId) ON DELETE SET NULL
            );
            CREATE TABLE IF NOT EXISTS DocumentShares (
                DocumentShareId INTEGER NOT NULL CONSTRAINT PK_DocumentShares PRIMARY KEY AUTOINCREMENT,
                DocumentId INTEGER NOT NULL,
                SharedWithUserId INTEGER NOT NULL,
                SharedByUserId INTEGER NOT NULL,
                SharedAtUtc TEXT NOT NULL,
                Message TEXT NULL,
                IsActive INTEGER NOT NULL,
                CONSTRAINT FK_DocumentShares_Documents_DocumentId FOREIGN KEY (DocumentId) REFERENCES Documents (DocumentId) ON DELETE CASCADE,
                CONSTRAINT FK_DocumentShares_Users_SharedWithUserId FOREIGN KEY (SharedWithUserId) REFERENCES Users (UserId) ON DELETE RESTRICT,
                CONSTRAINT FK_DocumentShares_Users_SharedByUserId FOREIGN KEY (SharedByUserId) REFERENCES Users (UserId) ON DELETE RESTRICT
            );
            CREATE TABLE IF NOT EXISTS DocumentAccessLogs (
                LogId INTEGER NOT NULL CONSTRAINT PK_DocumentAccessLogs PRIMARY KEY AUTOINCREMENT,
                DocumentId INTEGER NOT NULL,
                UserId INTEGER NOT NULL,
                ActionType TEXT NOT NULL,
                ActionAtUtc TEXT NOT NULL,
                Details TEXT NULL,
                CONSTRAINT FK_DocumentAccessLogs_Documents_DocumentId FOREIGN KEY (DocumentId) REFERENCES Documents (DocumentId) ON DELETE CASCADE,
                CONSTRAINT FK_DocumentAccessLogs_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (UserId) ON DELETE RESTRICT
            );
            CREATE TABLE IF NOT EXISTS ScanJobMessages (
                ScanJobMessageId INTEGER NOT NULL CONSTRAINT PK_ScanJobMessages PRIMARY KEY AUTOINCREMENT,
                DocumentId INTEGER NOT NULL,
                QueueName TEXT NOT NULL,
                Payload TEXT NOT NULL,
                State INTEGER NOT NULL,
                CreatedAtUtc TEXT NOT NULL,
                ProcessedAtUtc TEXT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_Documents_UploadedByUserId ON Documents (UploadedByUserId);
            CREATE INDEX IF NOT EXISTS IX_Documents_ProjectId ON Documents (ProjectId);
            CREATE INDEX IF NOT EXISTS IX_Documents_ScanStatus ON Documents (ScanStatus);
            CREATE INDEX IF NOT EXISTS IX_DocumentShares_DocumentId_SharedWithUserId_IsActive ON DocumentShares (DocumentId, SharedWithUserId, IsActive);
            CREATE INDEX IF NOT EXISTS IX_DocumentAccessLogs_DocumentId_ActionAtUtc ON DocumentAccessLogs (DocumentId, ActionAtUtc);
        ");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // Use HSTS even in development for training purposes
    app.UseHsts();
}

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    
    // Content Security Policy for Blazor Server
    context.Response.Headers["Content-Security-Policy"] = 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self' wss: ws:;";
    
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
