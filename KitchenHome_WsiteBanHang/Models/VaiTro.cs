using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Models;

[Table("VaiTro")]
[Index("MaVaiTro", Name = "UQ__VaiTro__C24C41CE49A1717A", IsUnique = true)]
public partial class VaiTro
{
    [Key]
    [Column("VaiTroID")]
    public int VaiTroId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string MaVaiTro { get; set; } = null!;

    [StringLength(100)]
    public string TenVaiTro { get; set; } = null!;

    public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
