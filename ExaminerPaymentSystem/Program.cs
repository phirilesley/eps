using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Extensions;
using ExaminerPaymentSystem.Models.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure DbContext with increased timeouts and retry logic
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            // Increase command timeout to 5 minutes (300 seconds)
            sqlOptions.CommandTimeout(300);

            // Enable resilient SQL connections with retry logic
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

// Only use this in development environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}


builder.Services.AddDefaultIdentity< ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
      .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false; // Allow passwords without digits
    options.Password.RequireUppercase = false; // Disable uppercase letter requirement
    options.Password.RequireLowercase = true; // Enforce lowercase requirement
    options.Password.RequireNonAlphanumeric = true; // Enforce special characters
    options.Password.RequiredLength = 6; // Minimum length for passwords
    
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Path to your login page
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });


//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.PropertyNamingPolicy = null;
//    });


builder.Services.AddControllersWithViews();

builder.Services
    .AddExaminerPaymentsModule()
    .AddSpecialNeedsModule()
    .AddExaminerRecruitmentModule()
    .AddExamMonitorModule()
    .AddNavigationModuleCatalog();

builder.Services.Configure<AppInfo>(builder.Configuration.GetSection("AppInfo"));


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SupervisorPolicy", policy =>
        policy.RequireRole("RegionalManager"));

    options.AddPolicy("HRPolicy", policy =>
        policy.RequireRole("HR"));

    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin"));
});



builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

});

builder.Services.AddHttpContextAccessor();



var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    //app.UseElmahExceptionPage();
    //app.UseExceptionHandler("/Error/ServerError");
    app.UseDeveloperExceptionPage();


}
else
{

  
    app.UseExceptionHandler("/Error/ServerError");
    //app.UseStatusCodePagesWithReExecute("/Error/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "PMS", "Examiner", "Accounts","BMS","RPMS","DPMS","SubjectManager", "CentreSupervisor","PeerReviewer","AssistantAccountant" ,"BT","A","PBT","OfficerSpecialNeeds",
        "SuperAdmin","HR","ExamsAdmin","ResidentMonitor","ClusterManager","AssistantClusterManager","RegionalManager","Directorate"};
        
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            IdentityRole roleRole = new IdentityRole(role);
            await roleManager.CreateAsync(roleRole);
        }
    }

}

app.UseSession();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();



app.MapGet("/test", () => "Routing works");
app.Run();

