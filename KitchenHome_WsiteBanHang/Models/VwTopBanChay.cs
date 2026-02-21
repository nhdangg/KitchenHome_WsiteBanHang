using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Keyless]
public partial class VwTopBanChay
{
    [Column("BienTheID")]
    public int BienTheId { get; set; }

    [Column("SKU")]
    [StringLength(60)]
    [Unicode(false)]
    public string Sku { get; set; } = null!;

    [StringLength(200)]
    public string TenSanPham { get; set; } = null!;

    public int? TongSoLuong { get; set; }

    [Column(TypeName = "decimal(38, 2)")]
    public decimal? TongTien { get; set; }
}
