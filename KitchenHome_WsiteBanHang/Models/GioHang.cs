using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("GioHang")]
public partial class GioHang
{
    [Key]
    [Column("GioHangID")]
    public long GioHangId { get; set; }

    [Column("TaiKhoanID")]
    public int? TaiKhoanId { get; set; }

    [Column("KhachHangID")]
    public int? KhachHangId { get; set; }

    [StringLength(80)]
    [Unicode(false)]
    public string? MaPhien { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string TrangThai { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    [InverseProperty("GioHang")]
    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    [ForeignKey("KhachHangId")]
    [InverseProperty("GioHangs")]
    public virtual KhachHang? KhachHang { get; set; }

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("GioHangs")]
    public virtual TaiKhoan? TaiKhoan { get; set; }
}
