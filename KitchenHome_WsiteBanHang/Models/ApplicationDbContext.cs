using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Banner> Banners { get; set; }

    public virtual DbSet<BienTheSanPham> BienTheSanPhams { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<ChiTietTraHang> ChiTietTraHangs { get; set; }

    public virtual DbSet<DanhGiaSanPham> DanhGiaSanPhams { get; set; }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<DiaChiKhachHang> DiaChiKhachHangs { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<DonHangDiaChiGiao> DonHangDiaChiGiaos { get; set; }

    public virtual DbSet<DonViTinh> DonViTinhs { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<HinhAnhSanPham> HinhAnhSanPhams { get; set; }

    public virtual DbSet<HoanTien> HoanTiens { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<Kho> Khos { get; set; }


    public virtual DbSet<LichSuTrangThaiDonHang> LichSuTrangThaiDonHangs { get; set; }

    public virtual DbSet<MaGiamGia> MaGiamGia { get; set; }

    public virtual DbSet<NhatKyCongThanhToan> NhatKyCongThanhToans { get; set; }

    public virtual DbSet<NhatKyHeThong> NhatKyHeThongs { get; set; }

    public virtual DbSet<NhatKyKho> NhatKyKhos { get; set; }

    public virtual DbSet<PhuongThucThanhToan> PhuongThucThanhToans { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<SanPhamYeuThich> SanPhamYeuThiches { get; set; }

    public virtual DbSet<SuDungMaGiamGium> SuDungMaGiamGia { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    public virtual DbSet<ThuongHieu> ThuongHieus { get; set; }

    public virtual DbSet<TokenBaoMat> TokenBaoMats { get; set; }

    public virtual DbSet<TonKho> TonKhos { get; set; }

    public virtual DbSet<TraHang> TraHangs { get; set; }

    public virtual DbSet<TrangNoiDung> TrangNoiDungs { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    public virtual DbSet<VwDoanhThuTheoNgay> VwDoanhThuTheoNgays { get; set; }

    public virtual DbSet<VwTopBanChay> VwTopBanChays { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning  To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-CISDHNE\\SQLEXPRESS;Initial Catalog=QLGiaDungBepOnline;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.BannerId).HasName("PK__Banner__32E86A31896FB17D");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<BienTheSanPham>(entity =>
        {
            entity.HasKey(e => e.BienTheId).HasName("PK__BienTheS__B5708622D06E7580");

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SoLuongToiThieu).HasDefaultValue(1);

            entity.HasOne(d => d.SanPham).WithMany(p => p.BienTheSanPhams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BT_SP");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.ChiTietDonHangId).HasName("PK__ChiTietD__45B33F83CBDB8F37");

            entity.Property(e => e.ThanhTien).HasComputedColumnSql("([SoLuong]*[DonGia]-[GiamGiaDong])", true);

            entity.HasOne(d => d.BienThe).WithMany(p => p.ChiTietDonHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTDH_BT");

            entity.HasOne(d => d.DonHang).WithMany(p => p.ChiTietDonHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTDH_DH");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => e.ChiTietGioHangId).HasName("PK__ChiTietG__EC01138D13BBCFC6");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.BienThe).WithMany(p => p.ChiTietGioHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTGH_BT");

            entity.HasOne(d => d.GioHang).WithMany(p => p.ChiTietGioHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTGH_GH");
        });

        modelBuilder.Entity<ChiTietTraHang>(entity =>
        {
            entity.HasKey(e => e.ChiTietTraHangId).HasName("PK__ChiTietT__B46A691FF5C06899");

            entity.Property(e => e.ThanhTienHoan).HasComputedColumnSql("([SoLuong]*[DonGiaHoan])", true);

            entity.HasOne(d => d.BienThe).WithMany(p => p.ChiTietTraHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTTH_BT");

            entity.HasOne(d => d.TraHang).WithMany(p => p.ChiTietTraHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTTH_TH");
        });

        modelBuilder.Entity<DanhGiaSanPham>(entity =>
        {
            entity.HasKey(e => e.DanhGiaId).HasName("PK__DanhGiaS__52C0CA257426FDBA");

            entity.Property(e => e.HienThi).HasDefaultValue(true);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.DonHang).WithMany(p => p.DanhGiaSanPhams).HasConstraintName("FK_DG_DH");

            entity.HasOne(d => d.KhachHang).WithMany(p => p.DanhGiaSanPhams).HasConstraintName("FK_DG_KH");

            entity.HasOne(d => d.SanPham).WithMany(p => p.DanhGiaSanPhams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DG_SP");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.DanhMucId).HasName("PK__DanhMuc__1C53BA7B6159E26F");

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.DanhMucCha).WithMany(p => p.InverseDanhMucCha).HasConstraintName("FK_DanhMuc_Cha");
        });

        modelBuilder.Entity<DiaChiKhachHang>(entity =>
        {
            entity.HasKey(e => e.DiaChiId).HasName("PK__DiaChiKh__94E668E6893D9931");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.KhachHang).WithMany(p => p.DiaChiKhachHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DCKH_KhachHang");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.DonHangId).HasName("PK__DonHang__D159F4DE9B7661B1");

            entity.Property(e => e.KenhBan).HasDefaultValue("WEB");
            entity.Property(e => e.NgayDat).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai).HasDefaultValue("CHO_XAC_NHAN");

            entity.HasOne(d => d.KhachHang).WithMany(p => p.DonHangs).HasConstraintName("FK_DH_KH");

            entity.HasOne(d => d.Kho).WithMany(p => p.DonHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DH_Kho");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.DonHangs).HasConstraintName("FK_DH_TK");
        });

        modelBuilder.Entity<DonHangDiaChiGiao>(entity =>
        {
            entity.HasKey(e => e.DonHangId).HasName("PK__DonHang___D159F4DE7FC996C8");

            entity.Property(e => e.DonHangId).ValueGeneratedNever();

            entity.HasOne(d => d.DonHang).WithOne(p => p.DonHangDiaChiGiao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DHDC_DH");
        });

        modelBuilder.Entity<DonViTinh>(entity =>
        {
            entity.HasKey(e => e.DonViTinhId).HasName("PK__DonViTin__2B93D42E0A8BFF4A");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.GioHangId).HasName("PK__GioHang__4242280DEF9295F2");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai).HasDefaultValue("DANG_MUA");

            entity.HasOne(d => d.KhachHang).WithMany(p => p.GioHangs).HasConstraintName("FK_GH_KH");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.GioHangs).HasConstraintName("FK_GH_TK");
        });

        modelBuilder.Entity<HinhAnhSanPham>(entity =>
        {
            entity.HasKey(e => e.HinhAnhId).HasName("PK__HinhAnhS__8EF32B7B766A8A95");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.BienThe).WithMany(p => p.HinhAnhSanPhams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HA_BT");
        });

        modelBuilder.Entity<HoanTien>(entity =>
        {
            entity.HasKey(e => e.HoanTienId).HasName("PK__HoanTien__99692BD3958FB1CB");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai).HasDefaultValue("CHO_HOAN");

            entity.HasOne(d => d.ThanhToan).WithMany(p => p.HoanTiens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HT_TT");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.KhachHangId).HasName("PK__KhachHan__880F211B78B26AF6");

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.TaiKhoan).WithOne(p => p.KhachHang).HasConstraintName("FK_KH_TaiKhoan");
        });

        modelBuilder.Entity<Kho>(entity =>
        {
            entity.HasKey(e => e.KhoId).HasName("PK__Kho__C29CB7FF626D0127");

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
        });

       

        modelBuilder.Entity<LichSuTrangThaiDonHang>(entity =>
        {
            entity.HasKey(e => e.LichSuId).HasName("PK__LichSuTr__CD0C1E3B5ABDFB49");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.DonHang).WithMany(p => p.LichSuTrangThaiDonHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LS_DH");
        });

        modelBuilder.Entity<MaGiamGia>(entity =>
        {
            entity.HasKey(e => e.MaGiamGiaId).HasName("PK__MaGiamGi__C28471D3D1F7E474");

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
        });

        modelBuilder.Entity<NhatKyCongThanhToan>(entity =>
        {
            entity.HasKey(e => e.NhatKyId).HasName("PK__NhatKyCo__DF38EB8F4D67DD75");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.ThanhToan).WithMany(p => p.NhatKyCongThanhToans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NKCTT_TT");
        });

        modelBuilder.Entity<NhatKyHeThong>(entity =>
        {
            entity.HasKey(e => e.NhatKyId).HasName("PK__NhatKyHe__DF38EB8F9B7528A6");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.NhatKyHeThongs).HasConstraintName("FK_NKHT_TK");
        });

        modelBuilder.Entity<NhatKyKho>(entity =>
        {
            entity.HasKey(e => e.NhatKyKhoId).HasName("PK__NhatKyKh__609D06FFE76850CB");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.BienThe).WithMany(p => p.NhatKyKhos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NKK_BT");

            entity.HasOne(d => d.Kho).WithMany(p => p.NhatKyKhos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NKK_Kho");
        });

        modelBuilder.Entity<PhuongThucThanhToan>(entity =>
        {
            entity.HasKey(e => e.PhuongThucId).HasName("PK__PhuongTh__1E306C036F77D8E5");

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.SanPhamId).HasName("PK__SanPham__05180FF402D9F70F");

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.DanhMuc).WithMany(p => p.SanPhams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SP_DanhMuc");

            entity.HasOne(d => d.DonViTinh).WithMany(p => p.SanPhams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SP_DonViTinh");

            entity.HasOne(d => d.ThuongHieu).WithMany(p => p.SanPhams).HasConstraintName("FK_SP_ThuongHieu");
        });

        modelBuilder.Entity<SanPhamYeuThich>(entity =>
        {
            entity.HasKey(e => new { e.KhachHangId, e.SanPhamId }).HasName("PK__SanPhamY__D85EA1E40A6F8964");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.KhachHang).WithMany(p => p.SanPhamYeuThiches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_YT_KH");

            entity.HasOne(d => d.SanPham).WithMany(p => p.SanPhamYeuThiches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_YT_SP");
        });

        modelBuilder.Entity<SuDungMaGiamGium>(entity =>
        {
            entity.HasKey(e => e.SuDungId).HasName("PK__SuDungMa__AF189E20DA376EB6");

            entity.Property(e => e.NgaySuDung).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.DonHang).WithOne(p => p.SuDungMaGiamGium)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SDMGG_DH");

            entity.HasOne(d => d.KhachHang).WithMany(p => p.SuDungMaGiamGia).HasConstraintName("FK_SDMGG_KH");

            entity.HasOne(d => d.MaGiamGia).WithMany(p => p.SuDungMaGiamGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SDMGG_Ma");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TaiKhoanId).HasName("PK__TaiKhoan__9A124B654B15B682");

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasMany(d => d.VaiTros).WithMany(p => p.TaiKhoans)
                .UsingEntity<Dictionary<string, object>>(
                    "TaiKhoanVaiTro",
                    r => r.HasOne<VaiTro>().WithMany()
                        .HasForeignKey("VaiTroId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TKVT_VaiTro"),
                    l => l.HasOne<TaiKhoan>().WithMany()
                        .HasForeignKey("TaiKhoanId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TKVT_TaiKhoan"),
                    j =>
                    {
                        j.HasKey("TaiKhoanId", "VaiTroId").HasName("PK__TaiKhoan__EE651376ABC05AF8");
                        j.ToTable("TaiKhoan_VaiTro");
                        j.IndexerProperty<int>("TaiKhoanId").HasColumnName("TaiKhoanID");
                        j.IndexerProperty<int>("VaiTroId").HasColumnName("VaiTroID");
                    });
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.ThanhToanId).HasName("PK__ThanhToa__24A8D6845501EA77");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai).HasDefaultValue("CHO_THANH_TOAN");

            entity.HasOne(d => d.DonHang).WithMany(p => p.ThanhToans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TT_DH");

            entity.HasOne(d => d.PhuongThuc).WithMany(p => p.ThanhToans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TT_PT");
        });

        modelBuilder.Entity<ThuongHieu>(entity =>
        {
            entity.HasKey(e => e.ThuongHieuId).HasName("PK__ThuongHi__4430751C4BF4B546");

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
        });

        modelBuilder.Entity<TokenBaoMat>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__TokenBao__658FEE8A16D2FD6B");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.TokenBaoMats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Token_TaiKhoan");
        });

        modelBuilder.Entity<TonKho>(entity =>
        {
            entity.HasKey(e => new { e.KhoId, e.BienTheId }).HasName("PK__TonKho__F9CBBF9D8B234966");

            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.BienThe).WithMany(p => p.TonKhos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TK_BT");

            entity.HasOne(d => d.Kho).WithMany(p => p.TonKhos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TK_Kho");
        });

        modelBuilder.Entity<TraHang>(entity =>
        {
            entity.HasKey(e => e.TraHangId).HasName("PK__TraHang__2506CD67CE8AC788");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai).HasDefaultValue("YEU_CAU");

            entity.HasOne(d => d.DonHang).WithMany(p => p.TraHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TH_DH");
        });

        modelBuilder.Entity<TrangNoiDung>(entity =>
        {
            entity.HasKey(e => e.TrangId).HasName("PK__TrangNoi__9B1AB183C081881D");

            entity.Property(e => e.DangHienThi).HasDefaultValue(true);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.VaiTroId).HasName("PK__VaiTro__47758136F3A1878D");
        });

        modelBuilder.Entity<VwDoanhThuTheoNgay>(entity =>
        {
            entity.ToView("vw_DoanhThuTheoNgay");
        });

        modelBuilder.Entity<VwTopBanChay>(entity =>
        {
            entity.ToView("vw_TopBanChay");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
