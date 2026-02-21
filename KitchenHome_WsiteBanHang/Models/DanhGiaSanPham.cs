using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("DanhGiaSanPham")]
[Index("SanPhamId", "NgayTao", Name = "IX_DG_SP", IsDescending = new[] { false, true })]
public partial class DanhGiaSanPham
{
    [Key]
    [Column("DanhGiaID")]
    public long DanhGiaId { get; set; }

    [Column("SanPhamID")]
    public int SanPhamId { get; set; }

    [Column("KhachHangID")]
    public int? KhachHangId { get; set; }

    public byte SoSao { get; set; }

    [StringLength(200)]
    public string? TieuDe { get; set; }

    [StringLength(2000)]
    public string? NoiDung { get; set; }

    public bool HienThi { get; set; }

    [Column("DonHangID")]
    public long? DonHangId { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("DonHangId")]
    [InverseProperty("DanhGiaSanPhams")]
    public virtual DonHang? DonHang { get; set; }

    [ForeignKey("KhachHangId")]
    [InverseProperty("DanhGiaSanPhams")]
    public virtual KhachHang? KhachHang { get; set; }

    [ForeignKey("SanPhamId")]
    [InverseProperty("DanhGiaSanPhams")]
    public virtual SanPham SanPham { get; set; } = null!;
}
