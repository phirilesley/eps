using System;
using System.Collections.Generic;
using System.Linq;

namespace ExaminerPaymentSystem.ViewModels.Layout
{
    public sealed class NavigationModuleCatalog : INavigationModuleCatalog
    {
        private static readonly IReadOnlyList<NavigationModuleDefinition> Modules = new List<NavigationModuleDefinition>
        {
            new("examiner-self-service", "~/Views/Shared/Layout/Navigation/_ExaminerSelfService.cshtml", roles =>
                roles.HasAny("Examiner", "DPMS", "BMS", "RPMS", "A")),
            new("pms", "~/Views/Shared/Layout/Navigation/_PmsMenu.cshtml", roles => roles.Has("PMS")),
            new("recruitment", "~/Views/Shared/Layout/Navigation/_RecruitmentMenu.cshtml", roles =>
                roles.HasAny("HRVerifier", "HRCapturer")),
            new("special-needs", "~/Views/Shared/Layout/Navigation/_SpecialNeedsMenu.cshtml", roles =>
                roles.HasAny("BT", "PBT", "I", "S")),
            new("officer-special-needs", "~/Views/Shared/Layout/Navigation/_OfficerSpecialNeedsMenu.cshtml", roles =>
                roles.Has("OfficerSpecialNeeds")),
            new("subject-manager", "~/Views/Shared/Layout/Navigation/_SubjectManagerMenu.cshtml", roles =>
                roles.HasAny("SubjectManager", "CentreSupervisor")),
            new("accounts", "~/Views/Shared/Layout/Navigation/_AccountsMenu.cshtml", roles =>
                roles.HasAny("Accounts", "AssistantAccountant", "PeerReviewer")),
            new("exams-admin", "~/Views/Shared/Layout/Navigation/_ExamsAdminMenu.cshtml", roles => roles.Has("ExamsAdmin")),
            new("system-admin", "~/Views/Shared/Layout/Navigation/_SystemAdministrationMenu.cshtml", roles =>
                roles.HasAny("Admin", "SuperAdmin")),
            new("regional-manager", "~/Views/Shared/Layout/Navigation/_RegionalManagerMenu.cshtml", roles =>
                roles.Has("RegionalManager")),
            new("hr", "~/Views/Shared/Layout/Navigation/_HrMenu.cshtml", roles => roles.Has("HR")),
            new("monitor-leadership", "~/Views/Shared/Layout/Navigation/_MonitorLeadershipMenu.cshtml", roles =>
                roles.HasAny("ClusterManager", "AssistantClusterManager", "ResidentMonitor"))
        };

        public IReadOnlyList<NavigationModuleDefinition> GetModules(LayoutRoleHelper roles)
        {
            if (roles is null)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            return Modules.Where(module => module.IsVisible(roles)).ToList();
        }
    }
}
