using System.ComponentModel.DataAnnotations;

namespace KitchenHome_WsiteBanHang.Areas.Login_Wsite.Models
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        public string ReturnUrl { get; set; }
    }
}
