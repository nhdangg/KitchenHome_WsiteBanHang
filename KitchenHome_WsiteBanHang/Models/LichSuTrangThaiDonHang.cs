using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("LichSuTrangThaiDonHang")]
public partial class LichSuTrangThaiDonHang
{
    [Key]
    [Column("LichSuID")]
    public long LichSuId { get; set; }

    [Column("DonHangID")]
    public long DonHangId { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? TrangThaiCu { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string TrangThaiMoi { get; set; } = null!;

    [StringLength(255)]
    public string? GhiChu { get; set; }

    [Column("NguoiThucHienID")]
    public int? NguoiThucHienId { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("DonHangId")]
    [InverseProperty("LichSuTrangThaiDonHangs")]
    public virtual DonHang DonHang { get; set; } = null!;
}
