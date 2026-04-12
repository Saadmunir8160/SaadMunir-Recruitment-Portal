using System.Security.Claims;

namespace RecruitmentAPI.Application.Common;

/// <summary>
/// Role checks for the two-step vacancy approval chain (HR Manager → HR Section Head).
/// JWT role claim values must align with <see cref="Microsoft.AspNetCore.Authorization.AuthorizeAttribute"/> on the API.
/// </summary>
public static class VacancyApprovalAuthorization
{
    public static HashSet<string> GetRoleSet(ClaimsPrincipal? user)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (user?.Identity?.IsAuthenticated != true)
            return set;

        foreach (var claim in user.FindAll(ClaimTypes.Role))
        {
            if (!string.IsNullOrWhiteSpace(claim.Value))
                set.Add(claim.Value.Trim());
        }

        // Short JWT "role" claim (some hosts do not map to ClaimTypes.Role before handlers run)
        foreach (var claim in user.FindAll("role"))
        {
            if (!string.IsNullOrWhiteSpace(claim.Value))
                set.Add(claim.Value.Trim());
        }

        return set;
    }

    /// <summary>
    /// Submit for approval: creators (Recruiter / HR) plus <c>Admin</c> override.
    /// Approvers (HR Manager, HR Section Head) and legacy <c>HRSupervisor</c> do not submit here.
    /// </summary>
    public static bool CanSubmitForApproval(HashSet<string> roles) =>
        roles.Contains("Admin")
        || roles.Contains("Recruiter")
        || roles.Contains("HR")
        || roles.Contains("hr");

    /// <summary>
    /// Step 1 → HR Manager; step 2 → HR Section Head (legacy <c>HRSupervisor</c> may act only at step 2).
    /// <c>Admin</c> may approve or reject at any pending step.
    /// </summary>
    public static bool CanActOnApprovalStep(HashSet<string> roles, byte approvalStep)
    {
        if (roles.Contains("Admin"))
            return true;

        return approvalStep switch
        {
            1 => roles.Contains("HR Manager"),
            2 => roles.Contains("HR Section Head")
                || roles.Contains("hr section head")
                || roles.Contains("HRSupervisor"),
            _ => false
        };
    }
}
