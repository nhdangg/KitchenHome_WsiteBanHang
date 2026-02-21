using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("DonHang")]
[Index("NgayDat", Name = "IX_DH_NgayDat", AllDescending = true)]
[Index("MaDonHang", Name = "UQ__DonHang__129584ACECE5D4CA", IsUnique = true)]
public partial class DonHang
{
    [Key]
    [Column("DonHangID")]
    public long DonHangId { get; set; }

    [StringLength(40)]
    [Unicode(false)]
    public string MaDonHang { get; set; } = null!;

    [Column("KhachHangID")]
    public int? KhachHangId { get; set; }

    [Column("TaiKhoanID")]
    public int? TaiKhoanId { get; set; }

    [Column("KhoID")]
    public int KhoId { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string TrangThai { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string KenhBan { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TamTinh { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal GiamGia { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PhiVanChuyen { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Thue { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TongTien { get; set; }

    [StringLength(255)]
    public string? GhiChu { get; set; }

    public DateTime NgayDat { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    [InverseProperty("DonHang")]
    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    [InverseProperty("DonHang")]
    public virtual ICollection<DanhGiaSanPham> DanhGiaSanPhams { get; set; } = new List<DanhGiaSanPham>();

    [InverseProperty("DonHang")]
    public virtual DonHangDiaChiGiao? DonHangDiaChiGiao { get; set; }

    [ForeignKey("KhachHangId")]
    [InverseProperty("DonHangs")]
    public virtual KhachHang? KhachHang { get; set; }

    [ForeignKey("KhoId")]
    [InverseProperty("DonHangs")]
    public virtual Kho Kho { get; set; } = null!;

    [InverseProperty("DonHang")]
    public virtual ICollection<LichSuTrangThaiDonHang> LichSuTrangThaiDonHangs { get; set; } = new List<LichSuTrangThaiDonHang>();

    [InverseProperty("DonHang")]
    public virtual SuDungMaGiamGium? SuDungMaGiamGium { get; set; }

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("DonHangs")]
    public virtual TaiKhoan? TaiKhoan { get; set; }

    [InverseProperty("DonHang")]
    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();

    [InverseProperty("DonHang")]
    public virtual ICollection<TraHang> TraHangs { get; set; } = new List<TraHang>();
}
