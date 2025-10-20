using System;
using System.Collections.Generic;
using LAF.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace LAF.DataAccess.Data;

public partial class LAFDbContext : DbContext
{
    public LAFDbContext()
    {
    }

    public LAFDbContext(DbContextOptions<LAFDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CashAccount> CashAccounts { get; set; }

    public virtual DbSet<Cashflow> Cashflows { get; set; }

    public virtual DbSet<CollateralType> CollateralTypes { get; set; }

    public virtual DbSet<Counterparty> Counterparties { get; set; }

    public virtual DbSet<Fund> Funds { get; set; }

    public virtual DbSet<RepoRate> RepoRates { get; set; }

    public virtual DbSet<RepoTrade> RepoTrades { get; set; }

    public virtual DbSet<Security> Securities { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VAvailableCash> VAvailableCashes { get; set; }

    public virtual DbSet<VCashAccountBalance> VCashAccountBalances { get; set; }

    public virtual DbSet<VFundBalance> VFundBalances { get; set; }

    public virtual DbSet<VwActiveCounterparty> VwActiveCounterparties { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=LAF;Trusted_Connection=true;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CashAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CashAcco__3214EC0724E06016");

            entity.ToTable("CashAccount");

            entity.HasIndex(e => e.FundId, "IX_CashAccount_FundId");

            entity.Property(e => e.AccountName).HasMaxLength(100);
            entity.Property(e => e.Balance)
                .HasDefaultValue(0.0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.OwnerType).HasMaxLength(50);

            entity.HasOne(d => d.Fund).WithMany(p => p.CashAccounts)
                .HasForeignKey(d => d.FundId)
                .HasConstraintName("FK_CashAccount_Fund");
        });

        modelBuilder.Entity<Cashflow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cashflow__3214EC07D095ED54");

            entity.ToTable("Cashflow", tb => tb.HasTrigger("trg_Cashflow_SetFundId"));

            entity.HasIndex(e => e.FundId, "IX_Cashflow_FundId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CashflowType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Description).HasMaxLength(255);

            entity.HasOne(d => d.CashAccount).WithMany(p => p.Cashflows)
                .HasForeignKey(d => d.CashAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cashflow__CashAc__6754599E");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CashflowCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Cashflows_CreatedBy");

            entity.HasOne(d => d.Fund).WithMany(p => p.Cashflows)
                .HasForeignKey(d => d.FundId)
                .HasConstraintName("FK_Cashflow_Fund");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.CashflowModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .HasConstraintName("FK_Cashflows_ModifiedBy");

            entity.HasOne(d => d.Trade).WithMany(p => p.Cashflows)
                .HasForeignKey(d => d.TradeId)
                .HasConstraintName("FK__Cashflow__TradeI__68487DD7");
        });

        modelBuilder.Entity<CollateralType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Collater__7F6321AB1EF33949");

            entity.Property(e => e.AssetType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CollateralType1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CollateralType");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetimeoffset())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CollateralTypeCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_CollateralTypes_CreatedBy");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.CollateralTypeModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .HasConstraintName("FK_CollateralTypes_ModifiedBy");
        });

        modelBuilder.Entity<Counterparty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Counterp__3214EC0797E54D95");

            entity.ToTable("Counterparty");

            entity.HasIndex(e => e.IsActive, "IX_Counterparty_Active");

            entity.HasIndex(e => e.CounterpartyCode, "IX_Counterparty_Code");

            entity.HasIndex(e => e.CounterpartyName, "IX_Counterparty_Name");

            entity.HasIndex(e => e.CounterpartyType, "IX_Counterparty_Type");

            entity.HasIndex(e => e.CounterpartyCode, "UQ__Counterp__45789F7F67FC07B3").IsUnique();

            entity.Property(e => e.CounterpartyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CounterpartyName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CounterpartyType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Dealer");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasDefaultValue("US")
                .IsFixedLength();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.CreditRating)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LegalEntityIdentifier)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Region)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Fund>(entity =>
        {
            entity.ToTable("Fund");

            entity.HasIndex(e => e.FundCode, "UQ_Fund_FundCode").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FundCode).HasMaxLength(50);
            entity.Property(e => e.FundName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<RepoRate>(entity =>
        {
            entity.ToTable("RepoRate");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.FinalCircle).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RepoRate1)
                .HasColumnType("decimal(8, 4)")
                .HasColumnName("RepoRate");
            entity.Property(e => e.TargetCircle).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Tenor)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.CollateralType).WithMany(p => p.RepoRates)
                .HasForeignKey(d => d.CollateralTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RepoRate__Collat__47DBAE45");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.RepoRates)
                .HasForeignKey(d => d.CounterpartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RepoRate_Counterparty");
        });

        modelBuilder.Entity<RepoTrade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RepoTrad__3028BABB9AA30143");

            entity.HasIndex(e => e.FundId, "IX_RepoTrades_FundId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Direction)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Notional).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Rate).HasColumnType("decimal(8, 4)");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TradeDate).HasDefaultValueSql("(sysdatetimeoffset())");

            entity.HasOne(d => d.CollateralType).WithMany(p => p.RepoTrades)
                .HasForeignKey(d => d.CollateralTypeId)
                .HasConstraintName("FK_RepoTrades_CollateralType");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.RepoTrades)
                .HasForeignKey(d => d.CounterpartyId)
                .HasConstraintName("FK_RepoTrades_Counterparty");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RepoTradeCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_RepoTrades_CreatedBy");

            entity.HasOne(d => d.Fund).WithMany(p => p.RepoTrades)
                .HasForeignKey(d => d.FundId)
                .HasConstraintName("FK_RepoTrades_Fund");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.RepoTradeModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .HasConstraintName("FK_RepoTrades_ModifiedBy");

            entity.HasOne(d => d.Security).WithMany(p => p.RepoTrades)
                .HasForeignKey(d => d.SecurityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RepoTrades_SecID");
        });

        modelBuilder.Entity<Security>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Security__3214EC073DB09232");

            entity.ToTable("Security");

            entity.HasIndex(e => e.Isin, "UQ_Security_ISIN").IsUnique();

            entity.Property(e => e.AssetType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Coupon).HasColumnType("decimal(9, 4)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Isin)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ISIN");
            entity.Property(e => e.Issuer).HasMaxLength(100);
            entity.Property(e => e.IssuerType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SecurityCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Securities_CreatedBy");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.SecurityModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .HasConstraintName("FK_Securities_ModifiedBy");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07E1EE789D");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D1053420EECB72").IsUnique();

            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(256);
        });

        modelBuilder.Entity<VAvailableCash>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vAvailableCash");

            entity.Property(e => e.AccountName).HasMaxLength(100);
            entity.Property(e => e.AvailableBalance).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FundCode).HasMaxLength(50);
            entity.Property(e => e.FundName).HasMaxLength(200);
        });

        modelBuilder.Entity<VCashAccountBalance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vCashAccountBalance");

            entity.Property(e => e.AccountName).HasMaxLength(100);
            entity.Property(e => e.Balance).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FundCode).HasMaxLength(50);
            entity.Property(e => e.FundName).HasMaxLength(200);
        });

        modelBuilder.Entity<VFundBalance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vFundBalances");

            entity.Property(e => e.AvailableBalance).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FundCode).HasMaxLength(50);
            entity.Property(e => e.FundName).HasMaxLength(200);
        });

        modelBuilder.Entity<VwActiveCounterparty>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ActiveCounterparties");

            entity.Property(e => e.CounterpartyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CounterpartyName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CounterpartyType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.CreditRating)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.LegalEntityIdentifier)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Region)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
