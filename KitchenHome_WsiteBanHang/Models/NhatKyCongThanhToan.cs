using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("NhatKyCongThanhToan")]
public partial class NhatKyCongThanhToan
{
    [Key]
    [Column("NhatKyID")]
    public long NhatKyId { get; set; }

    [Column("ThanhToanID")]
    public long ThanhToanId { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string CongThanhToan { get; set; } = null!;

    public string DuLieuNhan { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string? MaKetQua { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("ThanhToanId")]
    [InverseProperty("NhatKyCongThanhToans")]
    public virtual ThanhToan ThanhToan { get; set; } = null!;
}
