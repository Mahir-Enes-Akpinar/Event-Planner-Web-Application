using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public class Context : IdentityDbContext<Kullanici, IdentityRole<int>, int>
{
    public Context(DbContextOptions<Context> options) : base(options) { }

    public DbSet<Etkinlik> Etkinlikler => Set<Etkinlik>();
    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<UserInterest> KullaniciKategorileri => Set<UserInterest>();
    public DbSet<EventCategory> EtkinlikKategorileri => Set<EventCategory>();
    public DbSet<Participant> Katilimlar => Set<Participant>();
    public DbSet<Mesaj> Mesajlar => Set<Mesaj>();
    public DbSet<Score> Puanlar => Set<Score>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EmailOutboxMessage> EmailOutbox => Set<EmailOutboxMessage>();
    public DbSet<EventStaff> EventStaff => Set<EventStaff>();
    public DbSet<AttendanceQrToken> AttendanceQrTokens => Set<AttendanceQrToken>();
    public DbSet<StoredImage> StoredImages => Set<StoredImage>();
    public DbSet<Kullanici> Kullanicilar => Users;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Kullanici>(entity =>
        {
            entity.ToTable("Kullanicilar");
            entity.HasQueryFilter(user => !user.IsDeleted);
            entity.Property(user => user.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedNever();
        });

        modelBuilder.Entity<Etkinlik>(entity =>
        {
            entity.HasQueryFilter(item => !item.IsDeleted && !item.CreatedByUser.IsDeleted);
            entity.Property(item => item.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedNever();
            entity.HasOne(item => item.CreatedByUser).WithMany()
                .HasForeignKey(item => item.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Kategori>().HasIndex(category => category.Name).IsUnique();

        modelBuilder.Entity<StoredImage>(entity =>
        {
            entity.HasKey(item => item.FileName);
            entity.Property(item => item.FileName).HasMaxLength(50);
            entity.Property(item => item.ContentType).HasMaxLength(50);
            entity.Property(item => item.Content).IsRequired();
        });

        modelBuilder.Entity<UserInterest>(entity =>
        {
            entity.HasKey(item => new { item.KullaniciId, item.KategoriId });
            entity.HasOne(item => item.Kullanici).WithMany(user => user.UserInterests)
                .HasForeignKey(item => item.KullaniciId);
            entity.HasOne(item => item.Kategori).WithMany(category => category.UserInterests)
                .HasForeignKey(item => item.KategoriId);
        });

        modelBuilder.Entity<EventCategory>(entity =>
        {
            entity.HasQueryFilter(item => !item.Etkinlik.IsDeleted);
            entity.HasKey(item => new { item.EtkinlikId, item.KategoriId });
            entity.HasOne(item => item.Etkinlik).WithMany(eventItem => eventItem.EventCategories)
                .HasForeignKey(item => item.EtkinlikId);
            entity.HasOne(item => item.Kategori).WithMany(category => category.EventCategories)
                .HasForeignKey(item => item.KategoriId);
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasQueryFilter(item => !item.Etkinlik.IsDeleted && !item.Kullanici.IsDeleted);
            entity.HasIndex(item => new { item.KullaniciId, item.EtkinlikId }).IsUnique();
            entity.HasOne(item => item.Kullanici).WithMany()
                .HasForeignKey(item => item.KullaniciId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Etkinlik).WithMany(eventItem => eventItem.Participants)
                .HasForeignKey(item => item.EtkinlikId);
        });

        modelBuilder.Entity<EventStaff>(entity =>
        {
            entity.HasQueryFilter(item => !item.Etkinlik.IsDeleted && !item.User.IsDeleted);
            entity.HasIndex(item => new { item.EtkinlikId, item.UserId, item.Role }).IsUnique();
            entity.HasOne(item => item.Etkinlik).WithMany(eventItem => eventItem.StaffMembers)
                .HasForeignKey(item => item.EtkinlikId);
            entity.HasOne(item => item.User).WithMany(user => user.EventStaffAssignments)
                .HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AttendanceQrToken>(entity =>
        {
            entity.HasQueryFilter(item => !item.Etkinlik.IsDeleted && !item.Participant.Kullanici.IsDeleted);
            entity.HasIndex(item => item.TokenHash).IsUnique();
            entity.HasOne(item => item.Etkinlik).WithMany(eventItem => eventItem.AttendanceQrTokens)
                .HasForeignKey(item => item.EtkinlikId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Participant).WithMany(participant => participant.AttendanceQrTokens)
                .HasForeignKey(item => item.ParticipantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Mesaj>(entity =>
        {
            entity.HasQueryFilter(message => !message.Etkinlik.IsDeleted && !message.Sender.IsDeleted);
            entity.HasOne(message => message.Sender).WithMany(user => user.SentMessages)
                .HasForeignKey(message => message.SenderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(message => message.Etkinlik).WithMany(eventItem => eventItem.Messages)
                .HasForeignKey(message => message.EtkinlikId);
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasQueryFilter(item => !item.Kullanici.IsDeleted);
            entity.HasIndex(item => new { item.KullaniciId, item.Reason, item.SourceId }).IsUnique();
            entity.HasIndex(item => new { item.KullaniciId, item.Reason })
                .IsUnique()
                .HasFilter("\"Reason\" = 2 AND \"RevokedAtUtc\" IS NULL");
            entity.HasOne(item => item.Kullanici).WithMany(user => user.Scores)
                .HasForeignKey(item => item.KullaniciId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges()
    {
        UpdateConcurrencyTokens();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateConcurrencyTokens();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateConcurrencyTokens()
    {
        foreach (var entry in ChangeTracker.Entries<Etkinlik>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Property(item => item.RowVersion).CurrentValue = Guid.NewGuid().ToByteArray();
        }

        foreach (var entry in ChangeTracker.Entries<Kullanici>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Property(item => item.RowVersion).CurrentValue = Guid.NewGuid().ToByteArray();
        }
    }
}
