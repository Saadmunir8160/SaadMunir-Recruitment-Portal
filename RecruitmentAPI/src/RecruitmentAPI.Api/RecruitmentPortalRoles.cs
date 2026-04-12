using System.Security.Claims;

namespace RecruitmentAPI.Api.Controllers;

/// <summary>
/// UCIC Identity role names for the recruiter admin portal. Keep in sync with JWT role claims.
/// </summary>
public static class RecruitmentPortalRoles
{
    /// <summary>HR staff + managers who use RMS (vacancies, candidates, interviews, etc.).</summary>
    public const string Staff =
        "Admin,HR,hr,Recruiter,HRSupervisor,HR Manager,HR Section Head,HiringManager";

    public const string StaffWithCandidate = Staff + ",Candidate";

    /// <summary>Same names as <see cref="Staff"/>, split — used to normalize JWT role casing so HR/hr/Hr all authorize.</summary>
    public static readonly string[] StaffRoleNames =
        Staff.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static readonly string[] RoleClaimTypes =
    [
        ClaimTypes.Role,
        "role",
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ];

    /// <summary>
    /// UCIC tokens may use short <c>role</c> claims or mixed casing (<c>Hr</c> vs <c>HR</c>).
    /// Ensures <see cref="ClaimTypes.Role"/> claims exist with canonical strings from <see cref="StaffRoleNames"/>.
    /// </summary>
    public static void NormalizeStaffJwtRoleClaims(ClaimsIdentity identity)
    {
        if (identity == null) return;

        foreach (var claim in identity.Claims.Where(c => RoleClaimTypes.Contains(c.Type)).ToList())
        {
            var v = claim.Value?.Trim();
            if (string.IsNullOrEmpty(v)) continue;

            var canonical = StaffRoleNames.FirstOrDefault(r => string.Equals(r, v, StringComparison.Ordinal));
            if (canonical == null)
                canonical = StaffRoleNames.FirstOrDefault(r => string.Equals(r, v, StringComparison.OrdinalIgnoreCase));
            if (canonical == null) continue;

            if (!identity.HasClaim(ClaimTypes.Role, canonical))
                identity.AddClaim(new Claim(ClaimTypes.Role, canonical, claim.ValueType, claim.Issuer));
        }
    }
}
