using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("PhuongThucThanhToan")]
[Index("MaPhuongThuc", Name = "UQ__PhuongTh__35F7404F80CAAA4A", IsUnique = true)]
public partial class PhuongThucThanhToan
{
    [Key]
    [Column("PhuongThucID")]
    public int PhuongThucId { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string MaPhuongThuc { get; set; } = null!;

    [StringLength(100)]
    public string TenPhuongThuc { get; set; } = null!;

    public bool DangHoatDong { get; set; }

    [InverseProperty("PhuongThuc")]
    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
