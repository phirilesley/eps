using System.Collections.Generic;

namespace ExaminerPaymentSystem.ViewModels.Layout
{
    public interface INavigationModuleCatalog
    {
        IReadOnlyList<NavigationModuleDefinition> GetModules(LayoutRoleHelper roles);
    }
}
