using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("TokenBaoMat")]
public partial class TokenBaoMat
{
    [Key]
    [Column("TokenID")]
    public long TokenId { get; set; }

    [Column("TaiKhoanID")]
    public int TaiKhoanId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string LoaiToken { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string TokenHash { get; set; } = null!;

    public DateTime HetHanLuc { get; set; }

    public bool DaDung { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("TokenBaoMats")]
    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
