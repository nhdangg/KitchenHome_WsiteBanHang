using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("TraHang")]
[Index("MaTraHang", Name = "UQ__TraHang__DC08D9BE567927DF", IsUnique = true)]
public partial class TraHang
{
    [Key]
    [Column("TraHangID")]
    public long TraHangId { get; set; }

    [StringLength(40)]
    [Unicode(false)]
    public string MaTraHang { get; set; } = null!;

    [Column("DonHangID")]
    public long DonHangId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string TrangThai { get; set; } = null!;

    [StringLength(255)]
    public string? LyDo { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SoTienDuKienHoan { get; set; }

    public DateTime NgayTao { get; set; }

    [InverseProperty("TraHang")]
    public virtual ICollection<ChiTietTraHang> ChiTietTraHangs { get; set; } = new List<ChiTietTraHang>();

    [ForeignKey("DonHangId")]
    [InverseProperty("TraHangs")]
    public virtual DonHang? DonHang { get; set; }
}
