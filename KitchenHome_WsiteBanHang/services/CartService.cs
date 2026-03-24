using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models.Context;
using KitchenHome_WsiteBanHang.Models;

namespace KitchenHome_WsiteBanHang.Services
{
    public class CartService
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public CartService(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // ================= ĐẾM SỐ LƯỢNG GIỎ =================
        public int GetCartCount(int? taiKhoanId, string maPhien)
        {
            var q = _context.ChiTietGioHangs
                .Include(ct => ct.GioHang)
                .Where(ct => ct.GioHang.TrangThai == "DANG_MUA");

            if (taiKhoanId.HasValue)
            {
                q = q.Where(ct => ct.GioHang.TaiKhoanId == taiKhoanId.Value);
            }
            else if (!string.IsNullOrEmpty(maPhien))
            {
                q = q.Where(ct => ct.GioHang.MaPhien == maPhien);
            }
            else
            {
                return 0;
            }

            return q.Sum(x => (int?)x.SoLuong) ?? 0;
        }

        // ================= LẤY / TẠO GIỎ =================
        public GioHang GetOrCreateCart(int? taiKhoanId, int? khachHangId, string maPhien)
        {
            GioHang cart = null;

            // 1. Ưu tiên giỏ theo Tài Khoản
            if (taiKhoanId.HasValue)
            {
                cart = _context.GioHangs.FirstOrDefault(x =>
                    x.TaiKhoanId == taiKhoanId.Value &&
                    x.TrangThai == "DANG_MUA");

                if (cart != null) return cart;
            }

            // 2. Nếu chưa có, kiểm tra giỏ guest
            if (!string.IsNullOrEmpty(maPhien))
            {
                cart = _context.GioHangs.FirstOrDefault(x =>
                    x.MaPhien == maPhien &&
                    x.TrangThai == "DANG_MUA");

                // Nếu user đã login → gán lại giỏ guest cho user
                if (cart != null && taiKhoanId.HasValue)
                {
                    cart.TaiKhoanId = taiKhoanId;
                    cart.KhachHangId = khachHangId;
                    cart.MaPhien = null;
                    cart.NgayCapNhat = DateTime.Now;
                    _context.SaveChanges();
                    return cart;
                }

                if (cart != null) return cart;
            }

            // 3. Tạo mới
            cart = new GioHang
            {
                TaiKhoanId = taiKhoanId,
                KhachHangId = khachHangId,
                MaPhien = taiKhoanId.HasValue ? null : maPhien,
                TrangThai = "DANG_MUA",
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now
            };

            _context.GioHangs.Add(cart);
            _context.SaveChanges();

            return cart;
        }

        // ================= THÊM VÀO GIỎ =================
        public void AddToCart(
    int? taiKhoanId,
    int? khachHangId,
    string maPhien,
    int bienTheId,
    int qty = 1)
        {
            if (qty <= 0) qty = 1;

            if (!taiKhoanId.HasValue && string.IsNullOrEmpty(maPhien))
                throw new Exception("Không xác định được người dùng!");

            var cart = GetOrCreateCart(taiKhoanId, khachHangId, maPhien);

            // ================== 🔥 CHECK BIẾN THỂ ==================
            var bt = _context.BienTheSanPhams
                .FirstOrDefault(x =>
                    x.BienTheId == bienTheId &&
                    x.DangHoatDong);

            if (bt == null || bt.GiaBan <= 0)
                throw new Exception("Sản phẩm không hợp lệ!");

            // ================== 🔥 TỔNG TỒN KHO ==================
            var tonKhos = _context.TonKhos
                .Where(x => x.BienTheId == bienTheId)
                .ToList();

            int tongTon = tonKhos.Sum(x => x.SoLuongTon);

            if (tongTon <= 0)
                throw new Exception("Sản phẩm đã hết hàng!");

            // ================== 🔥 TỔNG ĐÃ CÓ TRONG TẤT CẢ GIỎ ==================
            int tongTrongTatCaGio = _context.ChiTietGioHangs
                .Where(x => x.BienTheId == bienTheId)
                .Sum(x => (int?)x.SoLuong) ?? 0;

            // ================== 🔥 SỐ LƯỢNG HIỆN TẠI TRONG GIỎ ==================
            var line = _context.ChiTietGioHangs.FirstOrDefault(x =>
                x.GioHangId == cart.GioHangId &&
                x.BienTheId == bienTheId);

            int soLuongHienTai = line?.SoLuong ?? 0;

            int tongSauThem = soLuongHienTai + qty;

            // ================== 🚨 CHECK CHẶN ==================
            if (tongTrongTatCaGio + qty > tongTon)
                throw new Exception($"Chỉ còn {tongTon - tongTrongTatCaGio} sản phẩm có thể bán!");

            if (tongSauThem > tongTon)
                throw new Exception($"Bạn chỉ có thể thêm tối đa {tongTon} sản phẩm!");

            // ================== 💰 GIÁ ==================
            decimal gia = bt.GiaBan;

            if (bt.GiaKhuyenMai.HasValue &&
                bt.GiaKhuyenMai.Value > 0 &&
                bt.GiaKhuyenMai.Value < bt.GiaBan)
            {
                gia = bt.GiaKhuyenMai.Value;
            }

            // ================== 🛒 THÊM / CỘNG ==================
            if (line != null)
            {
                line.SoLuong += qty;
                line.DonGiaTaiThoiDiem = gia;
            }
            else
            {
                _context.ChiTietGioHangs.Add(new ChiTietGioHang
                {
                    GioHangId = cart.GioHangId,
                    BienTheId = bienTheId,
                    SoLuong = qty,
                    DonGiaTaiThoiDiem = gia,
                    NgayTao = DateTime.Now
                });
            }

            cart.NgayCapNhat = DateTime.Now;

            _context.SaveChanges();
        }
    }
}
