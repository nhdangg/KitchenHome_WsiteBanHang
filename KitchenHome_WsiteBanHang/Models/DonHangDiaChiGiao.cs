using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("DonHang_DiaChiGiao")]
public partial class DonHangDiaChiGiao
{
    [Key]
    [Column("DonHangID")]
    public long DonHangId { get; set; }

    [StringLength(150)]
    public string TenNguoiNhan { get; set; } = null!;

    [Column("SDTNguoiNhan")]
    [StringLength(20)]
    [Unicode(false)]
    public string SdtnguoiNhan { get; set; } = null!;

    [StringLength(255)]
    public string DiaChiCuThe { get; set; } = null!;

    [StringLength(100)]
    public string? PhuongXa { get; set; }

    [StringLength(100)]
    public string? QuanHuyen { get; set; }

    [StringLength(100)]
    public string? TinhThanh { get; set; }

    [StringLength(255)]
    public string? GhiChuGiao { get; set; }

    [ForeignKey("DonHangId")]
    [InverseProperty("DonHangDiaChiGiao")]
    public virtual DonHang DonHang { get; set; } = null!;
}
