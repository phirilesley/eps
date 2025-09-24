using System;

namespace ExaminerPaymentSystem.ViewModels.Layout
{
    public sealed class NavigationModuleDefinition
    {
        private readonly Func<LayoutRoleHelper, bool> _isVisible;

        public NavigationModuleDefinition(string key, string partialViewPath, Func<LayoutRoleHelper, bool>? isVisible = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Module key cannot be empty", nameof(key));
            }

            if (string.IsNullOrWhiteSpace(partialViewPath))
            {
                throw new ArgumentException("Partial view path cannot be empty", nameof(partialViewPath));
            }

            Key = key;
            PartialViewPath = partialViewPath;
            _isVisible = isVisible ?? (_ => true);
        }

        public string Key { get; }

        public string PartialViewPath { get; }

        public bool IsVisible(LayoutRoleHelper roles) => _isVisible(roles);
    }
}
