using Microsoft.AspNetCore.Http;

namespace FootBallOne.Services
{
    /// <summary>
    /// Implementation of IAcademyContext - reads academy info from HTTP session
    /// </summary>
    public class AcademyContext : IAcademyContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AcademyContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetCurrentAcademyId()
        {
            var userRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");

            // For Managers, always return their fixed academy (cannot be changed)
            if (userRole == "Manager")
            {
                var managerAcademyId = _httpContextAccessor.HttpContext?.Session.GetInt32("ManagerAcademyID");
                if (managerAcademyId.HasValue && managerAcademyId.Value > 0)
                {
                    return managerAcademyId.Value;
                }
            }

            // For Super Admin or others, return current session academy
            var academyId = _httpContextAccessor.HttpContext?.Session.GetInt32("AcademyID");
            if (academyId.HasValue && academyId.Value > 0)
            {
                return academyId.Value;
            }

            return null;
        }

        public string GetCurrentAcademyName()
        {
            // Try to get AcademyName from session
            var academyName = _httpContextAccessor.HttpContext?.Session.GetString("AcademyName");

            return academyName ?? "Unknown Branch";
        }

        public bool IsSuperAdmin()
        {
            var userRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");
            return userRole == "SuperAdmin";
        }
    }
}