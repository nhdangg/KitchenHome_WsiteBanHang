using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("DiaChiKhachHang")]
public partial class DiaChiKhachHang
{
    [Key]
    [Column("DiaChiID")]
    public int DiaChiId { get; set; }

    [Column("KhachHangID")]
    public int KhachHangId { get; set; }

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

    public bool MacDinh { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("KhachHangId")]
    [InverseProperty("DiaChiKhachHangs")]
    public virtual KhachHang KhachHang { get; set; } = null!;
}
