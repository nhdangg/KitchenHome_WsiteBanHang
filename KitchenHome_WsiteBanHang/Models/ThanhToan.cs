using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("ThanhToan")]
[Index("DonHangId", Name = "IX_TT_DonHang")]
public partial class ThanhToan
{
    [Key]
    [Column("ThanhToanID")]
    public long ThanhToanId { get; set; }

    [Column("DonHangID")]
    public long DonHangId { get; set; }

    [Column("PhuongThucID")]
    public int PhuongThucId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SoTien { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string TrangThai { get; set; } = null!;

    [StringLength(80)]
    [Unicode(false)]
    public string? MaGiaoDich { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("DonHangId")]
    [InverseProperty("ThanhToans")]
    public virtual DonHang DonHang { get; set; } = null!;

    [InverseProperty("ThanhToan")]
    public virtual ICollection<HoanTien> HoanTiens { get; set; } = new List<HoanTien>();

    [InverseProperty("ThanhToan")]
    public virtual ICollection<NhatKyCongThanhToan> NhatKyCongThanhToans { get; set; } = new List<NhatKyCongThanhToan>();

    [ForeignKey("PhuongThucId")]
    [InverseProperty("ThanhToans")]
    public virtual PhuongThucThanhToan PhuongThuc { get; set; } = null!;
}
