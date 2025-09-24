using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ExaminerPaymentSystem.ViewModels.Layout
{
    public sealed class LayoutRoleHelper
    {
        private readonly HashSet<string> _roles;

        private LayoutRoleHelper(IEnumerable<string> roles)
        {
            _roles = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
        }

        public static LayoutRoleHelper FromUser(ClaimsPrincipal? user)
        {
            if (user is null)
            {
                return new LayoutRoleHelper(Array.Empty<string>());
            }

            var roles = user.Claims
                .Where(claim => claim.Type == ClaimTypes.Role)
                .Select(claim => claim.Value);

            return new LayoutRoleHelper(roles);
        }

        public bool Has(string role) => _roles.Contains(role);

        public bool HasAny(params string[] roles)
        {
            foreach (var role in roles)
            {
                if (_roles.Contains(role))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAll(params string[] roles)
        {
            foreach (var role in roles)
            {
                if (!_roles.Contains(role))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
