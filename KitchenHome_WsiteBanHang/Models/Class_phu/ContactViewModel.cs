using System.ComponentModel.DataAnnotations;

namespace KitchenHome_WsiteBanHang.Models.Class_phu
{
    public class ContactViewModel
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chủ đề")]
        public string ChuDe { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string NoiDung { get; set; } = null!;
    }
}