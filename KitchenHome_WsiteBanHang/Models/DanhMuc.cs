using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("DanhMuc")]
[Index("DanhMucChaId", Name = "IX_DanhMuc_Cha")]
[Index("Slug", Name = "UQ__DanhMuc__BC7B5FB61EEB4093", IsUnique = true)]
public partial class DanhMuc
{
    [Key]
    [Column("DanhMucID")]
    public int DanhMucId { get; set; }

    [Column("DanhMucChaID")]
    public int? DanhMucChaId { get; set; }

    [StringLength(150)]
    public string TenDanhMuc { get; set; } = null!;

    [StringLength(200)]
    [Unicode(false)]
    public string Slug { get; set; } = null!;

    public bool DangHoatDong { get; set; }

    public int ThuTu { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("DanhMucChaId")]
    [InverseProperty("InverseDanhMucCha")]
    public virtual DanhMuc? DanhMucCha { get; set; }

    [InverseProperty("DanhMucCha")]
    public virtual ICollection<DanhMuc> InverseDanhMucCha { get; set; } = new List<DanhMuc>();

    [InverseProperty("DanhMuc")]
    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
