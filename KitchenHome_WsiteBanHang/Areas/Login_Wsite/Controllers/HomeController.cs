using KitchenHome_WsiteBanHang.Areas.Login_Wsite.Models;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace KitchenHome_WsiteBanHang.Areas.Login_Wsite.Controllers
{
    [Area("Login_Wsite")]
    public class HomeController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public HomeController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // ================== MD5 THUẦN (THEO DB CŨ) ==================
        private string GetMD5(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // ================== GET: LOGIN ==================
        [HttpGet]
        public IActionResult Index(string returnUrl = null)
        {
            // Đã login thì không vào lại trang login
            if (HttpContext.Session.GetInt32("USER_ID") != null)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Tránh loop login
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                returnUrl.Contains("/Login_Wsite", StringComparison.OrdinalIgnoreCase))
            {
                returnUrl = null;
            }

            return View(new LoginVM
            {
                ReturnUrl = returnUrl
            });
        }

        // ================== POST: LOGIN ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(LoginVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var u = _context.TaiKhoans
                .Include(x => x.VaiTros)
                .FirstOrDefault(x =>
                    x.TenDangNhap == vm.TenDangNhap &&
                    x.DangHoatDong == true
                );

            if (u == null)
            {
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc tài khoản bị khóa.");
                return View(vm);
            }

            // ================== KIỂM TRA MẬT KHẨU ==================
            bool loginOK = false;

            // ================= 1️⃣ Nếu đã là BCrypt =================
            if (!string.IsNullOrEmpty(u.MatKhauHash) && u.MatKhauHash.StartsWith("$2"))
            {
                loginOK = BCrypt.Net.BCrypt.Verify(vm.MatKhau, u.MatKhauHash);
            }
            else
            {
                // ================= 2️⃣ Nếu mật khẩu lưu THUẦN (plain text) =================
                if (!string.IsNullOrEmpty(u.MatKhauHash) && u.MatKhauHash == vm.MatKhau)
                {
                    loginOK = true;

                    // 🔁 Nâng cấp ngay sang BCrypt
                    u.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(vm.MatKhau);
                    u.MuoiHash = null;
                    _context.SaveChanges();
                }
                else
                {
                    // ================= 3️⃣ Nếu là MD5 cũ =================
                    var md5 = GetMD5(vm.MatKhau);

                    if (!string.IsNullOrEmpty(u.MatKhauHash) &&
                        u.MatKhauHash.Equals(md5, StringComparison.OrdinalIgnoreCase))
                    {
                        loginOK = true;

                        // 🔁 Nâng cấp ngay sang BCrypt
                        u.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(vm.MatKhau);
                        u.MuoiHash = null;
                        _context.SaveChanges();
                    }
                }
            }

            if (!loginOK)
            {
                ModelState.AddModelError("", "Sai mật khẩu.");
                return View(vm);
            }

            // ================== LOGIN OK → SESSION ==================
            HttpContext.Session.SetInt32("USER_ID", u.TaiKhoanId);
            HttpContext.Session.SetString("USERNAME", u.TenDangNhap);

            var role = u.VaiTros.FirstOrDefault();
            if (role != null)
            {
                HttpContext.Session.SetString("ROLE", role.MaVaiTro);
            }

            u.LanDangNhapCuoi = DateTime.Now;
            _context.SaveChanges();

            // ================== ĐIỀU HƯỚNG ==================

            // 1️⃣ ADMIN / QUẢN LÝ
            if (u.VaiTros.Any(r => r.MaVaiTro == "ADMIN"))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            if(u.VaiTros.Any(r => r.MaVaiTro == "QUAN_LY")){
                return RedirectToAction("Index", "Home", new { area = "Quan_Ly" });
            }
            if (u.VaiTros.Any(r => r.MaVaiTro == "THU_NGAN"))
            {
                return RedirectToAction("Index", "Home", new { area = "Thu_Ngan" });
            }
            // 2️⃣ THỦ KHO
            if (u.VaiTros.Any(r => r.MaVaiTro == "THU_KHO"))
            {
                return RedirectToAction("Index", "Home", new { area = "Thu_Kho" });
            }

            // 3️⃣ USER → quay lại trang cũ
            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl)
                && Url.IsLocalUrl(vm.ReturnUrl)
                && !vm.ReturnUrl.Contains("/Login_Wsite", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(vm.ReturnUrl);
            }

            // 4️⃣ Mặc định
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // ================== TAO TAI KHOAN (VIEW KHACH) ==================
        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyTKVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Kiểm tra trùng username
            if (await _context.TaiKhoans
                .AnyAsync(x => x.TenDangNhap == vm.TenDangNhap))
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
                return View(vm);
            }

            // Bắt đầu transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ===== 1️⃣ TẠO TÀI KHOẢN =====
                string hash = BCrypt.Net.BCrypt.HashPassword(vm.MatKhau);

                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = vm.TenDangNhap,
                    MatKhauHash = hash,
                    Email = vm.Email,
                    SoDienThoai = vm.SoDienThoai,
                    DangHoatDong = true,
                    EmailDaXacMinh = false,
                    DienThoaiDaXacMinh = false,
                    NgayTao = DateTime.Now
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync(); // Lưu để lấy TaiKhoanId


                // ===== 2️⃣ GÁN ROLE KHÁCH HÀNG =====
                var roleKhach = await _context.VaiTros
                    .FirstOrDefaultAsync(r => r.MaVaiTro == "KHACH_HANG");

                if (roleKhach == null)
                {
                    ModelState.AddModelError("", "Chưa có role KHACH_HANG trong hệ thống");
                    return View(vm);
                }

                taiKhoan.VaiTros = new List<VaiTro> { roleKhach };
                await _context.SaveChangesAsync();


                // ===== 3️⃣ TẠO KHÁCH HÀNG =====
                var khachHang = new KhachHang
                {
                    TaiKhoanId = taiKhoan.TaiKhoanId,
                    MaKhachHang = "KH" + taiKhoan.TaiKhoanId.ToString("D5"),
                    HoTen = string.IsNullOrEmpty(vm.HoTen) ? vm.TenDangNhap : vm.HoTen,
                    SoDienThoai = vm.SoDienThoai,
                    Email = vm.Email,
                    GioiTinh = null,
                    NgaySinh = null,
                    GhiChu = null,
                    DangHoatDong = true,
                    NgayTao = DateTime.Now
                };

                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();


                // Commit nếu mọi thứ OK
                await transaction.CommitAsync();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Có lỗi xảy ra khi đăng ký.");
                return View(vm);
            }
        }


        // ================== LOGOUT ==================
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
