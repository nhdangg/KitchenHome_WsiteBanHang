using KitchenHome_WsiteBanHang.Controllers;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
namespace WsiteBanHang_GiaDung.Areas.Quan_Ly.Controllers
{
    [Area("Quan_Ly")]
    public class MaGiamGiaController : BaseController
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public MaGiamGiaController(DbConnect_KitchenHome_WsiteBanHang context): base(context)
        {
            _context = context;
        }

        // =======================
        // DANH SÁCH MÃ GIẢM GIÁ
        // =======================
        public IActionResult Index()
        {
            var list = _context.MaGiamGias
                .OrderByDescending(x => x.MaGiamGiaId)
                .ToList();

            return View(list);
        }

        // =======================
        // TẠO MỚI
        // =======================
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MaGiamGia model)
        {
            if (ModelState.IsValid)
            {
                // Check trùng code
                if (_context.MaGiamGias.Any(x => x.Code == model.Code))
                {
                    ModelState.AddModelError("Code", "Mã giảm giá này đã tồn tại!");
                    return View(model);
                }

                // Validate ngày
                if (model.BatDau >= model.KetThuc)
                {
                    ModelState.AddModelError("KetThuc", "Ngày kết thúc phải sau ngày bắt đầu.");
                    return View(model);
                }

                model.Code = model.Code.ToUpper().Trim();
                model.DaDung = 0;
                model.DangHoatDong = true;

                _context.MaGiamGias.Add(model);
                _context.SaveChanges();

                SetAlert("Tạo mã giảm giá thành công", "success");
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // =======================
        // SỬA
        // =======================
        public IActionResult Edit(int id)
        {
            var item = _context.MaGiamGias.Find(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MaGiamGia model)
        {
            if (ModelState.IsValid)
            {
                if (model.BatDau >= model.KetThuc)
                {
                    ModelState.AddModelError("KetThuc", "Ngày kết thúc phải sau ngày bắt đầu.");
                    return View(model);
                }

                var dbItem = _context.MaGiamGias.Find(model.MaGiamGiaId);
                if (dbItem == null) return NotFound();

                // Không sửa Code
                dbItem.TenMa = model.TenMa;
                dbItem.LoaiGiam = model.LoaiGiam;
                dbItem.GiaTriGiam = model.GiaTriGiam;
                dbItem.GiamToiDa = model.GiamToiDa;
                dbItem.DonHangToiThieu = model.DonHangToiThieu;
                dbItem.BatDau = model.BatDau;
                dbItem.KetThuc = model.KetThuc;
                dbItem.GioiHanLuotDung = model.GioiHanLuotDung;
                dbItem.GioiHanMoiKhach = model.GioiHanMoiKhach;
                dbItem.DangHoatDong = model.DangHoatDong;

                _context.SaveChanges();
                SetAlert("Cập nhật mã thành công", "success");

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // =======================
        // XÓA / NGỪNG HOẠT ĐỘNG
        // =======================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var item = _context.MaGiamGias.Find(id);
            if (item != null)
            {
                bool daSuDung = _context.SuDungMaGiamGias
                    .Any(x => x.MaGiamGiaId == id);

                if (daSuDung)
                {
                    item.DangHoatDong = false;
                    _context.SaveChanges();
                    SetAlert("Mã đã có người dùng, hệ thống chuyển sang 'Ngừng hoạt động'.", "warning");
                }
                else
                {
                    _context.MaGiamGias.Remove(item);
                    _context.SaveChanges();
                    SetAlert("Đã xóa mã giảm giá.", "success");
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // =======================
        // CHI TIẾT + LỊCH SỬ DÙNG
        // =======================
        public IActionResult Details(int id)
        {
            var item = _context.MaGiamGias.Find(id);
            if (item == null) return NotFound();

            var lichSuDung = _context.SuDungMaGiamGias
                .Where(x => x.MaGiamGiaId == id)
                .OrderByDescending(x => x.NgaySuDung)
                .ToList();

            ViewBag.LichSuDung = lichSuDung;

            return View(item);
        }
    }
}
