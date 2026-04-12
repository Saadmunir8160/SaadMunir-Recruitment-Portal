using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Infra.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? CreditLimit { get; set; }
        
        [MaxLength(50)]
        public string? LnId { get; set; }

        [ForeignKey("UserType")]
        public int? UserTypeId { get; set; }
        public virtual UserType UserType { get; set; }

        // Navigation properties for entities that reference this user
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; }
        public virtual ICollection<Driver> Drivers { get; set; }
        public virtual ICollection<Transporter> Transporters { get; set; }
        public virtual ICollection<Dealer> Dealers { get; set; }
        public virtual ICollection<DealerDriver> DealerDrivers { get; set; }
    }
}
