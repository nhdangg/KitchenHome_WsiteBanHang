using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("SanPham")]
[Index("DanhMucId", Name = "IX_SP_DanhMuc")]
[Index("Slug", Name = "UQ__SanPham__BC7B5FB63A42126D", IsUnique = true)]
[Index("MaSanPham", Name = "UQ__SanPham__FAC7442CDCEF4187", IsUnique = true)]
public partial class SanPham
{
    [Key]
    [Column("SanPhamID")]
    public int SanPhamId { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string ? MaSanPham { get; set; }

    [StringLength(200)]
    public string ? TenSanPham { get; set; }

    [StringLength(220)]
    [Unicode(false)]
    public string ? Slug { get; set; }

    [Column("DanhMucID")]
    public int DanhMucId { get; set; }

    [Column("ThuongHieuID")]
    public int? ThuongHieuId { get; set; }

    [Column("DonViTinhID")]
    public int DonViTinhId { get; set; }

    [StringLength(500)]
    public string? MoTaNgan { get; set; }

    public string? MoTaChiTiet { get; set; }

    [StringLength(500)]
    public string? AnhDaiDien { get; set; }

    public bool DangHoatDong { get; set; }

    public bool HienThiTrangChu { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    [InverseProperty("SanPham")]
    public virtual ICollection<BienTheSanPham> BienTheSanPhams { get; set; } = new List<BienTheSanPham>();

    [InverseProperty("SanPham")]
    public virtual ICollection<DanhGiaSanPham> DanhGiaSanPhams { get; set; } = new List<DanhGiaSanPham>();

    [ForeignKey("DanhMucId")]
    [InverseProperty("SanPhams")]
    public virtual DanhMuc ? DanhMuc { get; set; }

    [ForeignKey("DonViTinhId")]
    [InverseProperty("SanPhams")]
    public virtual DonViTinh ? DonViTinh { get; set; }

    [InverseProperty("SanPham")]
    public virtual ICollection<SanPhamYeuThich> SanPhamYeuThiches { get; set; } = new List<SanPhamYeuThich>();

    [ForeignKey("ThuongHieuId")]
    [InverseProperty("SanPhams")]
    public virtual ThuongHieu? ThuongHieu { get; set; }
}
