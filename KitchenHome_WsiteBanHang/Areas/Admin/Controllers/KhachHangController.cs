using KitchenHome_WsiteBanHang.Controllers;
using KitchenHome_WsiteBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;

namespace WsiteBanHang_GiaDung.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhachHangController : BaseController
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public KhachHangController(DbConnect_KitchenHome_WsiteBanHang context): base(context)
        {
            _context = context;
        }

        // GET: Admin/KhachHang
        public IActionResult Index(string searchString, int page = 1)
        {
            // 1. Cấu hình số bản ghi trên mỗi trang
            int pageSize = 8;

            // 2. Truy vấn ban đầu (Giữ nguyên Include và AsQueryable)
            var khachHangs = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .AsQueryable();

            // 3. Logic tìm kiếm (Giữ nguyên)
            if (!string.IsNullOrEmpty(searchString))
            {
                khachHangs = khachHangs.Where(k =>
                    k.HoTen.Contains(searchString) ||
                    k.SoDienThoai.Contains(searchString) ||
                    k.MaKhachHang.Contains(searchString));
            }

            // 4. Tính toán phân trang
            int totalItems = khachHangs.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Đảm bảo số trang luôn hợp lệ
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // 5. Thực hiện phân trang và sắp xếp
            var result = khachHangs
                .OrderByDescending(k => k.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 6. Gửi thông tin phân trang ra View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentSearch = searchString; // Để giữ từ khóa khi chuyển trang

            return View(result);
        }
        // GET: Admin/KhachHang/Details/5
        public IActionResult Details(int id)
        {
            var khachHang = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .Include(k => k.DiaChiKhachHangs)
                .FirstOrDefault(k => k.KhachHangId == id);

            if (khachHang == null)
                return NotFound();

            return View(khachHang);
        }

        // GET: Admin/KhachHang/Edit/5
        public IActionResult Edit(int id)
        {
            var kh = _context.KhachHangs.Find(id);
            if (kh == null)
                return NotFound();

            return View(kh);
        }

        // POST: Admin/KhachHang/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(KhachHang model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var khDB = _context.KhachHangs.Find(model.KhachHangId);
            if (khDB == null)
                return NotFound();

            // Admin chỉ sửa thông tin cơ bản
            khDB.HoTen = model.HoTen;
            khDB.SoDienThoai = model.SoDienThoai;
            khDB.Email = model.Email;
            khDB.GioiTinh = model.GioiTinh;
            khDB.NgaySinh = model.NgaySinh;
            khDB.GhiChu = model.GhiChu;
            khDB.DangHoatDong = model.DangHoatDong;

            _context.SaveChanges();

            SetAlert("Cập nhật thông tin khách hàng thành công!", "success");
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/KhachHang/Delete/5
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var kh = _context.KhachHangs.Find(id);
            if (kh == null)
                return RedirectToAction(nameof(Index));

            // 1. Nếu đã có đơn hàng -> không cho xóa
            bool hasOrder = _context.DonHangs.Any(d => d.KhachHangId == id);
            if (hasOrder)
            {
                SetAlert("Không thể xóa: Khách hàng này đã có lịch sử mua hàng!", "error");
                return RedirectToAction(nameof(Index));
            }

            // 2. Xóa địa chỉ trước
            var diaChis = _context.DiaChiKhachHangs
                .Where(d => d.KhachHangId == id)
                .ToList();

            _context.DiaChiKhachHangs.RemoveRange(diaChis);
            _context.KhachHangs.Remove(kh);
            _context.SaveChanges();

            SetAlert("Đã xóa hồ sơ khách hàng!", "success");
            return RedirectToAction(nameof(Index));
        }
    }
}
