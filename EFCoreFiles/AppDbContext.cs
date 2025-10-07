using LiteBanking.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace LiteBanking.EFCoreFiles;

public class AppDbContext : DbContext
{
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Balance> Balances { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder b)
    {
        #region User
        b.Entity<User>(u =>
        {
            u.HasKey(x => x.Id);                      
            u.HasIndex(x => x.HashKeyRecoveryWord).IsUnique();
            u.Property(x => x.Name).HasMaxLength(100);
            u.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        });
        #endregion

        #region Balance
        b.Entity<Balance>(bal =>
        {
            bal.HasKey(x => x.Id);
            bal.HasIndex(x => x.OwnerId);
            bal.Property(x => x.Amount).HasPrecision(18, 2);
            bal.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            b.Entity<Balance>()
                .HasOne<User>()
                .WithMany(u => u.Balances)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Cascade); 
        });
        #endregion
        
    }
}