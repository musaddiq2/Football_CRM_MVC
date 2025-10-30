using System;
namespace FootBallOne.DTO
{
    public class FBOrderDTO
    {
        public int OrderId { get; set; }
        public int Id { get; set; }
        public int? AcademyId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemsCount { get; set; }
    }
}
