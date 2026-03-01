using KitchenHome_WsiteBanHang.Controllers;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Areas.Thu_Ngan.Controllers
{
    [Area("Thu_Ngan")]
    public class DonHangController : BaseController
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _db;

        public DonHangController(DbConnect_KitchenHome_WsiteBanHang db)
            : base(db)
        {
            _db = db;
        }

        // ==============================
        // 1️⃣ Danh sách đơn hàng
        // ==============================
        public IActionResult Index(string searchString, string statusFilter, int page = 1)
        {
            // Kiểm tra quyền
            if (HttpContext.Session.GetString("ROLE") != "THU_NGAN")
                return RedirectToAction("Index", "Home", new { area = "" });

            int pageSize = 8;

            var query = _db.DonHangs
                .Include(x => x.KhachHang)
                .Include(x => x.TaiKhoan)
                .AsQueryable();

            // Tìm theo mã đơn hoặc tên khách
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(x => x.MaDonHang.Contains(searchString) ||
                                         x.KhachHang.HoTen.Contains(searchString));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(x => x.TrangThai == statusFilter);
            }

            query = query.OrderByDescending(x => x.NgayDat);

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = page < 1 ? 1 : (totalPages > 0 && page > totalPages ? totalPages : page);

            var orders = query.Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToList();

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = statusFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(orders);
        }

        // ==============================
        // 2️⃣ Chi tiết đơn hàng
        // ==============================
        public IActionResult Details(long id)
        {
            if (HttpContext.Session.GetString("ROLE") != "THU_NGAN")
                return RedirectToAction("Index", "Home", new { area = "" });

            var order = _db.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.ChiTietDonHangs)
                .Include(d => d.LichSuTrangThaiDonHangs)
                .FirstOrDefault(x => x.DonHangId == id);

            if (order == null)
                return NotFound();

            ViewBag.DiaChiGiao = _db.DonHang_DiaChiGiaos
                .FirstOrDefault(x => x.DonHangId == id);

            return View(order);
        }

        // ==============================
        // 3️⃣ Cập nhật trạng thái thanh toán
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(long id, string trangThai)
        {
            if (HttpContext.Session.GetString("ROLE") != "THU_NGAN")
                return Unauthorized();

            var order = _db.DonHangs.Find(id);

            if (order != null)
            {
                string oldStatus = order.TrangThai;

                order.TrangThai = trangThai;
                order.NgayCapNhat = DateTime.Now;

                var log = new LichSuTrangThaiDonHang
                {
                    DonHangId = id,
                    TrangThaiCu = oldStatus,
                    TrangThaiMoi = trangThai,
                    NguoiThucHienId = HttpContext.Session.GetInt32("USER_ID") ?? 0,
                    NgayTao = DateTime.Now,
                    GhiChu = "Thu ngân xác nhận thanh toán"
                };

                _db.LichSuTrangThaiDonHangs.Add(log);
                _db.SaveChanges();

                SetAlert("Cập nhật trạng thái thành công!", "success");
            }

            return RedirectToAction("Details", new { id });
        }
    }
}