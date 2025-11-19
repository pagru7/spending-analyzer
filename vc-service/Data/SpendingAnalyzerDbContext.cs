using Microsoft.EntityFrameworkCore;
using SpendingAnalyzer.Entities;

namespace SpendingAnalyzer.Data;

public class SpendingAnalyzerDbContext : DbContext
{
    public SpendingAnalyzerDbContext(DbContextOptions<SpendingAnalyzerDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Bank configuration
        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsInactive).HasDefaultValue(false);
            
            entity.HasMany(e => e.BankAccounts)
                .WithOne(e => e.Bank)
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BankAccount configuration
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreationDate).IsRequired();
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.IsInactive).HasDefaultValue(false);
            
            entity.HasOne(e => e.Bank)
                .WithMany(e => e.BankAccounts)
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Transaction configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Recipient).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Account)
                .WithMany(e => e.Transactions)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Transfer configuration
        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.SourceAccount)
                .WithMany(e => e.SourceTransfers)
                .HasForeignKey(e => e.SourceAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.TargetAccount)
                .WithMany(e => e.TargetTransfers)
                .HasForeignKey(e => e.TargetAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
