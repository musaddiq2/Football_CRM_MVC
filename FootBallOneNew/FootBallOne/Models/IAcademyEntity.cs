namespace FootBallOne.Models
{
    /// <summary>
    /// Interface for entities that belong to a specific academy/branch
    /// </summary>
    public interface IAcademyEntity
    {
        // CHANGE: Make AcademyID non-nullable (int instead of int?)
        int? AcademyID { get; set; }
    }
}