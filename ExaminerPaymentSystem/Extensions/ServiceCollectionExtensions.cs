using DinkToPdf;
using DinkToPdf.Contracts;
using ExaminerPaymentSystem.Interfaces.Common;
using ExaminerPaymentSystem.Interfaces.ExamMonitors;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Interfaces.Transcribers;
using ExaminerPaymentSystem.Repositories.Common;
using ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys;
using ExaminerPaymentSystem.Repositories.Examiners;
using ExaminerPaymentSystem.Repositories.ExamMonitors;
using ExaminerPaymentSystem.Services;
using ExaminerPaymentSystem.Services.ExamMonitors;
using ExaminerPaymentSystem.ViewModels.Layout;
using Microsoft.Extensions.DependencyInjection;

namespace ExaminerPaymentSystem.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExaminerPaymentsModule(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IExaminerRepository, ExaminerRepository>();
            services.AddScoped<ITandSRepository, TandSRepository>();
            services.AddScoped<ITandSDetailsRepository, TandSDetailRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ICategoryRateRepository, CategoryRateRepository>();
            services.AddScoped<IPaperMarkingRateRepository, PaperMarkingRateRepository>();
            services.AddScoped<ISubjectsRepository, SubjectsRepository>();
            services.AddScoped<IExamCodesRepository, ExamCodesRepository>();
            services.AddScoped<IAdvanceFeesRepository, AdvanceFeesRepository>();
            services.AddScoped<IMarksCapturedRepository, MarksCapturedRepository>();
            services.AddScoped<IRateAndTaxRepository, RateAndTaxRepository>();
            services.AddScoped<IBanksRepository, BanksRepository>();
            services.AddScoped<ITandSFilesRepository, TandSFilesRepository>();
            services.AddScoped<IVenueRepository, VenueRepository>();
            services.AddScoped<IAuditTrailRepository, AuditTrailRepository>();
            services.AddScoped<IValidateTandS, ValidateTandSRepository>();
            services.AddScoped<IRegisterRepository, RegisterRepository>();
            services.AddScoped<IApportionmentRepository, ApportionmentRepository>();
            services.AddScoped<IMaxExaminerCodeRepository, MaxExaminerCodeRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IManageTeamsRepository, ManageTeamsRepository>();
            services.AddTransient<IPdfService, PdfService>();
            services.AddTransient<IExportService, ExportService>();
            services.AddTransient<IMaterialRepository, MaterialRepository>();
            services.AddTransient<IMaterialTransactionRepository, MaterialTransactionRepository>();
            services.AddTransient<ICategoryCheckInCheckOutRepository, CategoryCheckInCheckOutRepository>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            return services;
        }

        public static IServiceCollection AddSpecialNeedsModule(this IServiceCollection services)
        {
            services.AddScoped<IBrailleTranscriptionRateRepository, BrailleTranscriptionRateRepository>();
            services.AddScoped<ITranscribersRepository, TranscribersRepository>();

            return services;
        }

        public static IServiceCollection AddExaminerRecruitmentModule(this IServiceCollection services)
        {
            services.AddScoped<IExaminerRecruitmentRepository, ExaminerRecruitmentRepository>();
            services.AddScoped<ITeachingExperienceRepository, TeachingExperienceRepository>();
            services.AddScoped<IExaminerRecruitmentAttachmentsRepository, ExaminerRecruitmentAttachmentsRepository>();
            services.AddScoped<IExaminerRecruitmentTrainingSelectionRepository, ExaminerRecruitmentTrainingSelectionRepository>();
            services.AddScoped<IExaminerRecruitmentRegisterRepository, ExaminerRecruitmentRegisterRepository>();
            services.AddScoped<IExaminerRecruitmentAssessmentRepository, ExaminerRecruitmentAssessmentRepository>();
            services.AddScoped<IExaminerRecruitmentVenueDetailsRepository, ExaminerRecruitmentVenueDetailsRepository>();
            services.AddScoped<IExaminerRecruitmentProfessionalQualifications, ExaminerRecruitmentProfessionalQualifications>();
            services.AddScoped<IExaminerRecruitmentEmailInvitationRepository, ExaminerRecruitmentEmailInvitationRepository>();
            services.AddScoped<PdfContractService>();

            return services;
        }

        public static IServiceCollection AddExamMonitorModule(this IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IExamMonitorService, ExamMonitorService>();
            services.AddScoped<IExamMonitorRepository, ExamMonitorRepository>();
            services.AddScoped<ICentreRepository, CentreRepository>();
            services.AddScoped<IExamMonitorTransactionRepository, ExamMonitorTransactionRepository>();
            services.AddAutoMapper(typeof(ExamMonitorProfile));
            services.AddScoped<IExamMonitorApprovalRepository, ExamMonitorApprovalRepository>();

            return services;
        }

        public static IServiceCollection AddNavigationModuleCatalog(this IServiceCollection services)
        {
            services.AddSingleton<INavigationModuleCatalog, NavigationModuleCatalog>();
            return services;
        }

    }
}
