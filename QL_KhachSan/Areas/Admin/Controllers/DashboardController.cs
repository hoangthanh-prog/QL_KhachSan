using System;
using System.Linq;
using System.Web.Mvc;
using QL_KhachSan.Models;
using QL_KhachSan.Models.ViewModels;
using System.Data.Entity;

namespace QL_KhachSan.Areas.Admin.Controllers
{
    public class DashboardController : AdminBaseController
    {
        private QL_KhachSanEntities db = new QL_KhachSanEntities();

        public ActionResult Index(int? page)
        {
            if (page == null || page < 1) page = 1;

            var model = new DashboardViewModel();

            // Tổng số phòng
            model.TongSoPhong = db.Phongs.Count();

            // Phòng đã đặt / phòng trống
            model.SoPhongDaDat = db.Phongs.Count(p => p.TinhTrang == "Đã đặt");
            model.SoPhongTrong = db.Phongs.Count(p => p.TinhTrang == "Trống");

            // Doanh thu tháng
                        model.DoanhThuThang = db.HoaDons
            .Where(h =>
                h.NgayLap.HasValue &&
                h.NgayLap.Value.Month == DateTime.Now.Month &&
                h.NgayLap.Value.Year == DateTime.Now.Year &&
                h.TinhTrang == true
            )
            .Sum(h => (decimal?)h.TongTien) ?? 0;

            // Phòng theo loại — FIX: Include Phongs để tránh lazy loading lỗi
            model.PhongTheoLoai = db.BienThePhongs
                .Include(bt => bt.Phongs)
                .Select(bt => new PhongTheoLoaiVM
                {
                    TenLoai = bt.BienThe,
                    Tong = bt.Phongs.Count(),
                    DaDat = bt.Phongs.Count(p => p.TinhTrang == "Đã đặt")
                }).ToList();

            // Đặt phòng gần đây — FIX: Include đầy đủ navigation
            var HoaDon = db.HoaDons.OrderByDescending(n => n.MaHD).ToList();

            int totalItems = HoaDon.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / 10);
            if (page > totalPages) page = totalPages;

            var PageHoaDon = HoaDon
                .Skip(((page ?? 1) - 1) * 10)
                .Take(10)
                .ToList();

            // Phòng được đăt trong hôm nay
            var SL_HoaDonHomNay = db.HoaDons.Where(n => n.NgayLap == DateTime.Today).ToList();

            ViewBag.HoaDonHomNay = SL_HoaDonHomNay.Count();
            ViewBag.Page = page;
            ViewBag.HoaDon = PageHoaDon;
            ViewBag.TotalPages = totalPages;
            return View(model);
        }

        public ActionResult _DoanhThu()
        {
            var doanhthu = db.HoaDons
                        .Include(h => h.KhachHang)
                        .Where(h => h.NgayLap.HasValue && h.NgayLap.Value.Month == DateTime.Now.Month)
                        .ToList();
            return PartialView("_DoanhThu", doanhthu);
        }
        public ActionResult _ChiTietHD(int MaHD)
        {
            var chiTiet = db.ChiTietHDs.Where(n => n.MaHD == MaHD).ToList();
            return PartialView(chiTiet);
        }
    }
}