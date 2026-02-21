using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Admin")]
public class HinhAnhSanPhamController : Controller
{
    private readonly DbConnect_KitchenHome_WsiteBanHang _context;

    public HinhAnhSanPhamController(DbConnect_KitchenHome_WsiteBanHang context)
    {
        _context = context;
    }

    public IActionResult Index(int bienTheId)
    {
        var bienThe = _context.BienTheSanPhams
            .Include(x => x.SanPham)
            .Include(x => x.HinhAnhSanPhams)
            .FirstOrDefault(x => x.BienTheId == bienTheId);

        if (bienThe == null)
            return NotFound();

        ViewBag.BienThe = bienThe;

        return View(bienThe.HinhAnhSanPhams.ToList());
    }
}
