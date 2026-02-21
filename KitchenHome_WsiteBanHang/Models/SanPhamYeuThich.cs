using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[PrimaryKey("KhachHangId", "SanPhamId")]
[Table("SanPhamYeuThich")]
public partial class SanPhamYeuThich
{
    [Key]
    [Column("KhachHangID")]
    public int KhachHangId { get; set; }

    [Key]
    [Column("SanPhamID")]
    public int SanPhamId { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("KhachHangId")]
    [InverseProperty("SanPhamYeuThiches")]
    public virtual KhachHang KhachHang { get; set; } = null!;

    [ForeignKey("SanPhamId")]
    [InverseProperty("SanPhamYeuThiches")]
    public virtual SanPham SanPham { get; set; } = null!;
}
