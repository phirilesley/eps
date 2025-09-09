using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Examiner> EXM_EXAMINER_MASTER { get; set; }
        public DbSet<PaperMarkingRate> CAN_PAPER_MARKING_RATE { get; set; }

        public DbSet<Subjects> Subjects { get; set; }

        public DbSet<CategoryMarkingRate> CAN_CATEGORY_RATE { get; set; }

        public DbSet<ExamCodes> CAN_EXAM { get; set; }

        public DbSet<ExaminerScriptsMarked> EXAMINER_TRANSACTIONS { get; set; }


        public DbSet<TandS> TRAVELLING_AND_SUBSISTENCE_CLAIM { get; set; }

        public DbSet<TandSDetail> TRAVELLING_AND_SUBSISTENCE_DETAILS { get; set; }

        public DbSet<TandSAdvance> TRAVELLING_AND_SUBSISTENCE_ADVANCES { get; set; }

        public DbSet<TandSAdvanceFees> TRAVELLING_AND_SUBSISTENCE_FEES { get; set; }

        public DbSet<RateAndTax> RATE_AND_TAX_INFO { get; set; }
        
        public DbSet<CategoryRate> EXM_CATEGORY_MARKING_RATE { get; set; }


        public DbSet<MarksCaptured> EXM_SCRIPT_CAPTURED { get; set; }


        public DbSet<BankData> BANKING_DATA { get; set;}

        public DbSet<Venue> VENUES{ get; set; }

        public DbSet<ValidateTandS> VALIDATETANDS { get; set; }

        public DbSet<AuditTrail> AuditTrails { get; set; }

        public DbSet<RegisterDto> ExaminerRegister { get; set; }

        public DbSet<TandSFile> TRAVELLING_AND_SUBSISTENCE_FILES { get; set; }

        public DbSet<Apportionment> APPORTIONMENT { get; set; }

        public DbSet<LastNumberDatabase> MaxExaminerCode { get; set; }

        public DbSet<ApplicationUserDTO> ZImSecStaff { get; set; }

        public DbSet<DeletedTandS> DeletedTandS { get; set; }

        public DbSet<ReturnTandS> ReturnedTandS { get; set; }

        public DbSet<BrailleTranscriptionRate> Braille_Transcription_Rate { get; set; }

        public DbSet<SubjectVenue> SubjectVenue { get; set; }

        public DbSet<EntriesData> EntriesData { get; set; }


        public DbSet<EntriesData> EntriesTranscribersData { get; set; }

        public DbSet<DateRange> DateRange { get; set; }

        public DbSet<AdvanceComment> AdvanceComments { get; set; }

        public DbSet<Activity> Activities { get; set; }

        public DbSet<Apportioned> REF_CAT_PAPER {  get; set; }

        public DbSet<ExaminerApportionment> ExaminerApportionment {  get; set; }

        public DbSet<Material> MaterialMaster { get; set; }
        public DbSet<MaterialTransaction> MaterialTransaction { get; set; }
        public DbSet<CategoryCheckInCheckOut> CATEGORYCHECKINCHECKOUT { get; set; }

        public DbSet<SettingRate> SettingRates { get; set; }
        public DbSet<ActivityRate> ActivityRates { get; set; }




        //ExaminerRecruitment

        public DbSet<ExaminerRecruitment> ExaminerRecruitment { get; set; }

        //Teaching experience

        public DbSet<TeachingExperience> TeachingExperiences { get; set; }
        public DbSet<ProfessionalQualifications> ExaminerRecruitmentProfessionalQualifications { get; set; }

        public DbSet<ExaminerRecruitmentAttachements> ExaminerRecruitmentAttachements { get; set; }

        public DbSet<ExaminerRecruitmentInvitation> ExaminerRecruitmentInvitations { get; set; }
        public DbSet<ExaminerRecruitmentEmailInvitation> ExaminerRecruitmentEmailInvitations { get; set; }
        public DbSet<ExaminerRecruitmentRegister> ExaminerRecruitmentRegisters { get; set; }
        public DbSet<ExaminerRecruitmentTrainingSelection> ExaminerRecruitmentTrainingSelection { get; set; }

        public DbSet<ExaminerRecruitmentVenueDetails> ExaminerRecruitmentVenueDetails { get; set; }
        public DbSet<ExaminerRecruitmentAssessment> ExaminerRecruitmentAssessments { get; set; }

        public DbSet<ExamMonitor> ExamMonitors { get; set; }
        public DbSet<ExamMonitorTransaction> ExamMonitorTransactions { get; set; }

        public DbSet<Centre> Centres { get; set; }

        //public DbSet<Cluster> Clusters { get; set; }
        public DbSet<Level> Levels { get; set; }

        public DbSet<Phase> Phases { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<ExamSession> ExamSessions { get; set; }
   
        public DbSet<ExamMonitorTandS> ExamMonitorsClaimTandSs { get; set; }
        public DbSet<DailyAdvances> ExamMonitorsDailyAdvances { get; set; }
        public DbSet<StipendAdvance> ExamMonitorsStipendAdvances { get; set; }

        public DbSet<ExamMonitorRegister> ExamMonitorsRegisters { get; set; }

        public DbSet<ExamMonitorRegisterDate> ExamMonitorRegisterDates { get; set; }

        public DbSet<ExamMonitorProfessionalQualifications> ExamMonitorProfessionalQualifications { get; set; }
        public DbSet<ExamMonitorAttachements> ExamMonitorAttachements { get; set; }
        public DbSet<ExamMonitorEmailInvitation> ExamMonitorEmailInvitations { get; set; }
        public DbSet<ExamTimeTable> Exm_Timetable { get; set; }
        public DbSet<ExamMonitorsRecruitment> ExamMonitorsRecruitments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Configure ExaminerRecruitment relationships
            ConfigureExaminerRecruitmentRelationships(modelBuilder);

            // Configure additional entities here (add new methods for other group)
            ConfigureOtherEntityRelationships(modelBuilder);

            base.OnModelCreating(modelBuilder);
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }


        

            // Examiner to ApplicationUser (One-to-Many)
            modelBuilder.Entity<Examiner>()
                .HasMany(e => e.ApplicationUsers)
                .WithOne(u => u.Examiner)
                .HasForeignKey(u => u.IDNumber)
                  .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

      

            // Examiner to RegisterDto (One-to-One)
            modelBuilder.Entity<Examiner>()
                .HasOne(e => e.RegisterDto)
                .WithOne(r => r.Examiner)
                .HasForeignKey<RegisterDto>(r => r.IDNumber)
                .OnDelete(DeleteBehavior.Restrict);

            // Examiner to TandS (One-to-Many)
            modelBuilder.Entity<Examiner>()
                .HasMany(a => a.TandSs)
                .WithOne(t => t.Examiner)
                .HasForeignKey(t => t.EMS_NATIONAL_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // Examiner to ExaminerScriptsMarked (One-to-Many)
            modelBuilder.Entity<Examiner>()
                .HasMany(e => e.ExaminerScriptsMarkeds)
                .WithOne(s => s.Examiner)
                .HasForeignKey(s => s.EMS_NATIONAL_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure unique constraint on EMS_SUBKEY in ExaminerScriptsMarked
            modelBuilder.Entity<ExaminerScriptsMarked>()
                .HasIndex(s => s.EMS_SUBKEY)
                .IsUnique();


            modelBuilder.Entity<ApplicationUser>()
       .HasOne(c => c.Role)
       .WithMany()
       .HasForeignKey(c => c.RoleId)
       .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TandS>()
          .HasOne(t => t.Examiner)
          .WithMany(e => e.TandSs)
          .HasForeignKey(t => t.EMS_NATIONAL_ID)
          .OnDelete(DeleteBehavior.Restrict); // Prevents cascading delete if preferred

            // TandSDetail to TandS relationship (Foreign Key: TANDSCODE)
            modelBuilder.Entity<TandSDetail>()
                .HasOne(td => td.TandS)
                .WithMany(t => t.TandSDetails)
                .HasForeignKey(td => td.TANDSCODE)
                .OnDelete(DeleteBehavior.Restrict);

            // TandSAdvance to TandS relationship (Foreign Key: TANDSCODE)
            modelBuilder.Entity<TandSAdvance>()
                .HasOne(ta => ta.TandS)
                .WithOne(t => t.TandSAdvance)
                .HasForeignKey<TandSAdvance>(ta => ta.TANDSCODE)
                .OnDelete(DeleteBehavior.Restrict);

            // TandSFile to TandS relationship (Foreign Key: TANDSCODE)
            modelBuilder.Entity<TandSFile>()
                .HasOne(tf => tf.TandS)
                .WithMany(t => t.TandSFiles)
                .HasForeignKey(tf => tf.TANDSCODE)
                .OnDelete(DeleteBehavior.Restrict);


              modelBuilder.Entity<ExamMonitor>()
                .HasMany(e => e.ApplicationUsers)
                .WithOne(u => u.ExamMonitor)
                .HasForeignKey(u => u.IDNumber)
                  .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // ExamMonitorTandS primary key
            modelBuilder.Entity<ExamMonitorTandS>()
                .HasKey(e => new { e.SubKey, e.ClaimID });

            modelBuilder.Entity<DailyAdvances>()
       .HasKey(d => d.Id);

            modelBuilder.Entity<DailyAdvances>()
                .HasOne(d => d.Claim)
                .WithMany(c => c.DailyAdvances)
                .HasForeignKey(d => new { d.SubKey, d.ClaimID })
              .OnDelete(DeleteBehavior.Cascade);

            // StipendAdvance primary key and relationship
            modelBuilder.Entity<StipendAdvance>()
                .HasKey(s => new { s.SubKey, s.ClaimID });

            modelBuilder.Entity<StipendAdvance>()
                .HasOne(s => s.Claim)
                .WithOne()
                .HasForeignKey<StipendAdvance>(s => new { s.SubKey, s.ClaimID })
                .OnDelete(DeleteBehavior.Cascade);
              
            // ExamMonitorRegister primary key and relationships
         

            modelBuilder.Entity<ExamMonitorRegister>()
                .HasOne(r => r.ExamMonitor)
                .WithMany(e => e.ExamMonitorRegisters)
                .HasForeignKey(r => r.NationalId);

            modelBuilder.Entity<ExamMonitorRegister>()
                .HasOne(r => r.ExamMonitorTransaction)
                .WithMany(t => t.ExamMonitorRegisters)
                .HasForeignKey(r => r.SubKey)
                  .OnDelete(DeleteBehavior.Cascade);

            // ExamMonitorTransaction primary key and relationships
            modelBuilder.Entity<ExamMonitorTransaction>()
                .HasKey(t => t.SubKey);

            modelBuilder.Entity<ExamMonitorTransaction>()
                .HasOne(t => t.ExamMonitor)
                .WithMany(e => e.ExamMonitorTransactions)
                .HasForeignKey(t => t.NationalId)
                .HasPrincipalKey(e => e.NationalId)
                .OnDelete(DeleteBehavior.Cascade); // Prevents cascading delete if preferred

            // ExamMonitorTandS relationships
            modelBuilder.Entity<ExamMonitorTandS>()
                .HasOne(t => t.ExamMonitor)
                .WithMany(e => e.ExamMonitorTandSs)
                .HasForeignKey(t => t.NationalId)
                  .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExamMonitorTandS>()
                .HasOne(t => t.ExamMonitorTransaction)
                .WithOne()
                .HasForeignKey<ExamMonitorTandS>(t => t.SubKey)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExamMonitorTandS>()
                .HasOne(t => t.StipendAdvance)
                .WithOne(s => s.Claim)
                .HasForeignKey<StipendAdvance>(s => new { s.SubKey, s.ClaimID })
                  .OnDelete(DeleteBehavior.Cascade);

         

            modelBuilder.Entity<ExamMonitorTandS>()
                .Property(e => e.ClaimID)
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Centre>(entity =>
            {
                entity.HasKey(c => c.CentreNumber);

                entity.Property(c => c.CentreNumber)
                    .HasMaxLength(6)  // New length
                    .IsRequired();

                entity.Property(c => c.CentreName)
                    .IsRequired()
                    .HasMaxLength(100);
                // Match your DB schema

                //entity.HasOne(c => c.Cluster)
                //    .WithMany(cl => cl.Centres)
                //    .HasForeignKey(c => c.ClusterCode)
                //    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<ExamMonitorProfessionalQualifications>()
            .HasOne(e => e.ExamMonitor)
            .WithMany()
            .HasForeignKey(e => e.NationalId)
            .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<ExamMonitorAttachements>()
                    .HasOne(e => e.ExamMonitor)
                    .WithMany()
                    .HasForeignKey(e => e.NationalId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<ExamMonitorEmailInvitation>()
                    .HasOne(e => e.ExamMonitor)
                    .WithMany()
                    .HasForeignKey(e => e.NationalId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<ExamMonitorEmailInvitation>()
                    .HasOne(e => e.InvitedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.InvitedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<ExamMonitor>()
      .HasMany(e => e.ProfessionalQualifications)
      .WithOne(q => q.ExamMonitor)
      .HasForeignKey(q => q.NationalId)
      .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<ExamMonitor>()
                    .HasMany(e => e.Attachments)
                    .WithOne(a => a.ExamMonitor)
                    .HasForeignKey(a => a.NationalId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<ExamMonitor>()
                    .HasMany(e => e.EmailInvitations)
                    .WithOne(i => i.ExamMonitor)
                    .HasForeignKey(i => i.NationalId)
                    .OnDelete(DeleteBehavior.Cascade);
            });




            modelBuilder.Entity<Phase>(entity =>
            {
                entity.HasKey(p => p.PhaseCode);

                entity.Property(p => p.PhaseCode)
                    .HasMaxLength(20);

                entity.Property(p => p.PhaseName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.Rate)
                    .HasColumnType("decimal(18,2)");

                entity.HasOne(p => p.Level)
                    .WithMany()
                    .HasForeignKey(p => p.LevelCode)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        }


        public virtual async Task<int> SaveChangesAsync(string userid = null)
        {

            if (userid != null)
            {
                OnBeforeSaveChanges(userid);
            }
            try
            {
                var result = await base.SaveChangesAsync();
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        
        }

        private void OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();
            var aurditEntries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditTrail || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry);
                auditEntry.TableName = entry.Entity.GetType().Name;
                auditEntry.Module = entry.Entity.GetType().Name;
                auditEntry.UserId = userId;
                aurditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    string propertyname = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyname] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Create;
                            auditEntry.NewValues[propertyname] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Delete;
                            auditEntry.OldValues[propertyname] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {

                                auditEntry.ChangedColumns.Add(propertyname);
                                auditEntry.AuditType = AuditType.Update;
                                auditEntry.OldValues[propertyname] = property.OriginalValue;
                                auditEntry.NewValues[propertyname] = property.CurrentValue;
                            }
                            break;
                    }
                }

            }
            foreach (var auditEntry in aurditEntries)
            {
                AuditTrails.Add(auditEntry.ToAudit());
            }
        }



        // Method to configure ExaminerRecruitment relationships
        private void ConfigureExaminerRecruitmentRelationships(ModelBuilder modelBuilder)
        {
            // Configure one-to-one relationship with ExaminerRecruitmentInvitation
            modelBuilder.Entity<ExaminerRecruitment>()
                .HasOne(er => er.ExaminerRecruitmentInvitation)
                .WithOne(eri => eri.ExaminerRecruitment)
                .HasForeignKey<ExaminerRecruitmentInvitation>(eri => eri.ExaminerRecruitmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-one relationship with ExaminerRecruitmentRegister
            modelBuilder.Entity<ExaminerRecruitment>()
                .HasOne(er => er.ExaminerRecruitmentRegister)
                .WithOne(err => err.ExaminerRecruitment)
                .HasForeignKey<ExaminerRecruitmentRegister>(err => err.ExaminerRecruitmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure 1-1 bettwen ExaminerRecruitmentAssessments and ExaminerRecruitment
            modelBuilder.Entity<ExaminerRecruitmentAssessment>()
            .HasOne(era => era.ExaminerRecruitment) // Each ExaminerRecruitmentAssessment has one ExaminerRecruitment
            .WithOne(er => er.ExaminerRecruitmentAssessment) // One ExaminerRecruitment has one ExaminerRecruitmentAssessment
            .HasForeignKey<ExaminerRecruitmentAssessment>(era => era.ExaminerRecruitmentId) // The foreign key in ExaminerRecruitmentAssessment
            .OnDelete(DeleteBehavior.Cascade);


            // Configure 1-1 bettwen ExaminerRecruitmentTrainingSelection and ExaminerRecruitment
            modelBuilder.Entity<ExaminerRecruitmentTrainingSelection>()
           .HasOne(erts => erts.ExaminerRecruitment)
           .WithOne(er => er.ExaminerRecruitmentTrainingSelection)
           .HasForeignKey<ExaminerRecruitmentTrainingSelection>(erts => erts.ExaminerRecruitmentId);


            // Configure 1-1 bettwen ExaminerRecruitmentAttachements and ExaminerRecruitment

            modelBuilder.Entity<ExaminerRecruitmentAttachements>()
            .HasOne(era => era.ExaminerRecruitment)
            .WithOne(er => er.ExaminerRecruitmentAttachements)
            .HasForeignKey<ExaminerRecruitmentAttachements>(era => era.ExaminerRecruitmentId);


            modelBuilder.Entity<TeachingExperience>()
            .HasOne(te => te.ExaminerRecruitment) // TeachingExperience -> PersonalDetails (Many-to-One)
            .WithMany(pd => pd.TeachingExperiences) // PersonalDetails -> TeachingExperience (One-to-Many)
            .HasForeignKey(te => te.ExaminerRecruitmentId) // Foreign Key in TeachingExperience
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete to remove related teaching experiences



            modelBuilder.Entity<ProfessionalQualifications>()
            .HasOne(te => te.ExaminerRecruitment) // TeachingExperience -> PersonalDetails (Many-to-One)
            .WithMany(pd => pd.ProfessionalQualifications) // PersonalDetails -> TeachingExperience (One-to-Many)
            .HasForeignKey(te => te.ExaminerRecruitmentId) // Foreign Key in TeachingExperience
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete to remove related teaching experiences


            modelBuilder.Entity<ExaminerRecruitment>()
            .HasOne(e => e.ExaminerRecruitmentEmailInvitation)
            .WithOne(ei => ei.ExaminerRecruitment)
            .HasForeignKey<ExaminerRecruitmentEmailInvitation>(ei => ei.ExaminerRecruitmentId)
            .OnDelete(DeleteBehavior.Cascade); // Optional: Cascade delete when an ExaminerRecruitment is deleted

            // One-to-Many: One user can invite many examiners
            modelBuilder.Entity<ExaminerRecruitmentEmailInvitation>()
                .HasOne(e => e.InvitedByUser)
                .WithMany(u => u.Invitations)
                .HasForeignKey(e => e.InvitedBy)
                .OnDelete(DeleteBehavior.Restrict); // Prevents accidental cascade delete


            modelBuilder.Entity<ExaminerRecruitmentAssessment>()
            .HasOne(e => e.Capturer)
            .WithMany(u => u.CapturerAssessments)
            .HasForeignKey(e => e.CapturerId)
            .OnDelete(DeleteBehavior.NoAction); // 👈 When user deleted, CapturerId = null

            modelBuilder.Entity<ExaminerRecruitmentAssessment>()
                .HasOne(e => e.Verifier)
                .WithMany(u => u.VerifierAssessments)
                .HasForeignKey(e => e.VerifierId)
                .OnDelete(DeleteBehavior.NoAction); // 👈 When user deleted, VerifierId = null



            // New one-to-one relationship
            modelBuilder.Entity<ExaminerRecruitmentRegister>()
                .HasOne(r => r.ExaminerRecruitmentAssessment)
                .WithOne(a => a.ExaminerRecruitmentRegister)
                .HasForeignKey<ExaminerRecruitmentAssessment>(a => a.ExaminerRecruitmentRegisterId);
        }



        // Example for other entities
        private void ConfigureOtherEntityRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExaminerRecruitmentInvitation>()
                        .HasOne(eri => eri.ExaminerRecruitmentVenueDetails)
                        .WithMany(ervd => ervd.ExaminerRecruitmentInvitations)
                        .HasForeignKey(eri => eri.ExaminerRecruitmentVenueDetailsId);


            // Conversion for  TimeOnly types if needed
            modelBuilder.Entity<ExaminerRecruitmentVenueDetails>()
                .Property(e => e.TrainingTime)
                .HasConversion(
                    v => v.Value.ToTimeSpan(),
                    v => TimeOnly.FromTimeSpan(v)
                );
        }


    }
}
