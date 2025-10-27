namespace FootBallOne.Models
{
    public class AlertsViewModel
    {
        public List<RegistrationManagement> NotPaidCoaches { get; set; }
        public List<RegistrationManagement> ExpiredSubscriptions { get; set; }
        public List<RegistrationManagement> ExpiringSoonSubscriptions { get; set; }

    }
}
