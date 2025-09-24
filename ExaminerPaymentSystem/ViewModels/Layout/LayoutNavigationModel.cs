using System;
using System.Collections.Generic;
using System.Linq;
using ExaminerPaymentSystem.Models.Examiner;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Models.Subjects;

namespace ExaminerPaymentSystem.ViewModels.Layout
{
    public class LayoutNavigationModel
    {
        public LayoutNavigationModel(
            LayoutRoleHelper roles,
            string systemName,
            IEnumerable<ExamCodes> examCodes,
            IEnumerable<Activity> activities,
            IEnumerable<Venue> venues,
            IEnumerable<ExaminerRecruitmentVenueDetails> examinerRecruitmentVenues,
            string subjectsJson,
            IEnumerable<NavigationModuleDefinition>? navigationModules = null)
        {
            Roles = roles ?? LayoutRoleHelper.FromUser(null);
            SystemName = systemName ?? string.Empty;
            ExamCodes = (examCodes ?? Array.Empty<ExamCodes>()).ToList();
            Activities = (activities ?? Array.Empty<Activity>()).ToList();
            Venues = (venues ?? Array.Empty<Venue>()).ToList();
            ExaminerRecruitmentVenues = (examinerRecruitmentVenues ?? Array.Empty<ExaminerRecruitmentVenueDetails>()).ToList();
            SubjectsJson = subjectsJson ?? "[]";
            NavigationModules = (navigationModules ?? Array.Empty<NavigationModuleDefinition>()).ToList();
        }

        public LayoutRoleHelper Roles { get; }

        public string SystemName { get; }

        public IReadOnlyList<ExamCodes> ExamCodes { get; }

        public IReadOnlyList<Activity> Activities { get; }

        public IReadOnlyList<Venue> Venues { get; }

        public IReadOnlyList<ExaminerRecruitmentVenueDetails> ExaminerRecruitmentVenues { get; }

        public string SubjectsJson { get; }

        public IReadOnlyList<NavigationModuleDefinition> NavigationModules { get; }

        public bool IsSystem(string systemName) =>
            string.Equals(SystemName, systemName, StringComparison.OrdinalIgnoreCase);
    }
}
