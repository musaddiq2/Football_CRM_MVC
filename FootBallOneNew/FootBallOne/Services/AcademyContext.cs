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
            // Try to get AcademyID from session
            var academyId = _httpContextAccessor.HttpContext?.Session.GetInt32("AcademyID");

            // Return the value if it exists and is greater than 0
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
            // Check if user has SuperAdmin role stored in session
            var isSuperAdmin = _httpContextAccessor.HttpContext?.Session.GetString("IsSuperAdmin");

            return isSuperAdmin == "true";
        }
    }
}