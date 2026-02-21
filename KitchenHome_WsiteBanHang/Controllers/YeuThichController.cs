using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using KitchenHome_WsiteBanHang.Services;
using KitchenHome_WsiteBanHang.Helpers;

namespace KitchenHome_WsiteBanHang.Controllers
{
    public class YeuThichController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;
        private readonly CartService _cartService;

        public YeuThichController(
            DbConnect_KitchenHome_WsiteBanHang context,
            CartService cartService)
        {
            _context = context;
            _cartService = cartService;
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
                    returnUrl = Url.Action("Index", "YeuThich")
                }
            );
        }

        // ================= DANH SÁCH YÊU THÍCH =================
        public IActionResult Index()
        {
            // 1. Badge giỏ hàng (header)
            var maPhien = CartCookie.GetOrCreate(HttpContext);
            int? taiKhoanId = GetUserId();
            ViewBag.CartCount = _cartService.GetCartCount(taiKhoanId, maPhien);

            // 2. Bắt buộc đăng nhập
            if (!taiKhoanId.HasValue)
                return RedirectToLogin();

            // 3. Lấy khách hàng
            var khachHang = _context.KhachHangs
                .FirstOrDefault(k => k.TaiKhoanId == taiKhoanId);

            if (khachHang == null)
                return RedirectToAction("Index", "Home");

            // 4. Danh sách sản phẩm yêu thích
            // (GIỮ NGUYÊN FIX HẾT HÀNG CỦA BẠN)
            var listYeuThich = _context.SanPhamYeuThiches
                .Include(yt => yt.SanPham)
                    .ThenInclude(sp => sp.BienTheSanPhams)
                .Where(yt => yt.KhachHangId == khachHang.KhachHangId)
                .Select(yt => yt.SanPham)
                .ToList();

            return View(listYeuThich);
        }

        // ================= XOÁ YÊU THÍCH =================
        public IActionResult XoaYeuThich(int id)
        {
            int? taiKhoanId = GetUserId();
            if (!taiKhoanId.HasValue)
                return RedirectToLogin();

            var khachHang = _context.KhachHangs
                .FirstOrDefault(k => k.TaiKhoanId == taiKhoanId);

            if (khachHang != null)
            {
                var item = _context.SanPhamYeuThiches
                    .FirstOrDefault(x =>
                        x.SanPhamId == id &&
                        x.KhachHangId == khachHang.KhachHangId);

                if (item != null)
                {
                    _context.SanPhamYeuThiches.Remove(item);
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        // ================= TOGGLE TIM (AJAX) =================
        [HttpPost]
        public IActionResult CapNhat(int id)
        {
            int? taiKhoanId = GetUserId();
            if (!taiKhoanId.HasValue)
            {
                return Json(new
                {
                    success = false,
                    requireLogin = true,
                    message = "Bạn cần đăng nhập để thực hiện chức năng này."
                });
            }

            var khachHang = _context.KhachHangs
                .FirstOrDefault(k => k.TaiKhoanId == taiKhoanId);

            if (khachHang == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi dữ liệu khách hàng."
                });
            }

            var existingItem = _context.SanPhamYeuThiches
                .FirstOrDefault(x =>
                    x.KhachHangId == khachHang.KhachHangId &&
                    x.SanPhamId == id);

            bool isLiked;

            if (existingItem != null)
            {
                _context.SanPhamYeuThiches.Remove(existingItem);
                isLiked = false;
            }
            else
            {
                _context.SanPhamYeuThiches.Add(new SanPhamYeuThich
                {
                    KhachHangId = khachHang.KhachHangId,
                    SanPhamId = id,
                    NgayTao = DateTime.Now
                });
                isLiked = true;
            }

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                isLiked = isLiked
            });
        }
    }
}
