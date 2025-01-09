using Core.Enums;
using System.Collections.ObjectModel;

namespace Core.Auth.Permissions
{
    public static class Action
    {
        public const string View = nameof(View);
        public const string Search = nameof(Search);
        public const string Create = nameof(Create);
        public const string Update = nameof(Update);
        public const string Delete = nameof(Delete);
        public const string Export = nameof(Export);
        public const string Generate = nameof(Generate);
        public const string Clean = nameof(Clean);
        public const string UpgradeSubscription = nameof(UpgradeSubscription);
        public const string Upload = nameof(Upload);
    }

    public static class Resource
    {
        public const string Permission = nameof(Permission);
        public const string Dashboard = nameof(Dashboard);
        public const string Hangfire = nameof(Hangfire);
        public const string Users = nameof(Users);
        public const string UserRoles = nameof(UserRoles);
        public const string Roles = nameof(Roles);
        public const string RoleClaims = nameof(RoleClaims);
        public const string UserClaims = nameof(UserClaims);
        public const string Files = nameof(Files);
        public const string AuditLogs = nameof(AuditLogs);
    }

    public static class Permissions
    {
        private static readonly Permission[] _all = new Permission[]
        {
            new("View Dashboard", Action.View, Resource.Dashboard),
            new("View Hangfire", Action.View, Resource.Hangfire),

            //Permission
            new("View Permissions", Action.View, Resource.Permission, Roles.SuperAdmin),

            // USERS
            new("View Users", Action.View, Resource.Users, Roles.SuperAdmin),
            new("Search Users", Action.Search, Resource.Users, Roles.SuperAdmin),
            new("Create Users", Action.Create, Resource.Users, Roles.SuperAdmin),
            new("Update Users", Action.Update, Resource.Users, Roles.SuperAdmin),
            new("Delete Users", Action.Delete, Resource.Users, Roles.SuperAdmin),
            new("Export Users", Action.Export, Resource.Users, Roles.SuperAdmin),

            // ROLES
            new("View UserRoles", Action.View, Resource.UserRoles, Roles.SuperAdmin),
            new("Update UserRoles", Action.Update, Resource.UserRoles, Roles.SuperAdmin),
            new("View Roles", Action.View, Resource.Roles, Roles.SuperAdmin),
            new("Create Roles", Action.Create, Resource.Roles, Roles.SuperAdmin),
            new("Update Roles", Action.Update, Resource.Roles, Roles.SuperAdmin),
            new("Delete Roles", Action.Delete, Resource.Roles, Roles.SuperAdmin),
            new("Create RoleClaims", Action.Create, Resource.RoleClaims, Roles.SuperAdmin),
            new("Delete RoleClaims", Action.Delete, Resource.RoleClaims, Roles.SuperAdmin),
            new("Create UserClaims", Action.Create, Resource.UserClaims, Roles.SuperAdmin),
            new("Delete UserClaims", Action.Delete, Resource.UserClaims, Roles.SuperAdmin),
            new("View UserClaims", Action.View, Resource.UserClaims, Roles.SuperAdmin),
            new("Update UserClaims", Action.Update, Resource.UserClaims, Roles.SuperAdmin),


            // FILES
            new("Upload files", Action.Upload, Resource.Files),

            // AUDIT LOGS
            new("View AuditLogs", Action.View, Resource.AuditLogs, Roles.SuperAdmin),
        };

        public static IReadOnlyList<Permission> All { get; } = new ReadOnlyCollection<Permission>(_all);
        public static IReadOnlyList<Permission> SuperAdmin { get; } = new ReadOnlyCollection<Permission>(_all.Where(p => p.MinimumRole == Roles.SuperAdmin).ToArray());
        public static IReadOnlyList<Permission> Staff { get; } = new ReadOnlyCollection<Permission>(_all.Where(p => p.MinimumRole == Roles.Staff).ToArray());
        public static IReadOnlyList<Permission> Contributor { get; } = new ReadOnlyCollection<Permission>(_all.Where(p => p.MinimumRole == Roles.Contributor).ToArray());
        public static IReadOnlyList<Permission> Publisher { get; } = new ReadOnlyCollection<Permission>(_all.Where(p => p.MinimumRole == Roles.Publisher).ToArray());
        public static IReadOnlyList<Permission> Customer { get; } = new ReadOnlyCollection<Permission>(_all.Where(p => p.MinimumRole > Roles.Customer).ToArray());
        public static IReadOnlyList<Permission> Guest { get; } = new ReadOnlyCollection<Permission>(_all.Where(p => p.MinimumRole == Roles.Guest).ToArray());
    }

    public record Permission(string Description, string Action, string Resource, Roles MinimumRole = Roles.Guest)
    {
        public string Name => NameFor(Action, Resource);
        public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
    }
}