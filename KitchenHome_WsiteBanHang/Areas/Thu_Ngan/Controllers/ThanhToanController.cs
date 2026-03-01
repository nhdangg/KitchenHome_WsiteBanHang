using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using System.Text.Json;

namespace KitchenHome_WsiteBanHang.Areas.Thu_Ngan.Controllers
{
    [Area("Thu_Ngan")]
    public class ThanhToanController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public ThanhToanController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // =========================================================
        // 1️⃣ DANH SÁCH THANH TOÁN
        // =========================================================
        public async Task<IActionResult> Index(string searchString, string trangThai)
        {
            var query = _context.ThanhToans
                .Include(t => t.DonHang)
                .Include(t => t.PhuongThuc)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t =>
                    t.MaGiaoDich.Contains(searchString) ||
                    t.DonHang.MaDonHang.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(t => t.TrangThai == trangThai);
            }

            query = query.OrderByDescending(t => t.NgayTao);

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentStatus = trangThai;
            ViewBag.TrangThaiList = GetTrangThaiList();

            return View(await query.ToListAsync());
        }

        // =========================================================
        // 2️⃣ CHI TIẾT
        // =========================================================
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var thanhToan = await _context.ThanhToans
                .Include(t => t.DonHang)
                .Include(t => t.PhuongThuc)
                .Include(t => t.NhatKyCongThanhToans)
                .Include(t => t.HoanTiens)
                .FirstOrDefaultAsync(m => m.ThanhToanId == id);

            if (thanhToan == null) return NotFound();

            return View(thanhToan);
        }

        // =========================================================
        // 3️⃣ TẠO THANH TOÁN
        // =========================================================
        public IActionResult Create()
        {
            ViewData["DonHangId"] = new SelectList(
                _context.DonHangs.Where(d => d.TrangThai != "DA_HUY"),
                "DonHangId", "MaDonHang");

            ViewData["PhuongThucId"] = new SelectList(
                _context.PhuongThucThanhToans,
                "PhuongThucId", "TenPhuongThuc");

            ViewBag.TrangThaiList = GetTrangThaiList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThanhToan thanhToan)
        {
            ModelState.Remove("DonHang");
            ModelState.Remove("PhuongThuc");

            if (ModelState.IsValid)
            {
                try
                {
                    thanhToan.NgayTao = DateTime.Now;

                    if (thanhToan.TrangThai == "THANH_CONG")
                    {
                        thanhToan.NgayThanhToan = DateTime.Now;
                    }

                    _context.Add(thanhToan);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Tạo thanh toán thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.InnerException?.Message ?? ex.Message;
                }
            }

            ViewData["DonHangId"] = new SelectList(_context.DonHangs, "DonHangId", "MaDonHang", thanhToan.DonHangId);
            ViewData["PhuongThucId"] = new SelectList(_context.PhuongThucThanhToans, "PhuongThucId", "TenPhuongThuc", thanhToan.PhuongThucId);
            ViewBag.TrangThaiList = GetTrangThaiList();

            return View(thanhToan);
        }

        // =========================================================
        // 4️⃣ CẬP NHẬT
        // =========================================================
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var thanhToan = await _context.ThanhToans.FindAsync(id);
            if (thanhToan == null) return NotFound();

            if (thanhToan.TrangThai == "THANH_CONG")
            {
                TempData["Error"] = "Không thể sửa giao dịch đã thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TrangThaiList = GetTrangThaiList();
            return View(thanhToan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, ThanhToan model)
        {
            if (id != model.ThanhToanId) return NotFound();

            ModelState.Remove("DonHang");
            ModelState.Remove("PhuongThuc");

            if (ModelState.IsValid)
            {
                var thanhToan = await _context.ThanhToans
                    .Include(t => t.PhuongThuc)
                    .FirstOrDefaultAsync(x => x.ThanhToanId == id);

                if (thanhToan == null) return NotFound();

                string trangThaiCu = thanhToan.TrangThai;

                if (trangThaiCu == "THANH_CONG")
                {
                    TempData["Error"] = "Không thể sửa giao dịch đã thành công!";
                    return RedirectToAction(nameof(Index));
                }

                thanhToan.SoTien = model.SoTien;
                thanhToan.TrangThai = model.TrangThai;
                thanhToan.MaGiaoDich = model.MaGiaoDich;

                if (trangThaiCu != "THANH_CONG" && model.TrangThai == "THANH_CONG")
                {
                    thanhToan.NgayThanhToan = DateTime.Now;

                    var log = new NhatKyCongThanhToan
                    {
                        ThanhToanId = thanhToan.ThanhToanId,
                        CongThanhToan = thanhToan.PhuongThuc?.TenPhuongThuc ?? "ADMIN",
                        DuLieuNhan = JsonSerializer.Serialize(new
                        {
                            orderId = thanhToan.DonHangId,
                            amount = thanhToan.SoTien,
                            status = "SUCCESS"
                        }),
                        MaKetQua = "SUCCESS",
                        NgayTao = DateTime.Now
                    };

                    _context.NhatKyCongThanhToans.Add(log);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TrangThaiList = GetTrangThaiList();
            return View(model);
        }

       
        // =========================================================
        private List<SelectListItem> GetTrangThaiList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "CHO_THANH_TOAN", Text = "Chờ thanh toán" },
                new SelectListItem { Value = "THANH_CONG", Text = "Thành công" },
                new SelectListItem { Value = "THAT_BAI", Text = "Thất bại" },
                new SelectListItem { Value = "HUY", Text = "Đã hủy" }
            };
        }
    }
}