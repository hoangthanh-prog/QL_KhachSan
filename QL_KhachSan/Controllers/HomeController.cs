using QL_KhachSan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_KhachSan.Controllers
{
    public class HomeController : Controller
    {
        QL_KhachSanEntities db = new QL_KhachSanEntities();

        public ActionResult Index()
        {
            var loaiPhongList = db.BienThePhongs
                                  .Include("HinhAnhs")
                                  .Include("LoaiPhong")
                                  .Include("Phongs")
                                  .ToList();

            var phongNoiBat = loaiPhongList
                                .OrderByDescending(x => x.GiaBan)
                                .Take(3)
                                .ToList();

            ViewBag.LoaiPhongList = loaiPhongList;
            ViewBag.PhongNoiBat = phongNoiBat;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TimPhong(DateTime? ngayDen, DateTime? ngayDi, int? maBienThe)
        {
            TempData["NgayDen"] = ngayDen;
            TempData["NgayDi"] = ngayDi;

            if (maBienThe.HasValue && maBienThe.Value > 0)
            {
                return RedirectToAction("ChiTietBienThe", "Phong", new { id = maBienThe.Value });
            }

            return RedirectToAction("DanhSachBienThe", "Phong");
        }

        public ActionResult KhuyenMai()
        {
            return View();
        }

        public ActionResult GioiThieu()
        {
            return View();
        }

        public ActionResult DichVu()
        {
            return View();
        }

        public ActionResult LienHe()
        {
            return View();
        }        
    }
}