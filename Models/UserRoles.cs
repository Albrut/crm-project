namespace MyAspNetApp.Models
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Employee = "Employee";

        public static readonly string[] AllRoles = new[]
        {
            Admin,
            Manager,
            Employee
        };
    }
}
