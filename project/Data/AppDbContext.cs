using Microsoft.EntityFrameworkCore;
using project.Models.Entities;

namespace project.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<Shelter> Shelters => Set<Shelter>();
    public DbSet<Adoption> Adoptions => Set<Adoption>();
    public DbSet<AnimalVolunteer> AnimalVolunteers => Set<AnimalVolunteer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(u => u.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(u => u.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(u => u.Permissions).HasColumnName("permissions");
            entity.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(u => u.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Shelter>(entity =>
        {
            entity.ToTable("shelters");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(s => s.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(s => s.Address).HasColumnName("address").HasMaxLength(500).IsRequired();
            entity.Property(s => s.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(s => s.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(s => s.Capacity).HasColumnName("capacity").IsRequired();
            entity.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Animal>(entity =>
        {
            entity.ToTable("animals");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(a => a.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(a => a.Species).HasColumnName("species").HasMaxLength(50).IsRequired();
            entity.Property(a => a.Breed).HasColumnName("breed").HasMaxLength(100).IsRequired();
            entity.Property(a => a.Age).HasColumnName("age").IsRequired();
            entity.Property(a => a.Gender).HasColumnName("gender").HasMaxLength(10).IsRequired();
            entity.Property(a => a.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(a => a.Description).HasColumnName("description");
            entity.Property(a => a.ShelterId).HasColumnName("shelter_id").IsRequired();
            entity.Property(a => a.ArrivalDate).HasColumnName("arrival_date").IsRequired();
            entity.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(a => a.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasOne(a => a.Shelter)
                .WithMany(s => s.Animals)
                .HasForeignKey(a => a.ShelterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Adoption>(entity =>
        {
            entity.ToTable("adoptions");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(a => a.AnimalId).HasColumnName("animal_id").IsRequired();
            entity.Property(a => a.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(a => a.AdoptionDate).HasColumnName("adoption_date").IsRequired();
            entity.Property(a => a.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(a => a.Notes).HasColumnName("notes");
            entity.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(a => a.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasOne(a => a.Animal)
                .WithMany(an => an.Adoptions)
                .HasForeignKey(a => a.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(a => a.User)
                .WithMany(u => u.Adoptions)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AnimalVolunteer>(entity =>
        {
            entity.ToTable("animal_volunteers");
            entity.HasKey(av => av.Id);
            entity.Property(av => av.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(av => av.AnimalId).HasColumnName("animal_id").IsRequired();
            entity.Property(av => av.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(av => av.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(av => av.AssignedDate).HasColumnName("assigned_date").IsRequired();
            entity.Property(av => av.UnassignedDate).HasColumnName("unassigned_date");
            entity.Property(av => av.CreatedAt).HasColumnName("created_at").IsRequired();
            
            entity.HasOne(av => av.Animal)
                .WithMany(a => a.AnimalVolunteers)
                .HasForeignKey(av => av.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(av => av.User)
                .WithMany(u => u.AnimalVolunteers)
                .HasForeignKey(av => av.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(av => new { av.AnimalId, av.UserId, av.Role });
        });
    }
}
