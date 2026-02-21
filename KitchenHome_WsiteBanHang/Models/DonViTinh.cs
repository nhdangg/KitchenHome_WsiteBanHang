using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("DonViTinh")]
[Index("TenDonViTinh", Name = "UQ__DonViTin__F370C670BD6F8DD8", IsUnique = true)]
public partial class DonViTinh
{
    [Key]
    [Column("DonViTinhID")]
    public int DonViTinhId { get; set; }

    [StringLength(50)]
    public string TenDonViTinh { get; set; } = null!;

    [StringLength(20)]
    public string? KyHieu { get; set; }

    [InverseProperty("DonViTinh")]
    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
