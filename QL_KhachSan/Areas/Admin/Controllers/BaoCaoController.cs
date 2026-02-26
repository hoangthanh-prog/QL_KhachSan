using QL_KhachSan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QL_KhachSan.Models.ViewModels;

namespace QL_KhachSan.Areas.Admin.Controllers
{
    public class BaoCaoController : AdminBaseController
    {
        private QL_KhachSanEntities db = new QL_KhachSanEntities();

        // =================== DOANH THU ===================
        public ActionResult DoanhThu(DateTime? tuNgay, DateTime? denNgay)
        {
            var hoaDons = db.HoaDons.AsQueryable();

            if (tuNgay.HasValue)
                hoaDons = hoaDons.Where(h => h.NgayLap >= tuNgay.Value);

            if (denNgay.HasValue)
                hoaDons = hoaDons.Where(h => h.NgayLap <= denNgay.Value);

            // ✅ CHỈ LẤY HÓA ĐƠN ĐÃ THANH TOÁN
            hoaDons = hoaDons.Where(h => h.TinhTrang == true);

            var list = hoaDons
                        .OrderByDescending(h => h.NgayLap)
                        .ToList();

            var vm = new BaoCaoDoanhThuVM
            {
                TuNgay = tuNgay,
                DenNgay = denNgay,
                DanhSachHoaDon = list,
                TongDoanhThu = list.Sum(x => x.TongTien ?? 0)
            };

            return View(vm);
        }
    }
}