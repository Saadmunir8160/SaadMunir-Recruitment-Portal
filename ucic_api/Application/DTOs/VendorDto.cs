using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Application.DTOs
{
    public class VendorDto
    {
        public long? VendorId { get; set; } // Nullable in case it's used for create

        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public string? BPId { get; set; }
        public string? FaxNo { get; set; }
        public int CountryId { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string? ContactPersonEmail { get; set; }
        public string? ContactPersonPhoneNo { get; set; }
        public string? Category { get; set; }
        public int Employees { get; set; }
        public string CrNo { get; set; }
        public string ProductDetails { get; set; }
        public string AnnualTurnover { get; set; }
        public string MajorCustomers { get; set; }
        public string TaxRegistrationNo { get; set; }

        public bool? Price { get; set; }
        public bool? Quality { get; set; }
        public bool? Delivery { get; set; }
        public bool? Reference { get; set; }
        public bool? LocationalSuitability { get; set; }
        public bool? HSECompliance { get; set; }

        public bool IsApproved { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }

        public string? FilePath { get; set; } // Path or filename of uploaded document
        public string CRCertificateFilePath { get; set; }
        public string VatCertificateFilePath { get; set; }
        public string? ISO9001_2015FilePath { get; set; }
        public string? ISO14001_2015FilePath { get; set; }
        public string? ISO45001_2018FilePath { get; set; }
        public string? CompanyProfileFilePath { get; set; }
    }

}
