using QL_KhachSan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_KhachSan.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private QL_KhachSanEntities db = new QL_KhachSanEntities(); // DbContext EF

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string taiKhoan, string matKhau)
        {
            // Tìm tài khoản trong DB
            var user = db.TaiKhoanNVs
                         .FirstOrDefault(u => u.TaiKhoan == taiKhoan && u.MatKhau == matKhau);

            if (user != null)
            {
                // Lưu session admin
                Session["Admin"] = user;
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        public ActionResult Logout()
        {
            Session["Admin"] = null;
            return RedirectToAction("Login");
        }
    }
}