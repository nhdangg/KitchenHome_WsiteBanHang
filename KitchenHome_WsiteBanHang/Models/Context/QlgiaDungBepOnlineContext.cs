using System;
using System.Collections.Generic;
using KitchenHome_WsiteBanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models.Context;

public partial class QlgiaDungBepOnlineContext : DbContext
{
    public QlgiaDungBepOnlineContext()
    {
    }

    public QlgiaDungBepOnlineContext(DbContextOptions<QlgiaDungBepOnlineContext> options)
        : base(options)
    {
    }

    public virtual DbSet<KhuyenMaiBienThe> KhuyenMaiBienThes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-CISDHNE\\SQLEXPRESS;Initial Catalog=QLGiaDungBepOnline;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KhuyenMaiBienThe>(entity =>
        {
            entity.HasKey(e => new { e.KhuyenMaiId, e.BienTheId }).HasName("PK__KhuyenMa__B95A7C15BB14DA5D");

            entity.ToTable("KhuyenMai_BienThe");

            entity.Property(e => e.KhuyenMaiId).HasColumnName("KhuyenMaiID");
            entity.Property(e => e.BienTheId).HasColumnName("BienTheID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
