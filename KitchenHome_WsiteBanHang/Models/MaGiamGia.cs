using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Index("Code", Name = "UQ__MaGiamGi__A25C5AA771E20C92", IsUnique = true)]
public partial class MaGiamGia
{
    [Key]
    [Column("MaGiamGiaID")]
    public int MaGiamGiaId { get; set; }

    [StringLength(40)]
    [Unicode(false)]
    public string Code { get; set; } = null!;

    [StringLength(200)]
    public string TenMa { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string LoaiGiam { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal GiaTriGiam { get; set; }

    public DateTime BatDau { get; set; }

    public DateTime KetThuc { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DonHangToiThieu { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GiamToiDa { get; set; }

    public int? GioiHanLuotDung { get; set; }

    public int? GioiHanMoiKhach { get; set; }

    public int DaDung { get; set; }

    public bool DangHoatDong { get; set; }

    [InverseProperty("MaGiamGia")]
    public virtual ICollection<SuDungMaGiamGium> SuDungMaGiamGia { get; set; } = new List<SuDungMaGiamGium>();
}
