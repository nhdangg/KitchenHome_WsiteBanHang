using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("ThuongHieu")]
[Index("TenThuongHieu", Name = "UQ__ThuongHi__98D6A83411C99517", IsUnique = true)]
public partial class ThuongHieu
{
    [Key]
    [Column("ThuongHieuID")]
    public int ThuongHieuId { get; set; }

    [StringLength(150)]
    public string TenThuongHieu { get; set; } = null!;

    [StringLength(100)]
    public string? QuocGia { get; set; }

    public bool DangHoatDong { get; set; }

    [InverseProperty("ThuongHieu")]
    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
