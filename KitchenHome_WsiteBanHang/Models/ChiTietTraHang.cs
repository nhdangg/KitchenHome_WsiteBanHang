using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("ChiTietTraHang")]
public partial class ChiTietTraHang
{
    [Key]
    [Column("ChiTietTraHangID")]
    public long ChiTietTraHangId { get; set; }

    [Column("TraHangID")]
    public long TraHangId { get; set; }

    [Column("BienTheID")]
    public int BienTheId { get; set; }

    public int SoLuong { get; set; }

    [StringLength(255)]
    public string? TinhTrangHang { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DonGiaHoan { get; set; }

    [Column(TypeName = "decimal(29, 2)")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal? ThanhTienHoan { get; set; }

    [ForeignKey("BienTheId")]
    [InverseProperty("ChiTietTraHangs")]
    public virtual BienTheSanPham BienThe { get; set; } = null!;

    [ForeignKey("TraHangId")]
    [InverseProperty("ChiTietTraHangs")]
    public virtual TraHang TraHang { get; set; } = null!;
}
