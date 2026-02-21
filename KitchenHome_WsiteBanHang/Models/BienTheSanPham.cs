using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("BienTheSanPham")]
[Index("SanPhamId", Name = "IX_BT_SP")]
[Index("MaVach", Name = "UQ__BienTheS__8BBF4A1CED9BD97F", IsUnique = true)]
[Index("Sku", Name = "UQ__BienTheS__CA1ECF0D1BCAE4D5", IsUnique = true)]
public partial class BienTheSanPham
{
    [Key]
    [Column("BienTheID")]
    public int BienTheId { get; set; }

    [Column("SanPhamID")]
    public int SanPhamId { get; set; }

    [Column("SKU")]
    [StringLength(60)]
    [Unicode(false)]
    public string Sku { get; set; } = null!;

    [StringLength(60)]
    [Unicode(false)]
    public string? MaVach { get; set; }

    [StringLength(200)]
    public string? TenBienThe { get; set; }

    public string? ThuocTinhJson { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal GiaNhapThamChieu { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal GiaBan { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GiaKhuyenMai { get; set; }

    public int SoLuongToiThieu { get; set; }

    public int? TrongLuongGram { get; set; }

    public bool DangHoatDong { get; set; }

    public DateTime NgayTao { get; set; }

    [InverseProperty("BienThe")]
    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    [InverseProperty("BienThe")]
    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    [InverseProperty("BienThe")]
    public virtual ICollection<ChiTietTraHang> ChiTietTraHangs { get; set; } = new List<ChiTietTraHang>();

    [InverseProperty("BienThe")]
    public virtual ICollection<HinhAnhSanPham> HinhAnhSanPhams { get; set; } = new List<HinhAnhSanPham>();

    [InverseProperty("BienThe")]
    public virtual ICollection<NhatKyKho> NhatKyKhos { get; set; } = new List<NhatKyKho>();

    [ForeignKey("SanPhamId")]
    [InverseProperty("BienTheSanPhams")]
    public virtual SanPham SanPham { get; set; } = null!;

    [InverseProperty("BienThe")]
    public virtual ICollection<TonKho> TonKhos { get; set; } = new List<TonKho>();

    //[ForeignKey("BienTheId")]
    //[InverseProperty("BienThes")];
    //public virtual ICollection<KhuyenMai> KhuyenMais { get; set; } = new List<KhuyenMai>();
}
