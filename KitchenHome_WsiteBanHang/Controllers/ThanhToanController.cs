using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using KitchenHome_WsiteBanHang.Models.Class_phu;
using KitchenHome_WsiteBanHang.Services;
using KitchenHome_WsiteBanHang.Helpers;

namespace KitchenHome_WsiteBanHang.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;
        private readonly CartService _cartService;

        public ThanhToanController(
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
                    returnUrl = Url.Action("Index", "ThanhToan")
                });
        }

        // ================= GET: THANH TOÁN =================
        [HttpGet]
        public IActionResult Index()
        {
            int? userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToLogin();

            var maPhien = CartCookie.GetOrCreate(HttpContext);
            var cartData = GetCartData(userId, maPhien);

            if (cartData.Items == null || !cartData.Items.Any())
                return RedirectToAction("Index", "GioHang");

            var danhSachPhuongThuc = _context.PhuongThucThanhToans
                .Where(p => p.DangHoatDong)
                .ToList();

            var model = new CheckoutViewModel
            {
                CartItems = cartData.Items,
                TongTienHang = cartData.TongTienHang,
                DanhSachPhuongThuc = danhSachPhuongThuc,

                // ⭐ SET MẶC ĐỊNH: COD
                PhuongThucThanhToanId = danhSachPhuongThuc
                .FirstOrDefault(p => p.MaPhuongThuc == "COD")
                ?.PhuongThucId ?? 0

            };

            var khachHang = _context.KhachHangs
                .FirstOrDefault(k => k.TaiKhoanId == userId);

            if (khachHang != null)
            {
                model.HoTen = khachHang.HoTen;
                model.SoDienThoai = khachHang.SoDienThoai;
                model.Email = khachHang.Email;

                var diaChi = _context.DiaChiKhachHangs
                    .FirstOrDefault(d =>
                        d.KhachHangId == khachHang.KhachHangId &&
                        d.MacDinh);

                if (diaChi != null)
                {
                    model.DiaChiCuThe = diaChi.DiaChiCuThe;
                    model.TinhThanh = diaChi.TinhThanh;
                    model.QuanHuyen = diaChi.QuanHuyen;
                    model.PhuongXa = diaChi.PhuongXa;
                }
            }

            return View(model);
        }


        // ================= POST: ĐẶT HÀNG =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DatHang(CheckoutViewModel model)
        {
            int? userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToLogin();

            var maPhien = CartCookie.GetOrCreate(HttpContext);
            var cartData = GetCartData(userId, maPhien);

            if (cartData.Items == null || !cartData.Items.Any())
                return RedirectToAction("Index", "GioHang");

            var khachHang = _context.KhachHangs
                .FirstOrDefault(k => k.TaiKhoanId == userId);

            decimal tienGiam = 0;
            MaGiamGia? coupon = null;

            if (!string.IsNullOrEmpty(model.MaGiamGia))
            {
                var kq = KiemTraMaGiamGia(
                    model.MaGiamGia,
                    cartData.TongTienHang,
                    khachHang?.KhachHangId
                );

                if (!kq.ok)
                {
                    ModelState.AddModelError("MaGiamGia", kq.msg);
                    model.CartItems = cartData.Items;
                    model.TongTienHang = cartData.TongTienHang;
                    model.DanhSachPhuongThuc = _context.PhuongThucThanhToans
                        .Where(p => p.DangHoatDong)
                        .ToList();
                    return View("Index", model);
                }

                coupon = kq.coupon;
                tienGiam = kq.tienGiam;
            }

            if (!ModelState.IsValid)
            {
                model.CartItems = cartData.Items;
                model.TongTienHang = cartData.TongTienHang;
                model.DanhSachPhuongThuc = _context.PhuongThucThanhToans
                    .Where(p => p.DangHoatDong)
                    .ToList();
                return View("Index", model);
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var khoMacDinh = _context.Khos.FirstOrDefault();
                if (khoMacDinh == null)
                    throw new Exception("Chưa cấu hình kho.");

                var donHang = new DonHang
                {
                    MaDonHang = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    TaiKhoanId = userId,
                    KhachHangId = khachHang?.KhachHangId,
                    KhoId = khoMacDinh.KhoId,
                    TrangThai = "CHO_XAC_NHAN",
                    KenhBan = "WEB",
                    TamTinh = cartData.TongTienHang,
                    PhiVanChuyen = model.PhiVanChuyen,
                    TongTien = cartData.TongTienHang - tienGiam + model.PhiVanChuyen,
                    GhiChu = model.GhiChu,
                    NgayDat = DateTime.Now
                };

                _context.DonHangs.Add(donHang);
                _context.SaveChanges();

                if (coupon != null && khachHang != null)
                {
                    _context.SuDungMaGiamGias.Add(new SuDungMaGiamGium
                    {
                        MaGiamGiaId = coupon.MaGiamGiaId,
                        KhachHangId = khachHang.KhachHangId,
                        DonHangId = donHang.DonHangId,
                        NgaySuDung = DateTime.Now
                    });

                    coupon.DaDung += 1;
                }

                _context.DonHang_DiaChiGiaos.Add(new DonHangDiaChiGiao
                {
                    DonHangId = donHang.DonHangId,
                    TenNguoiNhan = model.HoTen,
                    SdtnguoiNhan = model.SoDienThoai,
                    DiaChiCuThe = model.DiaChiCuThe,
                    TinhThanh = model.TinhThanh,
                    QuanHuyen = model.QuanHuyen,
                    PhuongXa = model.PhuongXa
                });

                foreach (var item in cartData.Items)
                {
                    _context.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        DonHangId = donHang.DonHangId,
                        BienTheId = item.BienTheID,
                        TenSanPhamLuu = item.TenSanPham,
                        Sku = item.SKU,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia
                    });
                }

                _context.ThanhToans.Add(new ThanhToan
                {
                    DonHangId = donHang.DonHangId,
                    PhuongThucId = model.PhuongThucThanhToanId,
                    SoTien = donHang.TongTien,
                    TrangThai = "CHO_THANH_TOAN",
                    NgayTao = DateTime.Now
                });

                var gioHang = _context.GioHangs.FirstOrDefault(g =>
                    g.TaiKhoanId == userId && g.TrangThai == "DANG_MUA");

                if (gioHang != null)
                {
                    gioHang.TrangThai = "DA_CHUYEN_THANH_DON";
                    gioHang.NgayCapNhat = DateTime.Now;
                }

                _context.SaveChanges();
                transaction.Commit();

                return RedirectToAction("Success", new { orderId = donHang.MaDonHang });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ModelState.AddModelError("", ex.Message);
                model.CartItems = cartData.Items;
                model.TongTienHang = cartData.TongTienHang;
                model.DanhSachPhuongThuc = _context.PhuongThucThanhToans
                    .Where(p => p.DangHoatDong)
                    .ToList();
                return View("Index", model);
            }
        }

        public IActionResult Success(string orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        // ================= GIỎ HÀNG =================
        private GioHangViewModel GetCartData(int? userId, string maPhien)
        {
            var model = new GioHangViewModel();

            var gioHang = _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(ct => ct.BienThe)
                        .ThenInclude(bt => bt.SanPham)
                .FirstOrDefault(g =>
                    g.TaiKhoanId == userId && g.TrangThai == "DANG_MUA");

            if (gioHang?.ChiTietGioHangs == null) return model;

            foreach (var item in gioHang.ChiTietGioHangs)
            {
                var bt = item.BienThe;
                var sp = bt?.SanPham;
                if (sp == null) continue;

                decimal giaBan = bt.GiaKhuyenMai > 0
                    ? bt.GiaKhuyenMai.Value
                    : bt.GiaBan;

                model.Items.Add(new CartItemVM
                {
                    BienTheID = bt.BienTheId,
                    SanPhamID = sp.SanPhamId,
                    TenSanPham = sp.TenSanPham,
                    TenBienThe = bt.TenBienThe ?? bt.Sku,
                    SKU = bt.Sku,
                    SoLuong = item.SoLuong,
                    DonGia = giaBan
                });
            }

            return model;
        }

        // ================= MÃ GIẢM GIÁ =================
        private (bool ok, string msg, MaGiamGia? coupon, decimal tienGiam)
            KiemTraMaGiamGia(string code, decimal tongTien, int? khachHangId)
        {
            var coupon = _context.MaGiamGias
                .Include(x => x.SuDungMaGiamGia)
                .FirstOrDefault(x =>
                    x.Code == code &&
                    x.DangHoatDong &&
                    x.BatDau <= DateTime.Now &&
                    x.KetThuc >= DateTime.Now);

            if (coupon == null)
                return (false, "Mã giảm giá không tồn tại hoặc đã hết hạn.", null, 0);

            if (tongTien < coupon.DonHangToiThieu)
                return (false, "Đơn hàng chưa đạt giá trị tối thiểu.", null, 0);

            if (coupon.GioiHanLuotDung.HasValue &&
                coupon.DaDung >= coupon.GioiHanLuotDung.Value)
                return (false, "Mã giảm giá đã hết lượt sử dụng.", null, 0);

            if (khachHangId.HasValue && coupon.GioiHanMoiKhach.HasValue)
            {
                int daDung = coupon.SuDungMaGiamGia
                    .Count(x => x.KhachHangId == khachHangId);

                if (daDung >= coupon.GioiHanMoiKhach.Value)
                    return (false, "Bạn đã sử dụng mã này.", null, 0);
            }

            decimal tienGiam;
            if (coupon.LoaiGiam == "PHAN_TRAM")
            {
                tienGiam = tongTien * coupon.GiaTriGiam / 100;
                if (coupon.GiamToiDa.HasValue)
                    tienGiam = Math.Min(tienGiam, coupon.GiamToiDa.Value);
            }
            else
            {
                tienGiam = coupon.GiaTriGiam;
            }

            return (true, "OK", coupon, tienGiam);
        }
    }
}
