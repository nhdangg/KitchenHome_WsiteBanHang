using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("TaiKhoan")]
[Index("TenDangNhap", Name = "UQ__TaiKhoan__55F68FC06CB20B37", IsUnique = true)]
public partial class TaiKhoan
{
    [Key]
    [Column("TaiKhoanID")]
    public int TaiKhoanId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string TenDangNhap { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string MatKhauHash { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string? MuoiHash { get; set; }

    [StringLength(120)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? SoDienThoai { get; set; }

    public bool DangHoatDong { get; set; }

    public bool EmailDaXacMinh { get; set; }

    public bool DienThoaiDaXacMinh { get; set; }

    public DateTime? LanDangNhapCuoi { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    [InverseProperty("TaiKhoan")]
    public virtual KhachHang? KhachHang { get; set; }

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<NhatKyHeThong> NhatKyHeThongs { get; set; } = new List<NhatKyHeThong>();

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<TokenBaoMat> TokenBaoMats { get; set; } = new List<TokenBaoMat>();

 
    public virtual ICollection<VaiTro> VaiTros { get; set; } = new List<VaiTro>();
}
