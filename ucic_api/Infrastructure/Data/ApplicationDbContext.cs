using Domain.Entities;
using Infra.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infra.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<CoverageArea> CoverageAreas { get; set; }
        public DbSet<Cities> Cities { get; set; }
        public DbSet<GeneralSettings> GeneralSettings { get; set; }
        public DbSet<ContactUsRequest> contactUsRequest { get; set; }
        public DbSet<GpsLocation> GpsLocation { get; set; }
        public DbSet<Jobs> Jobs { get; set; }
        public DbSet<JobApplications> JobApplications { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<CitiesByCountry> CitiesByCountry { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<OrderType> OrderType { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<Driver> Driver { get; set; }
        public DbSet<UserType> UserType { get; set; }
        public DbSet<DepartmentNotificationRecipient> DepartmentNotificationRecipients { get; set; }
        public DbSet<Transporter> Transporters { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<News> News { get; set; }

        // Dealer-related tables
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<DealerProduct> DealerProducts { get; set; }
        public DbSet<DealerOrder> DealerOrders { get; set; }
        public DbSet<DealerOrderItem> DealerOrderItems { get; set; }
        public DbSet<DealerDriver> DealerDrivers { get; set; }
        public DbSet<DealerVehicle> DealerVehicles { get; set; }
        public DbSet<DealerShippingAddress> DealerShippingAddresses { get; set; }
        public DbSet<DealerArea> DealerAreas { get; set; }
        public DbSet<DealerDailyLimit> DealerDailyLimits { get; set; }
        public DbSet<DealerDriverLog> DealerDriverLogs { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }

        // Note: Some dealer portal entities removed - using existing alternatives
        // DealerAddress: Use DealerShippingAddress instead
        // DealerNotification: Removed from system  
        // FAQ, FAQFeedback, CallbackRequest, DealerFeedback: Removed from system
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportTicketAttachment> SupportTicketAttachments { get; set; }
        public DbSet<SupportTicketMessage> SupportTicketMessages { get; set; }
        public DbSet<SupportTicketMessageAttachment> SupportTicketMessageAttachments { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // DealerArea configuration (general areas shared across dealers)
            modelBuilder.Entity<DealerArea>(eb =>
            {
                eb.HasKey(a => a.AreaID);
                eb.Property(a => a.AreaName).HasMaxLength(100).IsRequired();
                eb.Property(a => a.AreaCode).HasMaxLength(50).IsRequired();
            });

            // Identity table configurations
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("AspNetUsers", "auth");
            });

            modelBuilder.Entity<IdentityRole>(b =>
            {
                b.ToTable("AspNetRoles", "auth");
            });

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable("AspNetUserRoles", "auth");
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("AspNetUserClaims", "auth");
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("AspNetUserLogins", "auth");
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("AspNetRoleClaims", "auth");
            });

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("AspNetUserTokens", "auth");
            });

            // Existing configurations
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.TrackingId)
                .IsUnique();

            // Vendor
            modelBuilder.Entity<Vendor>()
                .HasKey(v => v.VendorId);
            modelBuilder.Entity<Vendor>()
                .HasIndex(o => o.TaxRegistrationNo)
                .IsUnique();

            modelBuilder.Entity<ContactUsRequest>()
                .HasKey(v => v.ContactUsRequestId);

            // Customer
            modelBuilder.Entity<Customer>()
                .HasKey(c => c.CustomerId);
            modelBuilder.Entity<Customer>()
                .Property(c => c.UserId)
                .IsRequired();

            // Job <-> JobApplications (One-to-Many)
            modelBuilder.Entity<JobApplications>()
                .HasOne(l => l.Jobs)
                .WithMany(c => c.JobApplications)
                .HasForeignKey(l => l.JobsId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Jobs>()
                .Property(j => j.WorkType)
                .HasConversion<string>();
            
            // Dealer entity key
            modelBuilder.Entity<Dealer>()
                .HasKey(d => d.DealerId);

            // Unique Ln/Code indexes for dealer-related tables
            modelBuilder.Entity<Dealer>()
                .HasIndex(d => d.Ln_ID)
                .IsUnique()
                .HasFilter("[Ln_ID] IS NOT NULL");

            modelBuilder.Entity<DealerDriver>()
                .HasIndex(d => d.Ln_ID)
                .IsUnique()
                .HasFilter("[Ln_ID] IS NOT NULL");

            modelBuilder.Entity<DealerVehicle>()
                .HasIndex(v => v.Ln_ID)
                .IsUnique()
                .HasFilter("[Ln_ID] IS NOT NULL");

            modelBuilder.Entity<DealerShippingAddress>()
                .HasIndex(a => a.Ln_ID)
                .IsUnique()
                .HasFilter("[Ln_ID] IS NOT NULL");

            modelBuilder.Entity<DealerProduct>()
                .HasIndex(p => p.Product_LnCode)
                .IsUnique()
                .HasFilter("[Product_LnCode] IS NOT NULL");

            modelBuilder.Entity<DealerOrder>()
                .HasIndex(o => o.Ln_OrderNumber)
                .IsUnique()
                .HasFilter("[Ln_OrderNumber] IS NOT NULL");
            // country <-> city (One-to-Many)
            modelBuilder.Entity<CitiesByCountry>()
                .HasOne(l => l.Country)
                .WithMany(c => c.CitiesByCountry)
                .HasForeignKey(l => l.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // country <-> city (One-to-Many)
            modelBuilder.Entity<DepartmentNotificationRecipient>()
                .HasOne(l => l.Department)
                .WithMany(c => c.DepartmentNotificationRecipients)
                .HasForeignKey(l => l.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Jobs>()
                .Property(j => j.WorkLocation)
                .HasConversion<string>();

            // Customer <-> Locations (One-to-Many)
            modelBuilder.Entity<Location>()
                .HasOne(l => l.Customer)
                .WithMany(c => c.Locations)
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // CoverageArea <-> Locations (One-to-Many)
            modelBuilder.Entity<Location>()
                .HasOne(l => l.CoverageArea)
                .WithMany(c => c.Locations)
                .HasForeignKey(l => l.CoverageAreaId)
                .OnDelete(DeleteBehavior.Restrict);

            // CoverageArea <-> Promotions (One-to-Many)
            modelBuilder.Entity<Promotion>()
                .HasOne(l => l.CoverageArea)
                .WithMany(c => c.Promotions)
                .HasForeignKey(l => l.CoverageAreaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vendor>()
                .HasOne(l => l.Country)
                .WithMany(c => c.Vendors)
                .HasForeignKey(l => l.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vendor>()
                .HasOne(l => l.CitiesByCountry)
                .WithMany(c => c.Vendors)
                .HasForeignKey(l => l.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vendor>()
                .HasOne(l => l.Currency)
                .WithMany(c => c.Vendors)
                .HasForeignKey(l => l.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vendor>()
                .HasOne(l => l.Category)
                .WithMany(c => c.Vendors)
                .HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // CoverageArea <-> Cities (One-to-Many)
            modelBuilder.Entity<Cities>()
                .HasOne(l => l.CoverageArea)
                .WithMany(c => c.Cities)
                .HasForeignKey(l => l.CoverageAreaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cities <-> Locations (One-to-Many)
            modelBuilder.Entity<Location>()
                .HasOne(l => l.Cities)
                .WithMany(c => c.Locations)
                .HasForeignKey(l => l.CitiesId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer <-> Orders (One-to-Many)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Location <-> Order (One-to-Many)
            modelBuilder.Entity<Order>()
               .HasOne(o => o.Location)
               .WithMany(c => c.Orders)
               .HasForeignKey(o => o.LocationId)
               .OnDelete(DeleteBehavior.Restrict);

            // Order <-> OrderItems (One-to-Many)
            modelBuilder.Entity<OrderItems>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            // OrderItem <-> Product (Many-to-One)
            modelBuilder.Entity<OrderItems>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order <-> GpsLocation (One-to-One)
            modelBuilder.Entity<GpsLocation>()
                .HasOne(p => p.Order)
                .WithOne(o => o.GpsLocation)
                .HasForeignKey<GpsLocation>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ApplicationUser <-> UserType (Optional One-to-Many)
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.UserType)
                .WithMany()
                .HasForeignKey(u => u.UserTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order <-> Promotion (Optional One-to-Many)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Promotion)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.PromotionId)
                .OnDelete(DeleteBehavior.Restrict);
       
            // Order <-> Driver (Optional One-to-Many)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Driver)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order <-> Vehicle (Optional One-to-Many)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Vehicle)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order <-> OrderType (Optional One-to-Many)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.OrderType)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.OrderTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vehicle <-> ApplicationUser (Optional One-to-Many)
            modelBuilder.Entity<Vehicle>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order <-> ApplicationUser (Optional One-to-Many)
            modelBuilder.Entity<Order>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExceptionLogs
            modelBuilder.Entity<CoverageArea>()
                .HasKey(c => c.CoverageAreaId);

            modelBuilder.Entity<Vendor>()
                .Property(o => o.ApprovalStatus)
                .HasConversion<string>();

            modelBuilder.Entity<News>()
                .Property(o => o.Type)
                .HasConversion<string>();

            // ApplicationUser <-> Driver (Optional One-to-Many)
            modelBuilder.Entity<Driver>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.Drivers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ApplicationUser <-> Transporter (Optional One-to-Many)
            modelBuilder.Entity<Transporter>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.Transporters)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Dealer relationships - Fixed cascade conflicts
            modelBuilder.Entity<Dealer>()
                .HasMany(d => d.DealerProducts)
                .WithOne(p => p.Dealer)
                .HasForeignKey(p => p.DealerID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dealer>()
                .HasMany(d => d.DealerOrders)
                .WithOne(o => o.Dealer)
                .HasForeignKey(o => o.DealerID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dealer>()
                .HasMany(d => d.DealerDrivers)
                .WithOne(dr => dr.Dealer)
                .HasForeignKey(dr => dr.DealerID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dealer>()
                .HasMany(d => d.DealerVehicles)
                .WithOne(v => v.Dealer)
                .HasForeignKey(v => v.DealerID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dealer>()
                .HasMany(d => d.DealerShippingAddresses)
                .WithOne(a => a.Dealer)
                .HasForeignKey(a => a.DealerID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dealer>()
                .HasMany(d => d.DealerDailyLimits)
                .WithOne(l => l.Dealer)
                .HasForeignKey(l => l.DealerID)
                .IsRequired(false) // Allow nullable DealerID for general limits
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DealerProduct>()
                .HasMany(p => p.DealerOrderItems)
                .WithOne(i => i.DealerProduct)
                .HasForeignKey(i => i.DealerProductID)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cycles

            modelBuilder.Entity<DealerOrder>()
                .HasMany(o => o.DealerOrderItems)
                .WithOne(i => i.DealerOrder)
                .HasForeignKey(i => i.DealerOrderID)
                .OnDelete(DeleteBehavior.Cascade);

            // Fixed the cascade conflict by making this Restrict instead of Cascade
            modelBuilder.Entity<DealerOrder>()
                .HasMany(o => o.DealerDriverLogs)
                .WithOne(l => l.DealerOrder)
                .HasForeignKey(l => l.DealerOrderID)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cycles

            modelBuilder.Entity<DealerDriver>()
                .HasMany(d => d.DealerOrders)
                .WithOne(o => o.DealerDriver)
                .HasForeignKey(o => o.DriverID)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cycles

            // Fixed the cascade conflict by making this Restrict instead of Cascade
            modelBuilder.Entity<DealerDriver>()
                .HasMany(d => d.DealerDriverLogs)
                .WithOne(l => l.DealerDriver)
                .HasForeignKey(l => l.DriverID)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cycles

            modelBuilder.Entity<DealerVehicle>()
                .HasMany(v => v.DealerOrders)
                .WithOne(o => o.DealerVehicle)
                .HasForeignKey(o => o.VehicleID)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cycles

            modelBuilder.Entity<DealerShippingAddress>()
                .HasMany(a => a.DealerOrders)
                .WithOne(o => o.DealerShippingAddress)
                .HasForeignKey(o => o.AddressID)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cycles

            modelBuilder.Entity<DealerArea>()
                .HasMany(a => a.DealerOrders)
                .WithOne(o => o.DealerArea)
                .HasForeignKey(o => o.AreaID)
                .OnDelete(DeleteBehavior.Restrict); // Areas -> Orders (Restrict to avoid cycles)

            // FIXED: ApplicationUser <-> DealerDriver relationship
            // Changed from "ApplicationUserId" to use the correct "UserId" property
            modelBuilder.Entity<DealerDriver>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.DealerDrivers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Note: Dealer portal entity configurations removed
            // DealerAddress, DealerNotification, FAQ, FAQFeedback, CallbackRequest, DealerFeedback entities removed
            
            // SupportTicket configurations remain for existing support functionality
            modelBuilder.Entity<SupportTicket>()
                .HasKey(st => st.TicketId);

            modelBuilder.Entity<SupportTicket>()
                .HasOne(st => st.Dealer)
                .WithMany()  // Removed d.SupportTickets navigation property reference
                .HasForeignKey(st => st.DealerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupportTicket>()
                .HasOne(st => st.Order)
                .WithMany()
                .HasForeignKey(st => st.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTicket>()
                .HasOne(st => st.Product)
                .WithMany()
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTicketAttachment>()
                .HasKey(sta => sta.AttachmentId);

            modelBuilder.Entity<SupportTicketAttachment>()
                .HasOne(sta => sta.SupportTicket)
                .WithMany(st => st.Attachments)
                .HasForeignKey(sta => sta.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupportTicketMessage>()
                .HasKey(stm => stm.MessageId);

            modelBuilder.Entity<SupportTicketMessage>()
                .HasOne(stm => stm.SupportTicket)
                .WithMany(st => st.Messages)
                .HasForeignKey(stm => stm.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupportTicketMessageAttachment>()
                .HasKey(stma => stma.AttachmentId);

            modelBuilder.Entity<SupportTicketMessageAttachment>()
                .HasOne(stma => stma.SupportTicketMessage)
                .WithMany(stm => stm.Attachments)
                .HasForeignKey(stma => stma.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
