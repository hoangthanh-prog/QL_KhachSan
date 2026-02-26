using QL_KhachSan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_KhachSan.Areas.Admin.Controllers
{
    public class HoaDonController : AdminBaseController
    {
        private QL_KhachSanEntities db = new QL_KhachSanEntities();
        // GET: Admin/DatPhongGiupKhach
        [HttpGet]
        public ActionResult DanhSachHoaDon(string TenKH, DateTime? tuNgay, DateTime? denNgay)
        {
            var ds = db.HoaDons.AsQueryable();
            
            if (tuNgay.HasValue)
                ds = ds.Where(h => h.NgayLap >= tuNgay.Value);

            if (denNgay.HasValue)
                ds = ds.Where(h => h.NgayLap <= denNgay.Value);

            if (!string.IsNullOrEmpty(TenKH))
            {
                ds = ds.Where(h => h.KhachHang.TenKH.ToLower().Contains(TenKH.ToLower().Trim()));
            }
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;

            var result = ds.OrderByDescending(h => h.MaHD).ToList();
            return View(result);
        }
        public ActionResult ThanhToan(int id)
        {
            var hoaDon = db.HoaDons.Find(id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            hoaDon.TinhTrang = true;
            db.SaveChanges();
            return RedirectToAction("DanhSachHoaDon");
        }
        public ActionResult HuyThanhToan(int id)
        {
            var hoaDon = db.HoaDons.Find(id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            hoaDon.TinhTrang = false;
            db.SaveChanges();
            return RedirectToAction("DanhSachHoaDon");
        }
    }
}