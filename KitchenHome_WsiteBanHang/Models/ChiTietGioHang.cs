using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("ChiTietGioHang")]
[Index("GioHangId", Name = "IX_CTGH_GioHang")]
public partial class ChiTietGioHang
{
    [Key]
    [Column("ChiTietGioHangID")]
    public long ChiTietGioHangId { get; set; }

    [Column("GioHangID")]
    public long GioHangId { get; set; }

    [Column("BienTheID")]
    public int BienTheId { get; set; }

    public int SoLuong { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DonGiaTaiThoiDiem { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("BienTheId")]
    [InverseProperty("ChiTietGioHangs")]
    public virtual BienTheSanPham BienThe { get; set; } = null!;

    [ForeignKey("GioHangId")]
    [InverseProperty("ChiTietGioHangs")]
    public virtual GioHang GioHang { get; set; } = null!;
}
