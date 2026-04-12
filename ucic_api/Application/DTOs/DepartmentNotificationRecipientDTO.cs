
namespace Application.DTOs
{
    public class DepartmentNotificationRecipientDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
