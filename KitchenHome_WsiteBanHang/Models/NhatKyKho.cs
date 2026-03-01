using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("NhatKyKho")]
[Index("KhoId", "BienTheId", "NgayTao", Name = "IX_NKK_Kho_BT", IsDescending = new[] { false, false, true })]
[Index("MaNhatKy", Name = "UQ__NhatKyKh__E42EF42F38BB8CF2", IsUnique = true)]
public partial class NhatKyKho
{
    [Key]
    [Column("NhatKyKhoID")]
    public long NhatKyKhoId { get; set; }

    [StringLength(40)]
    [Unicode(false)]
    public string? MaNhatKy { get; set; }

    [Column("KhoID")]
    public int KhoId { get; set; }

    [Column("BienTheID")]
    public int BienTheId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? LoaiPhatSinh { get; set; }

    public int SoLuong { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? LoaiThamChieu { get; set; }

    [Column("ThamChieuID")]
    public long? ThamChieuId { get; set; }

    [StringLength(255)]
    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("BienTheId")]
    [InverseProperty("NhatKyKhos")]
    public virtual BienTheSanPham ? BienThe { get; set; }

    [ForeignKey("KhoId")]
    [InverseProperty("NhatKyKhos")]
    public virtual Kho ? Kho { get; set; }
}
