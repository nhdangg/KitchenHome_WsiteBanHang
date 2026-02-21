using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Cần thư viện này
using KitchenHome_WsiteBanHang.Models;

namespace KitchenHome_WsiteBanHang.Models.Class_phu
{
    public class CheckoutViewModel
    {
        // --- THÔNG TIN NGƯỜI NHẬN (Lưu vào bảng DonHang_DiaChiGiao) ---
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        public string DiaChiCuThe { get; set; }

        public string TinhThanh { get; set; }
        public string QuanHuyen { get; set; }
        public string PhuongXa { get; set; }

        public string GhiChu { get; set; } // Lưu vào bảng DonHang

        // --- THÔNG TIN THANH TOÁN ---
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public int PhuongThucThanhToanId { get; set; }

        // --- DỮ LIỆU HIỂN THỊ (READONLY) ---
        // ValidateNever: Bỏ qua kiểm tra dữ liệu đầu vào cho các list này
        [ValidateNever]
        public List<CartItemVM> CartItems { get; set; } = new List<CartItemVM>();

        public decimal TongTienHang { get; set; }
        public string? MaGiamGia { get; set; }
        public decimal TienGiam { get; set; }

        public decimal PhiVanChuyen { get; set; } = 0;
        public decimal TongThanhToan => TongTienHang + PhiVanChuyen;

        [ValidateNever]
        public List<PhuongThucThanhToan> DanhSachPhuongThuc { get; set; }
    }
}