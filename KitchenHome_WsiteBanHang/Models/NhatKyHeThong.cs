using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("NhatKyHeThong")]
public partial class NhatKyHeThong
{
    [Key]
    [Column("NhatKyID")]
    public long NhatKyId { get; set; }

    [Column("TaiKhoanID")]
    public int? TaiKhoanId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string HanhDong { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string TenBang { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? KhoaBanGhi { get; set; }

    public string? DuLieuCu { get; set; }

    public string? DuLieuMoi { get; set; }

    [Column("DiaChiIP")]
    [StringLength(45)]
    [Unicode(false)]
    public string? DiaChiIp { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("NhatKyHeThongs")]
    public virtual TaiKhoan? TaiKhoan { get; set; }
}
