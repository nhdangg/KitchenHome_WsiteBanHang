using KitchenHome_WsiteBanHang.Helpers;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using KitchenHome_WsiteBanHang.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace KitchenHome_WsiteBanHang.Controllers
{
    public class ThongTinTaiKhoanController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public ThongTinTaiKhoanController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // ================= HELPER =================
        private int? GetUserId()
        {
            return HttpContext.Session.GetInt32("USER_ID");
        }

        private IActionResult RedirectToLogin()
        {
            return RedirectToAction(
                "Index",
                "Home",
                new
                {
                    area = "Login_Wsite",
                    returnUrl = "/ThongTinTaiKhoan"
                }
            );
        }

        // ================= INDEX =================
        public IActionResult Index()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToLogin();

            var taiKhoan = _context.TaiKhoans.Find(userId);
            if (taiKhoan == null)
                return RedirectToLogin();

            var khachHang = _context.KhachHangs
                .FirstOrDefault(k => k.TaiKhoanId == userId);

            ViewBag.KhachHangInfo = khachHang;

            return View(taiKhoan);
        }

        // ================= CẬP NHẬT THÔNG TIN =================
        [HttpGet]
        public IActionResult CapNhat()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToLogin();

            var khachHang = _context.KhachHangs
                .FirstOrDefault(k => k.TaiKhoanId == userId)
                ?? new KhachHang { TaiKhoanId = userId };

            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CapNhat(KhachHang model)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToLogin();

            if (!ModelState.IsValid)
                return View(model);

            var khachHangDB = _context.KhachHangs
                .FirstOrDefault(k => k.TaiKhoanId == userId);

            if (khachHangDB != null)
            {
                khachHangDB.HoTen = model.HoTen;
                khachHangDB.SoDienThoai = model.SoDienThoai;
                khachHangDB.Email = model.Email;
                khachHangDB.GioiTinh = model.GioiTinh;
                khachHangDB.NgaySinh = model.NgaySinh;
                khachHangDB.GhiChu = model.GhiChu;
            }
            else
            {
                model.TaiKhoanId = userId.Value;
                model.MaKhachHang = "KH" + DateTime.Now.ToString("yyyyMMddHHmmss");
                model.DangHoatDong = true;
                model.NgayTao = DateTime.Now;
                _context.KhachHangs.Add(model);
            }

            var taiKhoan = _context.TaiKhoans.Find(userId);
            if (taiKhoan != null)
            {
                taiKhoan.NgayCapNhat = DateTime.Now;
                if (!string.IsNullOrEmpty(model.Email))
                    taiKhoan.Email = model.Email;
                if (!string.IsNullOrEmpty(model.SoDienThoai))
                    taiKhoan.SoDienThoai = model.SoDienThoai;
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Index");
        }

        // ================= ĐỔI MẬT KHẨU =================
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            if (!GetUserId().HasValue)
                return RedirectToLogin();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoiMatKhau(string MatKhauCu, string MatKhauMoi, string XacNhanMatKhau)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToLogin();

            var taiKhoan = _context.TaiKhoans.Find(userId);
            if (taiKhoan == null)
                return RedirectToLogin();

            if (MatKhauMoi != XacNhanMatKhau)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            string oldHash = GetMD5(MatKhauCu);
            if (taiKhoan.MatKhauHash != oldHash && taiKhoan.MatKhauHash != MatKhauCu)
            {
                ModelState.AddModelError("", "Mật khẩu cũ không chính xác.");
                return View();
            }

            taiKhoan.MatKhauHash = GetMD5(MatKhauMoi);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // ================= MD5 =================
        private string GetMD5(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            var sb = new StringBuilder();
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
