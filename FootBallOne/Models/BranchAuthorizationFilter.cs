using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FootBallOne.Filters
{
    /// <summary>
    /// Authorization filter to ensure branch-level access control
    /// </summary>
    public class BranchAuthorizationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            // Check if user is authenticated
            var adminEmail = session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                context.Result = new RedirectToActionResult("Login", "Academy", null);
                return;
            }

            // Get user's academy and selected academy
            var userAcademyId = session.GetInt32("AcademyID");
            var selectedAcademyId = session.GetInt32("SelectedAcademyID");
            var isSuperAdmin = session.GetString("IsSuperAdmin") == "True";

            // If not super admin and trying to access different branch
            if (!isSuperAdmin && userAcademyId != selectedAcademyId)
            {
                // Reset to user's own branch
                session.SetInt32("SelectedAcademyID", userAcademyId.Value);

                context.Result = new RedirectToActionResult(
                    "Dashboard",
                    "Academy",
                    new { error = "Access denied to other branches" }
                );
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }
    }

    /// <summary>
    /// Attribute to mark controllers/actions that require branch authorization
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireBranchAuthAttribute : Attribute
    {
    }
}