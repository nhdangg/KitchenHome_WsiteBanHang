using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace WsiteBanHang_GiaDung.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DiaChiKhachHangController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public DiaChiKhachHangController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // 1. GET: Tạo địa chỉ mới
        public IActionResult Create(int khachHangId)
        {
            var khachHang = _context.KhachHangs.Find(khachHangId);
            if (khachHang == null) return NotFound();

            var model = new DiaChiKhachHang { KhachHangId = khachHangId };
            ViewBag.TenKhachHang = khachHang.HoTen;
            return View(model);
        }

        // 2. POST: Lưu địa chỉ mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DiaChiKhachHang model)
        {
            // [FIX LỖI QUAN TRỌNG]: Bỏ qua validate object KhachHang vì ta chỉ cần KhachHangId
            ModelState.Remove("KhachHang");

            if (ModelState.IsValid)
            {
                try
                {
                    model.NgayTao = DateTime.Now;

                    // Xử lý logic Mặc định
                    if (model.MacDinh)
                    {
                        ResetMacDinh(model.KhachHangId);
                    }
                    else if (!_context.DiaChiKhachHangs.Any(d => d.KhachHangId == model.KhachHangId))
                    {
                        // Nếu chưa có địa chỉ nào -> cái đầu tiên auto mặc định
                        model.MacDinh = true;
                    }

                    _context.Add(model);
                    _context.SaveChanges();

                    TempData["Success"] = "Thêm địa chỉ thành công!";
                    return RedirectToAction("Details", "KhachHang", new { id = model.KhachHangId });
                }
                catch (Exception ex)
                {
                    // [BẪY LỖI]: Ghi lỗi ra để hiển thị trên View
                    ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        ModelState.AddModelError("", "Chi tiết: " + ex.InnerException.Message);
                    }
                }
            }
            else
            {
                // In ra các lỗi validation để debug nếu cần
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
            }

            // Nếu lỗi, load lại tên khách hàng để hiển thị lại View
            var kh = _context.KhachHangs.Find(model.KhachHangId);
            ViewBag.TenKhachHang = kh != null ? kh.HoTen : "---";
            return View(model);
        }

        // 3. GET: Sửa địa chỉ
        public IActionResult Edit(int id)
        {
            var diaChi = _context.DiaChiKhachHangs
                .Include(d => d.KhachHang)
                .FirstOrDefault(d => d.DiaChiId == id);

            if (diaChi == null) return NotFound();

            ViewBag.TenKhachHang = diaChi.KhachHang.HoTen;
            return View(diaChi);
        }

        // 4. POST: Lưu sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DiaChiKhachHang model)
        {
            // [FIX LỖI QUAN TRỌNG]
            ModelState.Remove("KhachHang");

            if (ModelState.IsValid)
            {
                try
                {
                    var dbDiaChi = _context.DiaChiKhachHangs.Find(model.DiaChiId);
                    if (dbDiaChi == null) return NotFound();

                    // Nếu set mặc định -> reset các cái khác
                    if (model.MacDinh && !dbDiaChi.MacDinh)
                    {
                        ResetMacDinh(model.KhachHangId);
                    }

                    dbDiaChi.TenNguoiNhan = model.TenNguoiNhan;
                    dbDiaChi.SdtnguoiNhan = model.SdtnguoiNhan;
                    dbDiaChi.DiaChiCuThe = model.DiaChiCuThe;
                    dbDiaChi.PhuongXa = model.PhuongXa;
                    dbDiaChi.QuanHuyen = model.QuanHuyen;
                    dbDiaChi.TinhThanh = model.TinhThanh;
                    dbDiaChi.MacDinh = model.MacDinh;

                    _context.SaveChanges();

                    TempData["Success"] = "Cập nhật địa chỉ thành công!";
                    return RedirectToAction("Details", "KhachHang", new { id = model.KhachHangId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
                }
            }

            // Load lại viewbag nếu lỗi
            var kh = _context.KhachHangs.Find(model.KhachHangId);
            ViewBag.TenKhachHang = kh != null ? kh.HoTen : "---";
            return View(model);
        }

        // 5. POST: Xóa địa chỉ
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var diaChi = _context.DiaChiKhachHangs.Find(id);
                if (diaChi != null)
                {
                    int khId = diaChi.KhachHangId;
                    _context.DiaChiKhachHangs.Remove(diaChi);
                    _context.SaveChanges();

                    TempData["Success"] = "Đã xóa địa chỉ.";
                    return RedirectToAction("Details", "KhachHang", new { id = khId });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi xóa: " + ex.Message;
                // Nếu xóa lỗi cần redirect về đâu đó, tạm thời về Index KhachHang hoặc trang cũ
                // Ở đây ta chấp nhận redirect về trang Details dù xóa thất bại để hiện thông báo
                var dc = _context.DiaChiKhachHangs.AsNoTracking().FirstOrDefault(x => x.DiaChiId == id);
                if (dc != null) return RedirectToAction("Details", "KhachHang", new { id = dc.KhachHangId });
            }
            return NotFound();
        }

        private void ResetMacDinh(int khachHangId)
        {
            var list = _context.DiaChiKhachHangs.Where(d => d.KhachHangId == khachHangId).ToList();
            foreach (var item in list)
            {
                item.MacDinh = false;
            }
            // Không SaveChanges ở đây để gộp vào transaction của hàm chính
        }
    }
}