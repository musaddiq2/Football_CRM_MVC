using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    public class CoachAttendance
    {
        public int Id { get; set; }
        public string PlayerName { get; set; }
        public string CoachName { get; set; }

        [Column("AttendanceDate")] // Match the SQL column name
        public DateTime? Date { get; set; }

        public bool IsPresent { get; set; }
        public string? Remarks { get; set; }
    }
}
