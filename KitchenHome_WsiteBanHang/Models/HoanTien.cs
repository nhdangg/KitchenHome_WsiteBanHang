using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("HoanTien")]
public partial class HoanTien
{
    [Key]
    [Column("HoanTienID")]
    public long HoanTienId { get; set; }

    [Column("ThanhToanID")]
    public long ThanhToanId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SoTienHoan { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string TrangThai { get; set; } = null!;

    [StringLength(255)]
    public string? LyDo { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("ThanhToanId")]
    [InverseProperty("HoanTiens")]
    public virtual ThanhToan ThanhToan { get; set; } = null!;
}
