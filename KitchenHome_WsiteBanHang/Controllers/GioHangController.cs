using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using KitchenHome_WsiteBanHang.Models.Class_phu;
using KitchenHome_WsiteBanHang.Services;
using KitchenHome_WsiteBanHang.Helpers;

namespace KitchenHome_WsiteBanHang.Controllers
{
    public class GioHangController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;
        private readonly CartService _cartService;

        public GioHangController(
            DbConnect_KitchenHome_WsiteBanHang context,
            CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // ================= HELPER =================
        private int? GetTaiKhoanId()
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
                    returnUrl = Url.Action("Index", "GioHang")
                });
        }

        // ================= THÊM VÀO GIỎ =================
        [HttpGet]
        public IActionResult Them(int variantId, int quantity = 1, string returnUrl = null)
        {
            int? taiKhoanId = GetTaiKhoanId();

            // GIỮ NGUYÊN NGHIỆP VỤ: bắt buộc đăng nhập
            if (!taiKhoanId.HasValue)
            {
                return RedirectToAction(
                    "Index",
                    "Home",
                    new
                    {
                        area = "Login_Wsite",
                        returnUrl = returnUrl
                    });
            }

            var maPhien = CartCookie.GetOrCreate(HttpContext);

            _cartService.AddToCart(
                taiKhoanId,
                null,
                maPhien,
                variantId,
                quantity);

            TempData["CartSuccess"] = "Đã thêm sản phẩm vào giỏ hàng thành công!";

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ================= HIỂN THỊ GIỎ HÀNG =================
        public IActionResult Index()
        {
            int? taiKhoanId = GetTaiKhoanId();
            var maPhien = CartCookie.GetOrCreate(HttpContext);

            // ✅ DÙNG SERVICE ĐÃ INJECT
            ViewBag.CartCount = _cartService.GetCartCount(taiKhoanId, maPhien);

            // GIỮ NGUYÊN NGHIỆP VỤ: chưa login → login
            if (!taiKhoanId.HasValue)
                return RedirectToLogin();

            var gioHangEntity = _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(ct => ct.BienThe)
                        .ThenInclude(bt => bt.SanPham)
                .FirstOrDefault(g =>
                    g.TaiKhoanId == taiKhoanId &&
                    g.TrangThai == "DANG_MUA");

            var model = new GioHangViewModel();

            if (gioHangEntity?.ChiTietGioHangs != null)
            {
                foreach (var item in gioHangEntity.ChiTietGioHangs)
                {
                    var bt = item.BienThe;
                    var sp = bt?.SanPham;
                    if (sp == null) continue;

                    string imgUrl =
                        !string.IsNullOrEmpty(sp.AnhDaiDien) &&
                        sp.AnhDaiDien.StartsWith("http")
                        ? sp.AnhDaiDien
                        : "/IMAGE/Img_SanPham/" + sp.AnhDaiDien;

                    decimal giaBan =
                        bt.GiaKhuyenMai.HasValue &&
                        bt.GiaKhuyenMai.Value > 0
                        ? bt.GiaKhuyenMai.Value
                        : bt.GiaBan;

                    model.Items.Add(new CartItemVM
                    {
                        BienTheID = item.BienTheId,
                        SanPhamID = sp.SanPhamId,
                        TenSanPham = sp.TenSanPham,
                        TenBienThe = !string.IsNullOrEmpty(bt.TenBienThe)
                            ? bt.TenBienThe
                            : bt.Sku,
                        SKU = bt.Sku,
                        AnhDaiDien = imgUrl,
                        SoLuong = item.SoLuong,
                        GiaGoc = bt.GiaBan,
                        DonGia = giaBan
                    });
                }
            }

            return View(model);
        }

        // ================= XOÁ SẢN PHẨM =================
        public IActionResult Xoa(int variantId)
        {
            int? taiKhoanId = GetTaiKhoanId();

            // GIỮ NGUYÊN NGHIỆP VỤ: chỉ xoá khi đã login
            if (taiKhoanId.HasValue)
            {
                var gioHang = _context.GioHangs.FirstOrDefault(g =>
                    g.TaiKhoanId == taiKhoanId &&
                    g.TrangThai == "DANG_MUA");

                if (gioHang != null)
                {
                    var item = _context.ChiTietGioHangs.FirstOrDefault(x =>
                        x.GioHangId == gioHang.GioHangId &&
                        x.BienTheId == variantId);

                    if (item != null)
                    {
                        _context.ChiTietGioHangs.Remove(item);
                        _context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Index");
        }
    }
}
