using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Keyless]
public partial class VwDoanhThuTheoNgay
{
    public DateOnly? Ngay { get; set; }

    public int? SoDon { get; set; }

    [Column(TypeName = "decimal(38, 2)")]
    public decimal? DoanhThu { get; set; }
}
