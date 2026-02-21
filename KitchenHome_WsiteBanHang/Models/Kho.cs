using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("Kho")]
[Index("MaKho", Name = "UQ__Kho__3BDA9351BB958B5D", IsUnique = true)]
public partial class Kho
{
    [Key]
    [Column("KhoID")]
    public int KhoId { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string MaKho { get; set; } = null!;

    [StringLength(150)]
    public string TenKho { get; set; } = null!;

    [StringLength(255)]
    public string? DiaChi { get; set; }

    public bool DangHoatDong { get; set; }

    [InverseProperty("Kho")]
    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    [InverseProperty("Kho")]
    public virtual ICollection<NhatKyKho> NhatKyKhos { get; set; } = new List<NhatKyKho>();

    [InverseProperty("Kho")]
    public virtual ICollection<TonKho> TonKhos { get; set; } = new List<TonKho>();
}
