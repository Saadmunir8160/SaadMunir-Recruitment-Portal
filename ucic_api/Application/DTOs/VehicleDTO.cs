namespace Application.DTOs
{
    public class VehicleDTO
    {
        public int VehicleId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? RegistrationNo { get; set; }
        public string? Model { get; set; }
        public string? UserId { get; set; }
        public string? ErpCode { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }
} 