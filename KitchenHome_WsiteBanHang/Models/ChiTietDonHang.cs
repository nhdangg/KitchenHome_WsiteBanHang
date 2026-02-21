using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("ChiTietDonHang")]
[Index("DonHangId", Name = "IX_CTDH_DonHang")]
public partial class ChiTietDonHang
{
    [Key]
    [Column("ChiTietDonHangID")]
    public long ChiTietDonHangId { get; set; }

    [Column("DonHangID")]
    public long DonHangId { get; set; }

    [Column("BienTheID")]
    public int BienTheId { get; set; }

    [StringLength(200)]
    public string TenSanPhamLuu { get; set; } = null!;

    [Column("SKU")]
    [StringLength(60)]
    [Unicode(false)]
    public string Sku { get; set; } = null!;

    public int SoLuong { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DonGia { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal GiamGiaDong { get; set; }

    [Column(TypeName = "decimal(30, 2)")]

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal? ThanhTien { get; set; }

    [ForeignKey("BienTheId")]
    [InverseProperty("ChiTietDonHangs")]
    public virtual BienTheSanPham BienThe { get; set; } = null!;

    [ForeignKey("DonHangId")]
    [InverseProperty("ChiTietDonHangs")]
    public virtual DonHang DonHang { get; set; } = null!;
}
