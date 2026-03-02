using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using KitchenHome_WsiteBanHang.Models.Context;

namespace WsiteBanHang_GiaDung.Areas.Thu_Ngan.Controllers
{
    [Area("Thu_Ngan")]
    public class InformationController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public InformationController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // 1. XEM THÔNG TIN
        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("USER_ID");
            if (userId == null)
                return RedirectToAction("Index", "Login", new { area = "" });

            var taiKhoan = _context.TaiKhoans.Find(userId.Value);
            if (taiKhoan == null)
                return RedirectToAction("Index", "Login", new { area = "" });

            return View(taiKhoan);
        }

        // 2. CẬP NHẬT THÔNG TIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CapNhat(string Email, string SoDienThoai)
        {
            int? userId = HttpContext.Session.GetInt32("USER_ID");
            if (userId == null)
                return RedirectToAction("Index", "Login", new { area = "" });

            var taiKhoan = _context.TaiKhoans.Find(userId.Value);
            if (taiKhoan != null)
            {
                taiKhoan.Email = Email;
                taiKhoan.SoDienThoai = SoDienThoai;
                taiKhoan.NgayCapNhat = DateTime.Now;

                _context.SaveChanges();

                TempData["Message"] = "Cập nhật hồ sơ thành công!";
                TempData["Type"] = "success";
            }

            return RedirectToAction("Index");
        }

        // 3. ĐỔI MẬT KHẨU (BCrypt)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoiMatKhau(string MatKhauCu, string MatKhauMoi, string XacNhanMatKhau)
        {
            int? userId = HttpContext.Session.GetInt32("USER_ID");
            if (userId == null)
                return RedirectToAction("Index", "Login", new { area = "" });

            var taiKhoan = _context.TaiKhoans.Find(userId.Value);
            if (taiKhoan == null)
                return RedirectToAction("Index", "Login", new { area = "" });

            if (string.IsNullOrEmpty(MatKhauCu) || string.IsNullOrEmpty(MatKhauMoi))
            {
                TempData["Message"] = "Vui lòng nhập đầy đủ thông tin!";
                TempData["Type"] = "error";
                return RedirectToAction("Index");
            }

            if (MatKhauMoi != XacNhanMatKhau)
            {
                TempData["Message"] = "Mật khẩu xác nhận không khớp!";
                TempData["Type"] = "error";
                return RedirectToAction("Index");
            }

            // ✅ VERIFY BCRYPT
            bool isCorrect = BCrypt.Net.BCrypt.Verify(MatKhauCu, taiKhoan.MatKhauHash);

            if (!isCorrect)
            {
                TempData["Message"] = "Mật khẩu cũ không chính xác!";
                TempData["Type"] = "error";
                return RedirectToAction("Index");
            }

            // ✅ HASH MẬT KHẨU MỚI
            taiKhoan.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(MatKhauMoi);
            taiKhoan.NgayCapNhat = DateTime.Now;

            _context.SaveChanges();

            TempData["Message"] = "Đổi mật khẩu thành công!";
            TempData["Type"] = "success";

            return RedirectToAction("Index");
        }
    }
}