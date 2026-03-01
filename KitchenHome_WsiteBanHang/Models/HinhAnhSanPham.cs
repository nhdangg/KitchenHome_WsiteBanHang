using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("HinhAnhSanPham")]
public partial class HinhAnhSanPham
{
    [Key]
    [Column("HinhAnhID")]
    public long HinhAnhId { get; set; }

    [Column("BienTheID")]
    public int BienTheId { get; set; }

    [StringLength(500)]
    public string ? DuongDan { get; set; }

    public bool LaChinh { get; set; }

    public int ThuTu { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("BienTheId")]
    [InverseProperty("HinhAnhSanPhams")]
    public virtual BienTheSanPham ? BienThe { get; set; }
}
