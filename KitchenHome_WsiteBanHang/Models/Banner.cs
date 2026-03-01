using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("Banner")]
public partial class Banner
{
    [Key]
    [Column("BannerID")]
    public int BannerId { get; set; }

    [StringLength(200)]
    public string? TieuDe { get; set; }

    [StringLength(500)]
    public string ? Anh { get; set; }

    [StringLength(500)]
    public string? Link { get; set; }

    [StringLength(200)]
    public string? Text { get; set; }

    public int ThuTu { get; set; }

    public bool IsActive { get; set; }
}
