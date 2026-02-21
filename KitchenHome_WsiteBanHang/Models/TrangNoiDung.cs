using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("TrangNoiDung")]
[Index("MaTrang", Name = "UQ__TrangNoi__399828AE7A235048", IsUnique = true)]
public partial class TrangNoiDung
{
    [Key]
    [Column("TrangID")]
    public int TrangId { get; set; }

    [StringLength(60)]
    [Unicode(false)]
    public string MaTrang { get; set; } = null!;

    [StringLength(200)]
    public string TieuDe { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public bool DangHienThi { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }
}
