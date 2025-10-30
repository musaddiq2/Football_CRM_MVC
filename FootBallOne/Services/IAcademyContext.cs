namespace FootBallOne.Services
{
    /// <summary>
    /// Interface for accessing current user's academy context
    /// </summary>
    public interface IAcademyContext
    {
        /// <summary>
        /// Gets the current user's AcademyID from session/claims
        /// </summary>
        int? GetCurrentAcademyId();

        /// <summary>
        /// Gets the current user's Academy Name
        /// </summary>
        string GetCurrentAcademyName();

        /// <summary>
        /// Checks if current user is a SuperAdmin (can see all branches)
        /// </summary>
        bool IsSuperAdmin();
    }
}