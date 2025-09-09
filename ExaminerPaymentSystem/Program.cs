using DinkToPdf;
using DinkToPdf.Contracts;

using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Common;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Interfaces.ExamMonitors;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Interfaces.Transcribers;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Repositories.Common;
using ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys;
using ExaminerPaymentSystem.Repositories.Examiners;
using ExaminerPaymentSystem.Repositories.ExamMonitors;
using ExaminerPaymentSystem.Services;
using ExaminerPaymentSystem.Services.ExamMonitors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Serilog;
using System.Text.Json;
using XmlFileErrorLog = ElmahCore.XmlFileErrorLog;

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

//Examiner Payment
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExaminerRepository, ExaminerRepository>();
builder.Services.AddScoped<ITandSRepository,TandSRepository>();
builder.Services.AddScoped<ITandSDetailsRepository,TandSDetailRepository>();
builder.Services.AddScoped<ITransactionRepository,TransactionRepository>();
builder.Services.AddScoped<ICategoryRateRepository,CategoryRateRepository>();
builder.Services.AddScoped<IPaperMarkingRateRepository,PaperMarkingRateRepository>();
builder.Services.AddScoped<ISubjectsRepository, SubjectsRepository>();
builder.Services.AddScoped<IExamCodesRepository, ExamCodesRepository>();
builder.Services.AddScoped<IAdvanceFeesRepository,AdvanceFeesRepository>();
builder.Services.AddScoped<IMarksCapturedRepository, MarksCapturedRepository>();   
builder.Services.AddScoped<IRateAndTaxRepository, RateAndTaxRepository>();
builder.Services.AddScoped<IBanksRepository, BanksRepository>();
builder.Services.AddScoped<ITandSFilesRepository, TandSFilesRepository>();
builder.Services.AddScoped<IVenueRepository, VenueRepository>();
builder.Services.AddScoped<IAuditTrailRepository,AuditTrailRepository>();
builder.Services.AddScoped<IValidateTandS, ValidateTandSRepository>();
builder.Services.AddScoped<IRegisterRepository, RegisterRepository>();
builder.Services.AddScoped<IApportionmentRepository,ApportionmentRepository>();
builder.Services.AddScoped<IMaxExaminerCodeRepository, MaxExaminerCodeRepository>();
builder.Services.AddScoped<IBrailleTranscriptionRateRepository, BrailleTranscriptionRateRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<ITranscribersRepository,TranscribersRepository>();
builder.Services.AddScoped<IManageTeamsRepository, ManageTeamsRepository>();
builder.Services.AddTransient<IPdfService, PdfService>();
builder.Services.AddTransient<IExportService, ExportService>();
builder.Services.AddTransient<IMaterialRepository, MaterialRepository>();
builder.Services.AddTransient<IMaterialTransactionRepository, MaterialTransactionRepository>();
builder.Services.AddTransient<ICategoryCheckInCheckOutRepository,CategoryCheckInCheckOutRepository>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.Configure<AppInfo>(builder.Configuration.GetSection("AppInfo"));

// In ConfigureServices method



//examiner Recruitment
builder.Services.AddScoped<IExaminerRecruitmentRepository, ExaminerRecruitmentRepository>();
builder.Services.AddScoped<ITeachingExperienceRepository, TeachingExperienceRepository>();
builder.Services.AddScoped<IExaminerRecruitmentAttachmentsRepository, ExaminerRecruitmentAttachmentsRepository>();
builder.Services.AddScoped<IExaminerRecruitmentTrainingSelectionRepository, ExaminerRecruitmentTrainingSelectionRepository>();
builder.Services.AddScoped<IExaminerRecruitmentRegisterRepository, ExaminerRecruitmentRegisterRepository>();
builder.Services.AddScoped<IExaminerRecruitmentAssessmentRepository, ExaminerRecruitmentAssessmentRepository>();
builder.Services.AddScoped<IExaminerRecruitmentVenueDetailsRepository, ExaminerRecruitmentVenueDetailsRepository>();
builder.Services.AddScoped<IExaminerRecruitmentProfessionalQualifications, ExaminerRecruitmentProfessionalQualifications>();
builder.Services.AddScoped<IExaminerRecruitmentEmailInvitationRepository, ExaminerRecruitmentEmailInvitationRepository>();

builder.Services.AddScoped<PdfContractService>();

//Exam Monitor
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

builder.Services.AddScoped<IExamMonitorService, ExamMonitorService>();
builder.Services.AddScoped<IExamMonitorRepository, ExamMonitorRepository>();
builder.Services.AddScoped<ICentreRepository, CentreRepository>();
builder.Services.AddScoped<IExamMonitorTransactionRepository, ExamMonitorTransactionRepository>();
builder.Services.AddAutoMapper(typeof(ExamMonitorProfile));
builder.Services.AddScoped<IExamMonitorApprovalRepository, ExamMonitorApprovalRepository>();


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

