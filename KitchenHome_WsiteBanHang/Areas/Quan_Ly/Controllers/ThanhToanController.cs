using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;

namespace KitchenHome_WsiteBanHang.Areas.Quan_Ly.Controllers
{
    [Area("Quan_Ly")]
    public class ThanhToanController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public ThanhToanController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // ... (Các hàm Index, Details giữ nguyên) ...
        public async Task<IActionResult> Index(string searchString, string trangThai)
        {
            var query = _context.ThanhToans
                .Include(t => t.DonHang)
                .Include(t => t.PhuongThuc)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.MaGiaoDich.Contains(searchString) || t.DonHang.MaDonHang.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(t => t.TrangThai == trangThai);
            }

            query = query.OrderByDescending(t => t.NgayTao);

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentStatus = trangThai;

            return View(await query.ToListAsync());
        }

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

       

        private bool ThanhToanExists(long id)
        {
            return _context.ThanhToans.Any(e => e.ThanhToanId == id);
        }

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