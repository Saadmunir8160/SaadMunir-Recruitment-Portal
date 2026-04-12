namespace Application.DTOs
{
    public class DealerAreaDTO
    {
        public int AreaID { get; set; }
        public required string AreaName { get; set; }
        public required string AreaCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateDealerAreaDTO
    {
        public required string AreaName { get; set; }
        public required string AreaCode { get; set; }
    }

    public class UpdateDealerAreaDTO
    {
        public int AreaID { get; set; }
        public required string AreaName { get; set; }
        public required string AreaCode { get; set; }
        public bool IsActive { get; set; }
    }
}
