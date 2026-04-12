using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Vendor : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long VendorId { get; set; }

        [Required, MaxLength(200)]
        public string CompanyName { get; set; }

        [Required, MaxLength(500)]
        public string Address { get; set; }

        [Required, MaxLength(50)]
        public string PhoneNo { get; set; }

        [MaxLength(50)]
        public string? BPId { get; set; }

        [MaxLength(50)]
        public string? FaxNo { get; set; }

        [ForeignKey("Country")]
        public int CountryId { get; set; }

        [ForeignKey("Currency")]
        public int CurrencyId { get; set; }

        [ForeignKey("City")]
        public long CityId { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Required, MaxLength(200)]
        public string Email { get; set; }

        public bool IsApproved { get; set; }

        [Required, MaxLength(200)]
        public string ContactPerson { get; set; }

        public string? ContactPersonEmail { get; set; }
        public string? ContactPersonPhoneNo { get; set; }

        [Required]
        public int Employees { get; set; }

        [Required]
        public string ProductDetails { get; set; }

        [Required]
        public string AnnualTurnover { get; set; }

        [Required]
        public string MajorCustomers { get; set; }

        [Required, MaxLength(150)]
        public string TaxRegistrationNo { get; set; }

        [Required, MaxLength(150)]
        public string CrNo { get; set; }
        [Required]
        public string CRCertificateFilePath { get; set; }
        [Required]
        public string VatCertificateFilePath { get; set; }
        public string? ISO9001_2015FilePath { get; set; }
        public string? ISO14001_2015FilePath { get; set; }
        public string? ISO45001_2018FilePath { get; set; }
        public string? CompanyProfileFilePath { get; set; }

        // Separate fields for each major registration criterion
        public bool? Price { get; set; }
        public bool? Quality { get; set; }
        public bool? Delivery { get; set; }
        public bool? Reference { get; set; }
        public bool? LocationalSuitability { get; set; }
        public bool? HSECompliance { get; set; }

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
        public virtual Country Country { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual CitiesByCountry CitiesByCountry { get; set; }
        public virtual Category Category { get; set; }
    }

    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
