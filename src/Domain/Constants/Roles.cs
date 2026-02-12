namespace DHAFacilitationAPIs.Domain.Constants;

public abstract class Roles
{
    public const string SuperAdministrator = nameof(SuperAdministrator);
    public const string Administrator = nameof(Administrator);
    public const string Admin = nameof(Admin);
    public static IEnumerable<string> GetRoles()
    {
        return new[] { SuperAdministrator, Administrator, Admin};
    }
}
