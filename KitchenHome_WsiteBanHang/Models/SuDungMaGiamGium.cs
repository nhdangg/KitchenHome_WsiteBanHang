using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Index("DonHangId", Name = "UQ__SuDungMa__D159F4DF4A6C415C", IsUnique = true)]
public partial class SuDungMaGiamGium
{
    [Key]
    [Column("SuDungID")]
    public long SuDungId { get; set; }

    [Column("MaGiamGiaID")]
    public int MaGiamGiaId { get; set; }

    [Column("DonHangID")]
    public long DonHangId { get; set; }

    [Column("KhachHangID")]
    public int? KhachHangId { get; set; }

    public DateTime NgaySuDung { get; set; }

    [ForeignKey("DonHangId")]
    [InverseProperty("SuDungMaGiamGium")]
    public virtual DonHang DonHang { get; set; } = null!;

    [ForeignKey("KhachHangId")]
    [InverseProperty("SuDungMaGiamGia")]
    public virtual KhachHang? KhachHang { get; set; }

    [ForeignKey("MaGiamGiaId")]
    [InverseProperty("SuDungMaGiamGia")]
    public virtual MaGiamGia MaGiamGia { get; set; } = null!;
}
