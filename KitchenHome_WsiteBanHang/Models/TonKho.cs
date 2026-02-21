using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[PrimaryKey("KhoId", "BienTheId")]
[Table("TonKho")]
public partial class TonKho
{
    [Key]
    [Column("KhoID")]
    public int KhoId { get; set; }

    [Key]
    [Column("BienTheID")]
    public int BienTheId { get; set; }

    public int SoLuongTon { get; set; }

    public int SoLuongGiuCho { get; set; }

    public int MucDatHangLai { get; set; }

    public DateTime NgayCapNhat { get; set; }

    [ForeignKey("BienTheId")]
    [InverseProperty("TonKhos")]
    public virtual BienTheSanPham BienThe { get; set; } = null!;

    [ForeignKey("KhoId")]
    [InverseProperty("TonKhos")]
    public virtual Kho Kho { get; set; } = null!;
}
