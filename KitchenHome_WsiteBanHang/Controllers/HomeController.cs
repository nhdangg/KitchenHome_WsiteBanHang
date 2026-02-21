using KitchenHome_WsiteBanHang.Helpers;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Class_phu;
using KitchenHome_WsiteBanHang.Models.Context;
using KitchenHome_WsiteBanHang.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace WsiteBanHang_KitchenHome.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;
        private readonly IConfiguration _configuration;
        private readonly CartService _cartService; // ✅ FIX: Inject CartService

        public HomeController(
            ILogger<HomeController> logger,
            DbConnect_KitchenHome_WsiteBanHang context,
            IConfiguration configuration,
            CartService cartService) // ✅ FIX
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _cartService = cartService;
        }

        // ===== Helper lấy UserID =====
        private int? GetTaiKhoanId()
        {
            return HttpContext.Session.GetInt32("USER_ID");
        }

        // ==============================
        // TRANG CHỦ
        // ==============================
        public async Task<IActionResult> Index()
        {
            // --- Cookie giỏ hàng (GIỮ NGUYÊN NGHIỆP VỤ CŨ) ---
            var maPhien = CartCookie.GetOrCreate(HttpContext);

            if (string.IsNullOrEmpty(maPhien))
            {
                maPhien = Guid.NewGuid().ToString();
                CookieOptions option = new CookieOptions { Expires = DateTime.Now.AddDays(30) };
                HttpContext.Response.Cookies.Append("CartSession", maPhien, option);
            }

            var taiKhoanId = GetTaiKhoanId();

            // Cart count
            ViewBag.CartCount = _cartService.GetCartCount(taiKhoanId, maPhien);

            // --- Wishlist ---
            var wishlistIds = new List<int>();
            if (taiKhoanId != null)
            {
                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.TaiKhoanId == taiKhoanId);

                if (khachHang != null)
                {
                    wishlistIds = await _context.SanPhamYeuThiches
                        .Where(x => x.KhachHangId == khachHang.KhachHangId)
                        .Select(x => x.SanPhamId)
                        .ToListAsync();
                }
            }
            ViewBag.WishlistIds = wishlistIds;

            var model = new HomeView_TrangChu();

            // ===============================
            // 1. Banner (CŨ)
            // ===============================
            model.DanhSachBanner = await _context.Banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.ThuTu)
                .ToListAsync();

            // ===============================
            // 2. Danh mục nổi bật (MỚI)
            // ===============================
            model.DanhMucNoiBat = await _context.DanhMucs
                .Where(d => d.DangHoatDong)
                .OrderBy(d => d.ThuTu)
                .Take(6)
                .ToListAsync();

            // ===============================
            // 3. Thương hiệu đối tác (MỚI)
            // ===============================
            model.ThuongHieuDoiTac = await _context.ThuongHieus
                .Where(t => t.DangHoatDong)
                .Take(8)
                .ToListAsync();

            // ===============================
            // 4. Sản phẩm nổi bật (CŨ)
            // ===============================
            model.DanhSachSanPham = await _context.SanPhams
                .Include(sp => sp.BienTheSanPhams)
                .Where(sp => sp.HienThiTrangChu
                             && sp.DangHoatDong
                             && sp.BienTheSanPhams.Any(bt => bt.DangHoatDong && bt.GiaBan > 0))
                .OrderByDescending(sp => sp.NgayTao)
                .Take(4)
                .ToListAsync();

            // ===============================
            // 5. Sản phẩm giảm giá (CŨ)
            // ===============================
            var idsNoiBat = model.DanhSachSanPham.Select(x => x.SanPhamId).ToList();

            model.DanhSachGiamGia = await _context.SanPhams
                .Include(sp => sp.BienTheSanPhams)
                .Where(sp => sp.DangHoatDong
                             && !idsNoiBat.Contains(sp.SanPhamId)
                             && sp.BienTheSanPhams.Any(bt =>
                                    bt.DangHoatDong &&
                                    bt.GiaBan > 0 &&
                                    bt.GiaKhuyenMai != null &&
                                    bt.GiaKhuyenMai > 0 &&
                                    bt.GiaKhuyenMai < bt.GiaBan))
                .OrderByDescending(sp => sp.NgayTao)
                .Take(8)
                .ToListAsync();


            // ===============================
            // 7. Đánh giá tiêu biểu (MỚI)
            // ===============================
            model.DanhGiaTieuBieu = await _context.DanhGiaSanPhams
                .Include(d => d.KhachHang)
                .Include(d => d.SanPham)
                .Where(d => d.HienThi && d.SoSao == 5 && !string.IsNullOrEmpty(d.NoiDung))
                .OrderByDescending(d => d.NgayTao)
                .Take(3)
                .ToListAsync();

            // ===============================
            // 8. Rating (CŨ - GIỮ NGUYÊN)
            // ===============================
            var allIds = model.DanhSachSanPham.Select(x => x.SanPhamId)
                .Concat(model.DanhSachGiamGia.Select(x => x.SanPhamId))
                .Distinct()
                .ToList();

            if (allIds.Any())
            {
                var ratingRaw = await _context.DanhGiaSanPhams
                    .Where(dg => dg.HienThi && allIds.Contains(dg.SanPhamId))
                    .ToListAsync();

                model.Ratings = ratingRaw
                    .GroupBy(dg => dg.SanPhamId)
                    .ToDictionary(
                        g => g.Key,
                        g => new RatingInfo
                        {
                            AvgStar = g.Average(x => (double)x.SoSao),
                            ReviewCount = g.Count()
                        });
            }
            else
            {
                model.Ratings = new Dictionary<int, RatingInfo>();
            }

            // ===============================
            // 9. Sản phẩm bán chạy (MỚI)
            // ===============================
            var topBanChayIds = await _context.ChiTietDonHangs
                .Include(ct => ct.BienThe)
                .GroupBy(ct => ct.BienThe.SanPhamId)
                .Select(g => new
                {
                    SanPhamId = g.Key,
                    DaBan = g.Sum(x => x.SoLuong)
                })
                .OrderByDescending(x => x.DaBan)
                .Take(8)
                .Select(x => x.SanPhamId)
                .ToListAsync();

            if (topBanChayIds.Any())
            {
                model.SanPhamBanChay = await _context.SanPhams
                    .Include(sp => sp.BienTheSanPhams)
                    .Where(sp => topBanChayIds.Contains(sp.SanPhamId))
                    .ToListAsync();

                model.SanPhamBanChay = model.SanPhamBanChay
                    .OrderBy(sp => topBanChayIds.IndexOf(sp.SanPhamId))
                    .ToList();
            }
            else
            {
                model.SanPhamBanChay = await _context.SanPhams
                    .Include(sp => sp.BienTheSanPhams)
                    .Where(sp => sp.DangHoatDong)
                    .OrderBy(sp => sp.NgayTao)
                    .Take(8)
                    .ToListAsync();
            }

            return View(model);
        }


        // ==============================
        // CHI TIẾT SẢN PHẨM
        // ==============================
        public async Task<IActionResult> ChiTiet(int id)
        {
            var sp = await _context.SanPhams
                .Include(s => s.BienTheSanPhams.Where(bt => bt.DangHoatDong))
                    .ThenInclude(bt => bt.HinhAnhSanPhams)
                .Include(s => s.DanhMuc)
                .Include(s => s.ThuongHieu)
                .FirstOrDefaultAsync(s => s.SanPhamId == id && s.DangHoatDong);

            if (sp == null)
                return RedirectToAction("Index");

            var taiKhoanId = GetTaiKhoanId();
            var maPhien = HttpContext.Request.Cookies["CartSession"];

            // ✅ FIX
            ViewBag.CartCount = _cartService.GetCartCount(taiKhoanId, maPhien);

            ViewBag.SanPhamLienQuan = await _context.SanPhams
                .Where(s => s.DanhMucId == sp.DanhMucId
                         && s.SanPhamId != id
                         && s.DangHoatDong
                         && s.BienTheSanPhams.Any(bt => bt.DangHoatDong && bt.GiaBan > 0))
                .OrderByDescending(s => s.NgayTao)
                .Take(4)
                .ToListAsync();

            ViewBag.DanhSachDanhGia = await _context.DanhGiaSanPhams
                .Include(d => d.KhachHang)
                .Where(d => d.SanPhamId == id && d.HienThi)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            return View(sp);
        }


        /// <summary>
        /// Đánh giá sản phẩm
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiDanhGia(int SanPhamId, int SoSao, string NoiDung)
        {
            // 1. Lấy ID tài khoản từ Session
            var taiKhoanId = GetTaiKhoanId();

            if (taiKhoanId == null)
            {
                // Nếu chưa đăng nhập, trả về thông báo hoặc chuyển hướng
                return Json(new { success = false, message = "Vui lòng đăng nhập để gửi đánh giá!" });
            }

            // 2. Tìm KhachHangId tương ứng với TaiKhoanId (vì bảng DanhGia thường liên kết với KhachHang)
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.TaiKhoanId == taiKhoanId);

            if (khachHang == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng!" });
            }

            try
            {
                // 3. Tạo đối tượng đánh giá mới
                var danhGia = new DanhGiaSanPham
                {
                    SanPhamId = SanPhamId,
                    KhachHangId = khachHang.KhachHangId,
                    SoSao = (byte)SoSao, // Ép kiểu từ int sang byte ở đây
                    NoiDung = NoiDung,
                    NgayTao = DateTime.Now,
                    HienThi = true
                };

                _context.DanhGiaSanPhams.Add(danhGia);
                await _context.SaveChangesAsync();

                // 4. Quay lại trang chi tiết sản phẩm
                return RedirectToAction("ChiTiet", new { id = SanPhamId });
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi khi gửi đánh giá: " + ex.Message);
            }
        }
        /// <returns></returns>
        // ==============================
        // CONTACT
        // ==============================
        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(
            string hoTen,
            string soDienThoai,
            string email,
            string chuDe,
            string noiDung)
        {
            if (HttpContext.Session.GetString("USER_ID") == null)
            {
                return RedirectToAction("Index", "Home", new { area = "Login_Wsite" });
            }

            try
            {
                var host = _configuration["EmailSettings:Host"];
                var port = int.Parse(_configuration["EmailSettings:Port"]);
                var fromEmail = _configuration["EmailSettings:Email"];
                var password = _configuration["EmailSettings:Password"];

                string body = $@"
                    <h3>Liên hệ mới</h3>
                    <p><b>Họ tên:</b> {hoTen}</p>
                    <p><b>SĐT:</b> {soDienThoai}</p>
                    <p><b>Email:</b> {email}</p>
                    <p><b>Nội dung:</b><br/>{noiDung}</p>";

                using var smtp = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromEmail, password)
                };

                var message = new MailMessage(fromEmail, fromEmail)
                {
                    Subject = $"[LIÊN HỆ] {chuDe}",
                    Body = body,
                    IsBodyHtml = true
                };

                await smtp.SendMailAsync(message);
                ViewBag.Status = "success";
                ViewBag.Message = "Gửi thành công!";
            }
            catch
            {
                ViewBag.Status = "danger";
                ViewBag.Message = "Không gửi được email.";
            }

            return View();
        }


        // Thêm vào trong HomeController

        [HttpPost]
        public async Task<IActionResult> SubscribeNewsletter(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Vui lòng nhập địa chỉ email." });
            }

            try
            {
                // 1. Lấy cấu hình Email từ appsettings.json (Giống hàm Contact)
                var host = _configuration["EmailSettings:Host"];
                var port = int.Parse(_configuration["EmailSettings:Port"]);
                var fromEmail = _configuration["EmailSettings:Email"];
                var password = _configuration["EmailSettings:Password"];

                // 2. Nội dung email gửi về cho Admin (hoặc chính chủ shop)
                string body = $@"
            <h3>Thông báo: Có người đăng ký nhận Voucher 50k cho đơn hàng đầu tiên</h3>
            <p>Khách hàng với email <b>{email}</b> vừa đăng ký nhận bản tin khuyến mãi trên website.</p>
            <p><i>Thời gian: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}</i></p>";

                using var smtp = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromEmail, password)
                };

                var message = new MailMessage(fromEmail, fromEmail) // Gửi từ email hệ thống -> đến email hệ thống
                {
                    Subject = "[Newsletter] Khách hàng mới đăng ký: " + email,
                    Body = body,
                    IsBodyHtml = true
                };

                await smtp.SendMailAsync(message);

                // 3. Trả về kết quả thành công
                return Json(new { success = true, message = "Đăng ký thành công! Chúng tôi đã ghi nhận email của bạn." });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi mail: " + ex.Message });
            }
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
