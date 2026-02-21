using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;

namespace WsiteBanHang_GiaDung.Areas.ADMIN.Controllers
{
    [Area("ADMIN")]
    public class InformationADMINController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public InformationADMINController(DbConnect_KitchenHome_WsiteBanHang context)
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

        // 3. ĐỔI MẬT KHẨU
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoiMatKhau(string MatKhauCu, string MatKhauMoi, string XacNhanMatKhau)
        {
            int? userId = HttpContext.Session.GetInt32("USER_ID");
            if (userId == null)
                return RedirectToAction("Index", "Login", new { area = "" });

            var taiKhoan = _context.TaiKhoans.Find(userId.Value);

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

            string oldPassHash = GetMD5(MatKhauCu);
            if (taiKhoan.MatKhauHash != oldPassHash)
            {
                TempData["Message"] = "Mật khẩu cũ không chính xác!";
                TempData["Type"] = "error";
                return RedirectToAction("Index");
            }

            taiKhoan.MatKhauHash = GetMD5(MatKhauMoi);
            taiKhoan.NgayCapNhat = DateTime.Now;

            _context.SaveChanges();

            TempData["Message"] = "Đổi mật khẩu thành công!";
            TempData["Type"] = "success";

            return RedirectToAction("Index");
        }

        // 4. HÀM MD5 – GIỮ NGUYÊN
        private static string GetMD5(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                byte[] hash = md5.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
