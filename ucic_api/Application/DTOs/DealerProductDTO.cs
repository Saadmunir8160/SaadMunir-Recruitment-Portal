namespace Application.DTOs
{
    public class DealerProductDTO
    {
        public int dealerProductID { get; set; } // Maps to DealerProductID
        public string productName { get; set; } = string.Empty; // Maps to ProductName
        public string? description { get; set; } // Maps to Description
        public string? shortDescription { get; set; } // Can be derived from Description
        public decimal price { get; set; } = 0; // Default price since not in entity
        public int stockQuantity { get; set; } = 100; // Default stock since not in entity
        // Image fields removed temporarily as requested
        public string? category { get; set; } = "General"; // Default category since not in entity
        public bool isActive { get; set; } = true; // Default active since not in entity
        public decimal rating { get; set; } = 0; // Default rating since not in entity
        public int reviewCount { get; set; } = 0; // Default review count since not in entity
        public int minimumOrderQuantity { get; set; } = 1; // Default minimum order since not in entity
        public DateTime? createdDate { get; set; } // Maps to CreatedDate from BaseEntity
        public DateTime? updatedDate { get; set; } // Maps to UpdatedDate from BaseEntity
        
        // Unit of measurement
        public string unit { get; set; } = "bags"; // Maps to Unit property from entity
        
        // Additional fields from original DTO
        public string? ERPItemCode { get; set; } // Maps to ProductCode
        public string? ItemDescription { get; set; } // Maps to Product_LnCode
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class CheckProductAvailabilityRequest
    {
        public int Quantity { get; set; }
    }
} 