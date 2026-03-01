using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models.Context;

namespace KitchenHome_WsiteBanHang.Controllers
{
    public class BaseController : Controller
    {
        protected readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public BaseController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 1. Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("USER_ID");

            if (userId == null)
            {
                filterContext.Result = new RedirectToActionResult(
                    "Index", "Home", new { area = "Login_Wsite" });
                return;
            }

            // 2. Kiểm tra quyền ADMIN
            bool isAdmin = _context.TaiKhoans
                .Include(x => x.VaiTros)
                .Any(user =>
                    user.TaiKhoanId == userId &&
                    user.VaiTros.Any(role => role.MaVaiTro == "ADMIN" || role.MaVaiTro == "QUAN_LY" || role.MaVaiTro == "THU_KHO" || role.MaVaiTro == "THU_NGAN")
                );

            if (!isAdmin)
            {
                filterContext.Result = new RedirectToActionResult(
                    "Index", "Home", new { area = "" });
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        protected void SetAlert(string message, string type)
        {
            TempData["AlertMessage"] = message;
            TempData["AlertType"] = type switch
            {
                "success" => "alert-success",
                "warning" => "alert-warning",
                "error" => "alert-danger",
                _ => "alert-info"
            };
        }
    }
}
