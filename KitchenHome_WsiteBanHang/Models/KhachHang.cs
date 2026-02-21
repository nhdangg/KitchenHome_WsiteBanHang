using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("KhachHang")]
[Index("MaKhachHang", Name = "UQ__KhachHan__88D2F0E4E14584FF", IsUnique = true)]
[Index("TaiKhoanId", Name = "UQ__KhachHan__9A124B6468383A5A", IsUnique = true)]
public partial class KhachHang
{
    [Key]
    [Column("KhachHangID")]
    public int KhachHangId { get; set; }

    [Column("TaiKhoanID")]
    public int? TaiKhoanId { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string MaKhachHang { get; set; } = null!;

    [StringLength(150)]
    public string HoTen { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string SoDienThoai { get; set; } = null!;

    [StringLength(120)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(10)]
    public string? GioiTinh { get; set; }

    public DateOnly? NgaySinh { get; set; }

    [StringLength(255)]
    public string? GhiChu { get; set; }

    public bool DangHoatDong { get; set; }

    public DateTime NgayTao { get; set; }

    [InverseProperty("KhachHang")]
    public virtual ICollection<DanhGiaSanPham> DanhGiaSanPhams { get; set; } = new List<DanhGiaSanPham>();

    [InverseProperty("KhachHang")]
    public virtual ICollection<DiaChiKhachHang> DiaChiKhachHangs { get; set; } = new List<DiaChiKhachHang>();

    [InverseProperty("KhachHang")]
    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    [InverseProperty("KhachHang")]
    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    [InverseProperty("KhachHang")]
    public virtual ICollection<SanPhamYeuThich> SanPhamYeuThiches { get; set; } = new List<SanPhamYeuThich>();

    [InverseProperty("KhachHang")]
    public virtual ICollection<SuDungMaGiamGium> SuDungMaGiamGia { get; set; } = new List<SuDungMaGiamGium>();

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("KhachHang")]
    public virtual TaiKhoan? TaiKhoan { get; set; }
}
