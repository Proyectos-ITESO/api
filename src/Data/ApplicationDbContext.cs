using Microsoft.EntityFrameworkCore;
using MicroJack.API.Models;
using MicroJack.API.Models.Core;
using MicroJack.API.Models.Catalog;
using MicroJack.API.Models.Transaction;

namespace MicroJack.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Legacy tables removed - using new normalized entities
    
    // Pre-registration system
    public DbSet<PreRegistration> PreRegistrations { get; set; }
    
    // Bitácora system
    public DbSet<BitacoraNote> BitacoraNotes { get; set; }

    // Core entities
    public DbSet<Guard> Guards { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<GuardRole> GuardRoles { get; set; }
    public DbSet<Booth> Booths { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Resident> Residents { get; set; }
    public DbSet<Visitor> Visitors { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }

    // Catalog entities
    public DbSet<VehicleBrand> VehicleBrands { get; set; }
    public DbSet<VehicleColor> VehicleColors { get; set; }
    public DbSet<VehicleType> VehicleTypes { get; set; }
    public DbSet<VisitReason> VisitReasons { get; set; }

    // Transaction entities
    public DbSet<AccessLog> AccessLogs { get; set; }
    public DbSet<EventLog> EventLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Legacy entities configuration removed

        // Pre-registration configuration
        ConfigurePreRegistration(modelBuilder);
        
        // New normalized schema configuration
        ConfigureNewSchema(modelBuilder);
    }

    private void ConfigurePreRegistration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PreRegistration>(entity =>
        {
            entity.HasIndex(e => e.Plates);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ExpectedArrivalTime);
            entity.HasIndex(e => new { e.Plates, e.Status }).IsUnique();
        });
    }

    private void ConfigureNewSchema(ModelBuilder modelBuilder)
    {
        // Configure Vehicle unique constraint
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(e => e.LicensePlate).IsUnique();
        });

        // Configure Guard unique constraint
        modelBuilder.Entity<Guard>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Configure Address unique extension and representative relationship
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasIndex(e => e.Extension).IsUnique();
            entity.HasOne(a => a.RepresentativeResident)
                  .WithMany()
                  .HasForeignKey(a => a.RepresentativeResidentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Role unique constraint
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure GuardRole relationships
        modelBuilder.Entity<GuardRole>(entity =>
        {
            entity.HasIndex(e => new { e.GuardId, e.RoleId }).IsUnique();
        });

        modelBuilder.Entity<GuardRole>()
            .HasOne(gr => gr.Guard)
            .WithMany(g => g.GuardRoles)
            .HasForeignKey(gr => gr.GuardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuardRole>()
            .HasOne(gr => gr.Role)
            .WithMany(r => r.GuardRoles)
            .HasForeignKey(gr => gr.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationships
        
        // Resident -> Address
        modelBuilder.Entity<Resident>()
            .HasOne(r => r.Address)
            .WithMany(a => a.Residents)
            .HasForeignKey(r => r.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        // Vehicle -> Brand/Color/Type
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Brand)
            .WithMany(b => b.Vehicles)
            .HasForeignKey(v => v.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Color)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.ColorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Type)
            .WithMany(t => t.Vehicles)
            .HasForeignKey(v => v.TypeId)
            .OnDelete(DeleteBehavior.SetNull);

        // AccessLog relationships
        modelBuilder.Entity<AccessLog>()
            .HasOne(al => al.Visitor)
            .WithMany(v => v.AccessLogs)
            .HasForeignKey(al => al.VisitorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccessLog>()
            .HasOne(al => al.Vehicle)
            .WithMany(v => v.AccessLogs)
            .HasForeignKey(al => al.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AccessLog>()
            .HasOne(al => al.Address)
            .WithMany(a => a.AccessLogs)
            .HasForeignKey(al => al.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccessLog>()
            .HasOne(al => al.ResidentVisited)
            .WithMany(r => r.AccessLogs)
            .HasForeignKey(al => al.ResidentVisitedId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AccessLog>()
            .HasOne(al => al.EntryGuard)
            .WithMany(g => g.EntryLogs)
            .HasForeignKey(al => al.EntryGuardId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccessLog>()
            .HasOne(al => al.ExitGuard)
            .WithMany(g => g.ExitLogs)
            .HasForeignKey(al => al.ExitGuardId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AccessLog>()
            .HasOne(al => al.VisitReason)
            .WithMany(vr => vr.AccessLogs)
            .HasForeignKey(al => al.VisitReasonId)
            .OnDelete(DeleteBehavior.SetNull);

        // EventLog -> Guard
        modelBuilder.Entity<EventLog>()
            .HasOne(el => el.Guard)
            .WithMany(g => g.EventLogs)
            .HasForeignKey(el => el.GuardId)
            .OnDelete(DeleteBehavior.Restrict);

        // BitacoraNote -> Guard
        modelBuilder.Entity<BitacoraNote>()
            .HasOne(b => b.Guard)
            .WithMany()
            .HasForeignKey(b => b.GuardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}