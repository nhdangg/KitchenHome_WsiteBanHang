using System.ComponentModel.DataAnnotations;

namespace KitchenHome_WsiteBanHang.Areas.Login_Wsite.Models
{
    public class DangKyTKVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string TenDangNhap { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string XacNhanMatKhau { get; set; } = null!;

        [EmailAddress]
        public string? Email { get; set; }
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
    }
}