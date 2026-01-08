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
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ImportedTransaction> ImportedTransactions => Set<ImportedTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Recipient).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TransactionDate).IsRequired().HasColumnType("DATE");
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Bank configuration
        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsInactive).HasDefaultValue(false);

            entity.HasMany(e => e.Accounts)
                .WithOne(e => e.Bank)
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BankAccount configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsInactive).HasDefaultValue(false);

            entity.HasOne(e => e.Bank)
                .WithMany(e => e.Accounts)
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Transactions)
                .WithOne(e => e.Account)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Transaction configuration
        modelBuilder.Entity<ImportedTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Recipient).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Type).HasColumnName("TransactionType");

            entity.HasOne(e => e.Transaction)
            .WithOne(e => e.ImportedTransaction)
            .HasForeignKey<Transaction>(e => e.ImportedTransactionId)
            .OnDelete(DeleteBehavior.SetNull);
        });
    }
}