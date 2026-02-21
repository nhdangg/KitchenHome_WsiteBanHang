using System;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models.Context
{
    public partial class DbConnect_KitchenHome_WsiteBanHang : DbContext
    {
        public DbConnect_KitchenHome_WsiteBanHang()
        {
        }

        public DbConnect_KitchenHome_WsiteBanHang(DbContextOptions<DbConnect_KitchenHome_WsiteBanHang> options)
            : base(options)
        {
        }

        // --- KHAI BÁO CÁC BẢNG (DbSet) ---
        // Tên Property (ví dụ SanPhams) phải trùng với tên bạn gọi trong Controller
        public virtual DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public virtual DbSet<VaiTro> VaiTros { get; set; } = null!;
        // Bảng trung gian (nếu có class riêng) hoặc cấu hình trong OnModelCreating
        // public virtual DbSet<TaiKhoan_VaiTro> TaiKhoan_VaiTros { get; set; } 

        public virtual DbSet<KhachHang> KhachHangs { get; set; } = null!;
        public virtual DbSet<DiaChiKhachHang> DiaChiKhachHangs { get; set; } = null!;

        public virtual DbSet<DanhMuc> DanhMucs { get; set; } = null!;
        public virtual DbSet<ThuongHieu> ThuongHieus { get; set; } = null!;
        public virtual DbSet<DonViTinh> DonViTinhs { get; set; } = null!;
        public virtual DbSet<SanPham> SanPhams { get; set; } = null!;
        public virtual DbSet<BienTheSanPham> BienTheSanPhams { get; set; } = null!;
        public virtual DbSet<HinhAnhSanPham> HinhAnhSanPhams { get; set; } = null!;

        public virtual DbSet<Kho> Khos { get; set; } = null!;
        public virtual DbSet<TonKho> TonKhos { get; set; } = null!;
        public virtual DbSet<NhatKyKho> NhatKyKhos { get; set; } = null!;

        public virtual DbSet<GioHang> GioHangs { get; set; } = null!;
        public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; } = null!;
        public virtual DbSet<SanPhamYeuThich> SanPhamYeuThiches { get; set; } = null!;

        public virtual DbSet<PhuongThucThanhToan> PhuongThucThanhToans { get; set; } = null!;
        public virtual DbSet<DonHang> DonHangs { get; set; } = null!;
        public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; } = null!;
        public virtual DbSet<DonHangDiaChiGiao> DonHang_DiaChiGiaos { get; set; } = null!;
        public virtual DbSet<LichSuTrangThaiDonHang> LichSuTrangThaiDonHangs { get; set; } = null!;

        public virtual DbSet<ThanhToan> ThanhToans { get; set; } = null!;
        public virtual DbSet<HoanTien> HoanTiens { get; set; } = null!;

        public virtual DbSet<TraHang> TraHangs { get; set; } = null!;
        public virtual DbSet<ChiTietTraHang> ChiTietTraHangs { get; set; } = null!;

        public virtual DbSet<KhuyenMai> KhuyenMais { get; set; } = null!;
        public virtual DbSet<MaGiamGia> MaGiamGias { get; set; } = null!;
        public virtual DbSet<SuDungMaGiamGium> SuDungMaGiamGias { get; set; } = null!;

        // Các bảng n-n nếu bạn có tạo model riêng
        // public virtual DbSet<KhuyenMai_BienThe> KhuyenMai_BienThes { get; set; }

        public virtual DbSet<DanhGiaSanPham> DanhGiaSanPhams { get; set; } = null!;

        public virtual DbSet<Banner> Banners { get; set; } = null!;
        public virtual DbSet<TrangNoiDung> TrangNoiDungs { get; set; } = null!;
        public virtual DbSet<NhatKyHeThong> NhatKyHeThongs { get; set; } = null!;

        public virtual DbSet<NhatKyCongThanhToan> NhatKyCongThanhToans { get; set; } = null!;

        // --- CẤU HÌNH (FLUENT API) ---

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SuDungMaGiamGium>().ToTable("SuDungMaGiamGia");
            modelBuilder.Entity<MaGiamGia>().ToTable("MaGiamGia");

            // ====== FIX QUAN HỆ TaiKhoan - VaiTro ======
            modelBuilder.Entity<TaiKhoan>()
             .HasMany(t => t.VaiTros)
             .WithMany(v => v.TaiKhoans)
             .UsingEntity<Dictionary<string, object>>(
                 "TaiKhoan_VaiTro",
                 j => j
                     .HasOne<VaiTro>()
                     .WithMany()
                     .HasForeignKey("VaiTroID")
                     .OnDelete(DeleteBehavior.Cascade),
                 j => j
                     .HasOne<TaiKhoan>()
                     .WithMany()
                     .HasForeignKey("TaiKhoanID")
                     .OnDelete(DeleteBehavior.Cascade),
                 j =>
                 {
                     // 🔥 DÒNG QUAN TRỌNG NHẤT
                     j.HasKey("TaiKhoanID", "VaiTroID");

                     j.ToTable("TaiKhoan_VaiTro");
                 }
             );


            // ====== COMPOSITE KEY KHÁC ======
            modelBuilder.Entity<TonKho>(entity =>
            {
                entity.HasKey(e => new { e.KhoId, e.BienTheId });
                entity.ToTable("TonKho");
            });

            modelBuilder.Entity<SanPhamYeuThich>(entity =>
            {
                entity.HasKey(e => new { e.KhachHangId, e.SanPhamId });
                entity.ToTable("SanPhamYeuThich");
            });

            modelBuilder.Entity<SanPham>().ToTable("SanPham");
            modelBuilder.Entity<DanhMuc>().ToTable("DanhMuc");
            modelBuilder.Entity<BienTheSanPham>().ToTable("BienTheSanPham");
            modelBuilder.Entity<DonHang>().ToTable("DonHang");
            modelBuilder.Entity<ChiTietDonHang>().ToTable("ChiTietDonHang");

            base.OnModelCreating(modelBuilder);
        }

    }
}