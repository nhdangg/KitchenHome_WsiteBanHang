using KitchenHome_WsiteBanHang.Helpers;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using KitchenHome_WsiteBanHang.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Controllers
{
    public class BoSuuTapController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;
        private readonly CartService _cartService;

        public BoSuuTapController(DbConnect_KitchenHome_WsiteBanHang context,CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: /BoSuuTap/Index
        public async Task<IActionResult> Index()
        {
          
            // 1. Lấy danh sách ID yêu thích để tô đỏ tim
            var taiKhoanId = HttpContext.Session.GetInt32("USER_ID");

            var maPhien = CartCookie.GetOrCreate(HttpContext);
            ViewBag.CartCount = _cartService.GetCartCount(taiKhoanId, maPhien);

            var wishlistIds = new List<int>();
            if (taiKhoanId != null)
            {
                var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.TaiKhoanId == taiKhoanId);
                if (khachHang != null)
                {
                    wishlistIds = await _context.SanPhamYeuThiches
                        .Where(x => x.KhachHangId == khachHang.KhachHangId)
                        .Select(x => x.SanPhamId).ToListAsync();
                }
            }
            ViewBag.WishlistIds = wishlistIds;

            // 2. Lấy các Danh Mục mà bạn muốn hiển thị (Ví dụ lấy danh mục cấp 1 hoặc top danh mục)
            // Ở đây tôi lấy tất cả danh mục đang hoạt động
            var categories = await _context.DanhMucs
                .Where(d => d.DangHoatDong)
                .OrderBy(d => d.ThuTu)
                .ToListAsync();

            // 3. Với mỗi danh mục, lấy 4 sản phẩm mới nhất (Explicit Loading)
            // Cách này tối ưu hơn việc Include toàn bộ sản phẩm
            foreach (var cat in categories)
            {
                _context.Entry(cat)
                    .Collection(c => c.SanPhams)
                    .Query()
                    .Include(p => p.BienTheSanPhams)
                    .Where(p => p.DangHoatDong)
                    .OrderByDescending(p => p.NgayTao)
                    .Take(4) // Chỉ lấy 4 sản phẩm mỗi danh mục để hiển thị
                    .Load();
            }

            // 4. Chỉ giữ lại những danh mục CÓ sản phẩm để hiển thị
            var model = categories.Where(c => c.SanPhams != null && c.SanPhams.Any()).ToList();

            return View(model);
        }

        // ... Giữ nguyên các hàm MoiVe, BanChay, GiamGiaSoc ...
    }
}