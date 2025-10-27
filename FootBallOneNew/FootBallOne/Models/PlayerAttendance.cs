using System;
using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class PlayerAttendance
    {
        [Key]
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string CoachName { get; set; }
        public string Status { get; set; }  // "Present" or "Absent"
        public DateTime Date { get; set; }
    }
}
