using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Controllers
{
    public class DonHangController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public DonHangController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        private int? UserId => HttpContext.Session.GetInt32("USER_ID");

        public async Task<IActionResult> Index()
        {
            if (!UserId.HasValue)
            {
                return RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "Login_Wsite", returnUrl = "/DonHang" }
                );
            }

            var donHangs = await _context.DonHangs
                .Where(x => x.TaiKhoanId == UserId)
                .OrderByDescending(x => x.NgayDat)
                .ToListAsync();

            return View(donHangs);
        }

        public async Task<IActionResult> ChiTiet(long id)
        {
            if (!UserId.HasValue)
            {
                return RedirectToAction("Index", "Home", new { area = "Login_Wsite", returnUrl = "/DonHang" });
            }

            var dh = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                    .ThenInclude(ct => ct.BienThe)       // <--- THÊM DÒNG NÀY: Để lấy Biến thể
                        .ThenInclude(bt => bt.SanPham)   // <--- THÊM DÒNG NÀY: Để lấy Sản phẩm (chứa Ảnh)
                .Include(x => x.SuDungMaGiamGium)
                    .ThenInclude(x => x.MaGiamGia)
                .Include(x => x.ThanhToans)
                    .ThenInclude(x => x.PhuongThuc)
                .Include(x => x.DonHangDiaChiGiao)
                .Include(x => x.LichSuTrangThaiDonHangs)
                .FirstOrDefaultAsync(x => x.DonHangId == id && x.TaiKhoanId == UserId);

            if (dh == null) return NotFound();

            return View(dh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDon(long id, string? lyDoHuy)
        {
            if (!UserId.HasValue)
                return Json(new { success = false, msg = "Phiên đăng nhập đã hết hạn." });

            var donHang = await _context.DonHangs
                .FirstOrDefaultAsync(x =>
                    x.DonHangId == id &&
                    x.TaiKhoanId == UserId);

            if (donHang == null)
                return Json(new { success = false, msg = "Không tìm thấy đơn hàng." });

            // ❗ CHỈ CHO HỦY KHI CHỜ XÁC NHẬN
            if (donHang.TrangThai != "CHO_XAC_NHAN")
            {
                return Json(new
                {
                    success = false,
                    msg = "Đơn hàng đã được xác nhận, không thể huỷ."
                });
            }

            // Cập nhật trạng thái
            donHang.TrangThai = "HUY";
            donHang.NgayCapNhat = DateTime.Now;

            // Ghi lịch sử trạng thái
            _context.LichSuTrangThaiDonHangs.Add(
                new LichSuTrangThaiDonHang
                {
                    DonHangId = donHang.DonHangId,
                    TrangThaiCu = "CHO_XAC_NHAN",
                    TrangThaiMoi = "HUY",
                    GhiChu = string.IsNullOrWhiteSpace(lyDoHuy)
                                ? "Khách hàng huỷ đơn khi chưa xác nhận"
                                : $"Khách hàng huỷ: {lyDoHuy}",
                    NguoiThucHienId = UserId,
                    NgayTao = DateTime.Now
                });

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                msg = "Huỷ đơn hàng thành công."
            });
        }

    }
}
