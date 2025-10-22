using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class PlayerRequest
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string City { get; set; }

        public DateTime RequestedDate { get; set; } = DateTime.Now;
        public string Status { get; set; }
    }
}
