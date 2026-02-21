using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Class_phu;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net; // Thư viện gửi mail
using System.Net.Mail; // Thư viện gửi mail

namespace WsiteBanHang_KitchenHome.Controllers
{
    public class PageController : Controller
    {
        // 1. Khai báo DbContext (Sử dụng DI)
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        // 2. Tiêm DbContext qua Constructor
        public PageController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // Hàm lấy User ID từ Session (.NET Core)
        private int? GetTaiKhoanId()
        {
            // Session trong Core trả về string hoặc byte[]
            var userIdStr = HttpContext.Session.GetString("USER_ID");
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            return null;
        }

        // GET: /Page/Show/{slug}
        [HttpGet]
        public async Task<IActionResult> Show(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return RedirectToAction("Index", "Home");
            }

            // 3. Xử lý Cookie & Session
            // Lưu ý: Bạn cần cập nhật Helper CartCookie để nhận tham số là HttpContext thay vì Request, Response riêng lẻ
            // Ví dụ: var maPhien = CartCookie.GetOrCreate(HttpContext);

            // Logic tạm thời lấy Cookie thủ công nếu chưa sửa Helper:
            var maPhien = HttpContext.Request.Cookies["CartSession"];
            if (string.IsNullOrEmpty(maPhien))
            {
                maPhien = Guid.NewGuid().ToString();
                CookieOptions option = new CookieOptions { Expires = DateTime.Now.AddDays(30) };
                HttpContext.Response.Cookies.Append("CartSession", maPhien, option);
            }

            var taiKhoanId = GetTaiKhoanId();

            //// 4. Gọi Service (Cần sửa CartService để nhận DbContext đã inject hoặc truyền _context vào)
            //var cartService = new CartService(_context);
            //ViewBag.CartCount = cartService.GetCartCount(taiKhoanId, maPhien);

            // 5. Truy vấn Database (Dùng Async)
            var page = await _context.TrangNoiDungs
                .FirstOrDefaultAsync(x => x.MaTrang == slug && x.DangHienThi == true);

            // Nếu không tìm thấy hoặc bài viết bị ẩn -> Về trang chủ
            if (page == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(page);
        }

        // Không cần override Dispose trong .NET Core
    }
}