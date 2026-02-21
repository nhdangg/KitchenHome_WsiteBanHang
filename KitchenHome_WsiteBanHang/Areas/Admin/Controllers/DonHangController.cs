using KitchenHome_WsiteBanHang.Controllers;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Area("Admin")]
public class DonHangController : BaseController
{
    private readonly DbConnect_KitchenHome_WsiteBanHang _db;

    public DonHangController(DbConnect_KitchenHome_WsiteBanHang db)
        : base(db)   // ✅ ĐÚNG
    {
        _db = db;
    }

    public IActionResult Index(string searchString, string statusFilter, int page = 1)
    {
        int pageSize = 8;

        // Lưu ý quan trọng: Phải Include KhachHang để lấy HoTen
        var query = _db.DonHangs
            .Include(x => x.KhachHang)
            .Include(x => x.TaiKhoan)
            .AsQueryable();

        // Tìm kiếm theo Mã đơn HOẶC Tên khách hàng
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

        var orders = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.CurrentSearch = searchString;
        ViewBag.CurrentStatus = statusFilter;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(orders);
    }
    public IActionResult Details(long id)
    {
        var order = _db.DonHangs
            .Include(d => d.ChiTietDonHangs)
            .Include(d => d.LichSuTrangThaiDonHangs)
            .FirstOrDefault(x => x.DonHangId == id);

        if (order == null)
            return NotFound();

        ViewBag.DiaChiGiao = _db.DonHang_DiaChiGiaos
            .FirstOrDefault(x => x.DonHangId == id);

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateStatus(long id, string trangThai)
    {
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
                GhiChu = "Admin cập nhật trạng thái"
            };

            _db.LichSuTrangThaiDonHangs.Add(log);
            _db.SaveChanges();

            SetAlert("Cập nhật trạng thái thành công!", "success");
        }

        return RedirectToAction("Details", new { id });
    }
}
