using QL_KhachSan.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;


namespace QL_KhachSan.Controllers
{
    public class PhongController : Controller
    {
        QL_KhachSanEntities db = new QL_KhachSanEntities();
        public ActionResult _Menu()
        {
            var ds = db.LoaiPhongs.ToList();
            return PartialView(ds);
        }
        [HttpGet]
        public ActionResult DanhSachBienThe(
    int? keyword,
    int? page,
    int? BienThe,
    string timPhong,
    string priceRange,
    DateTime? ngayDen,
    DateTime? ngayDi,
    int? slp,
    string sort = "gia_asc")
        {
            if (page == null) page = 1;

            var listKhoangGia = new List<SelectListItem>
    {
        new SelectListItem { Text = "Dưới 500.000 VNĐ", Value = "0-500000" },
        new SelectListItem { Text = "500.000 - 1.000.000 VNĐ", Value = "500000-1000000" },
        new SelectListItem { Text = "1.000.000 - 2.000.000 VNĐ", Value = "1000000-2000000" },
        new SelectListItem { Text = "2.000.000 - 5.000.000 VNĐ", Value = "2000000-5000000" },
        new SelectListItem { Text = "Trên 5.000.000 VNĐ", Value = "5000000-100000000" }
    };
            ViewBag.PriceRangeList = new SelectList(listKhoangGia, "Value", "Text", priceRange);
            ViewBag.HangPhongList = new SelectList(db.LoaiPhongs.ToList(), "MaLoai", "TenLoai");

            var query = db.BienThePhongs
                          .Include("LoaiPhong")
                          .Include("Phongs")
                          .AsQueryable();

            // 📌 Lọc theo loại phòng
            if (keyword != null)
            {
                query = query.Where(x => x.MaLoai == keyword);
            }
            if (BienThe != null)
            {
                query = query.Where(x => x.MaBienThe == BienThe);
            }

            // 📌 Lọc theo giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                var parts = priceRange.Split('-');
                if (parts.Length == 2)
                {
                    decimal min = decimal.Parse(parts[0]);
                    decimal max = decimal.Parse(parts[1]);
                    query = query.Where(x => x.GiaBan >= min && x.GiaBan <= max);
                }
            }

            // 📌 Lọc theo tên biến thể
            if (!string.IsNullOrEmpty(timPhong))
            {
                query = query.Where(x => x.BienThe.ToLower().Contains(timPhong.ToLower().Trim()));
            }

            // 📌 Lọc theo ngày và số lượng phòng
            if (ngayDen != null && ngayDi != null)
            {
                query = query.Where(x =>
                    x.Phongs.Count(p =>
                        !db.ChiTietHDs.Any(hd =>
                            hd.MaPhong == p.MaPhong &&
                            !(hd.NgayKT < ngayDen || hd.NgayBD > ngayDi)
                        )
                    ) >= (slp ?? 1)   // nếu có slp thì kiểm tra, mặc định >=1
                );
            }

            // 📌 Sắp xếp
            switch (sort)
            {
                case "gia_desc": query = query.OrderByDescending(x => x.GiaBan); break;
                case "trong_asc": query = query.OrderBy(x => x.Phongs.Count(p => p.TinhTrang.Trim() == "Trống")); break;
                case "trong_desc": query = query.OrderByDescending(x => x.Phongs.Count(p => p.TinhTrang.Trim() == "Trống")); break;
                default: query = query.OrderBy(x => x.GiaBan); break;
            }

            
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / 5);

            var items = query
                .Skip(((page ?? 1) - 1) * 5)
                .Take(5)
                .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;
            ViewBag.PriceRange = priceRange;
            ViewBag.Sort = sort;
            ViewBag.TimPhong = timPhong;
            ViewBag.NgayDen = ngayDen;
            ViewBag.NgayDi = ngayDi;
            ViewBag.SLP = slp;

            return View(items);
        }


        public ActionResult ChiTietBienThe(int id)
        {
            var btp = db.BienThePhongs
                        .Include("LoaiPhong")
                        .Include("Phongs")
                        .FirstOrDefault(x => x.MaBienThe == id);

            if (btp == null)
                return HttpNotFound();

            var lienQuan = db.BienThePhongs
                             .Where(x => x.MaLoai == btp.MaLoai && x.MaBienThe != id)
                             .ToList();

            ViewBag.LienQuan = lienQuan;

            DateTime? ngayDen = null, ngayDi = null;
            if (TempData["NgayDen"] != null) ngayDen = (DateTime)TempData["NgayDen"];
            if (TempData["NgayDi"] != null) ngayDi = (DateTime)TempData["NgayDi"];

            ViewBag.NgayDen = ngayDen;
            ViewBag.NgayDi = ngayDi;

            return View(btp);
        }

        [HttpPost]
        public ActionResult ThanhToan(int maBienThe, int sld, DateTime? ngayDen, DateTime? ngayDi)
        {
            #region error warning
            if (Session["MaKH"] == null)
                return RedirectToAction("DangNhapKhachHang", "Account");
            if (ngayDen < DateTime.Today)
            {
                TempData["Message"] = "Ngày đến phải lớn hơn hoặc bằng ngày hiện tại!";
                return RedirectToAction("ChiTietBienThe", new { id = maBienThe });
            }
            if (ngayDi <= ngayDen)
            {
                TempData["Message"] = "Ngày đi phải lớn hơn ngày đến!";
                return RedirectToAction("ChiTietBienThe", new { id = maBienThe });
            }
            if (sld <= 0)
            {
                TempData["Message"] = "Số lượng phòng phải lớn hơn 0!";
                return RedirectToAction("ChiTietBienThe", new { id = maBienThe });
            }
            var ph = db.BienThePhongs.Find(maBienThe);
            var sl = ph.Phongs.Count(p => p.TinhTrang.Trim() == "Trống");
            if (sld > sl)
            {
                TempData["Message"] = "Hiện tại không đủ số lượng phòng, mong quý khách thông cảm!!!";
                return RedirectToAction("ChiTietBienThe", new { id = maBienThe });
            }
            #endregion                        

            DateTime start = ngayDen ?? DateTime.Today;
            DateTime end = ngayDi ?? DateTime.Today.AddDays(1);
            if (end <= start)
            {
                end = start.AddDays(1);
            }
            int soNgay = (end - start).Days;

            decimal gia = ph.GiaBan ?? 0;

            int maKh = Convert.ToInt32(Session["MaKH"]);
            var hd = new HoaDon
            {
                MaKH = maKh,
                NgayLap = DateTime.Now,
                TinhTrang = true,
                TongTien = 0
            };
            db.HoaDons.Add(hd);
            db.SaveChanges();

            decimal tongTien = 0;

            var listPhong = ph.Phongs.Where(p => p.TinhTrang.Trim() == "Trống").ToList();

            foreach (var p in listPhong)
            {
                if (sld == 0)
                {
                    break;
                }
                if ((p.TinhTrang ?? "").Trim() == "Trống")
                {
                    var ct = new ChiTietHD
                    {
                        MaHD = hd.MaHD,
                        MaPhong = p.MaPhong,
                        NgayBD = start,
                        NgayKT = end,
                        SoNgay = soNgay
                    };
                    db.ChiTietHDs.Add(ct);

                    tongTien += gia * soNgay;

                    p.TinhTrang = "Đã đặt";
                }
                sld--;
            }

            hd.TongTien = tongTien;

            db.SaveChanges();

            TempData["NgayDen"] = start;
            TempData["NgayDi"] = end;

            return RedirectToAction("ThanhToanThanhCong");
        }
        public ActionResult ThanhToanThanhCong(int? f)
        {
            int flag = 1;
            if (f != null) flag = (int)f;
            if (Session["MaKH"] == null)
                return RedirectToAction("DangNhapKhachHang", "Account");
            var kh = Convert.ToInt32(Session["MaKH"]);
            var HD_MoiNhat = db.HoaDons.Where(n => n.MaKH == kh).AsEnumerable().Reverse().Take(flag).ToList();
            return View(HD_MoiNhat);
        }
        [HttpPost]
        public ActionResult ThanhToanToanBo()
        {
            Cart gh = (Cart)Session["GioHang"];
            var kh = Convert.ToInt32(Session["MaKH"]);
            int flag = 0;
            foreach (var item in gh.cart)
            {
                ThanhToan(item.MaBienThe, item.SLPhong, item.NgayBD, item.NgayKT);
                flag++;
            }
            var HD_MoiNhat= db.HoaDons.Where(n => n.MaKH == kh).AsEnumerable().Reverse().Take(flag).ToList();
            gh = new Cart();
            Session["GioHang"] = gh;
            return RedirectToAction("ThanhToanThanhCong", new {f = flag});
        }
        public ActionResult GioHang()
        {
            Cart gh = (Cart)Session["GioHang"];
            return View(gh);
        }
        [HttpPost]
        public ActionResult ThemVaoGio(int maBienThe, int sld, DateTime ngayDen, DateTime ngayDi)
        {

            #region error warning
            if (Session["MaKH"] == null)
                return RedirectToAction("DangNhapKhachHang", "Account");
            if (ngayDen < DateTime.Today)
            {
                TempData["Message"] = "Ngày đến phải lớn hơn hoặc bằng ngày hiện tại!";
                return RedirectToAction("ChiTietBienThe", new { id = maBienThe });
            }
            if (ngayDi <= ngayDen)
            {
                TempData["Message"] = "Ngày đi phải lớn hơn ngày đến!";
                return RedirectToAction("ChiTietBienThe", new { id = maBienThe });
            }
            if (sld <= 0)
            {
                TempData["Message"] = "Số lượng phòng phải lớn hơn 0!";
                return RedirectToAction("ChiTietBienThe", new { id = maBienThe });
            }
            var sl = db.Phongs.Count(p => p.MaBienThe == maBienThe && p.TinhTrang.Trim() == "Trống");
            if (sld > sl)
            {
                TempData["Message"] = "Hiện tại không đủ số lượng phòng, mong quý khách thông cảm!!!";
                return RedirectToAction("ChiTietBienThe", new { id = maBienThe });
            }
            #endregion
            Cart gh = (Cart)Session["GioHang"];

            var bt = db.BienThePhongs.FirstOrDefault(b => b.MaBienThe == maBienThe);
            var hinhanh = bt.HinhAnhs.FirstOrDefault(n => n.Stt == 1);

            gh.Them(bt.MaBienThe, bt.BienThe, sld, ngayDen, ngayDi, bt.GiaBan, hinhanh.HinhAnh1, bt.LoaiPhong.TenLoai);

            return RedirectToAction("ChiTietBienThe", new { id = maBienThe });
        }
        public ActionResult XoaKhoiGio(int MaBT)
        {
            Cart gh = (Cart)Session["GioHang"];
            gh.Xoa(MaBT);
            return RedirectToAction("GioHang");
        }
        public ActionResult CapNhatGio(int MaBT)
        {
            Cart gh = (Cart)Session["GioHang"];
            var item = gh.cart.FirstOrDefault(i => i.MaBienThe == MaBT);
            return PartialView(item);
        }
        [HttpPost]
        public ActionResult CapNhatGio(int MaBT, DateTime ngayDen, DateTime ngayDi, int sld)
        {
            #region error warning
            if (ngayDen < DateTime.Today)
            {
                TempData["Message"] = "Ngày đến phải lớn hơn hoặc bằng ngày hiện tại!";
                return RedirectToAction("GioHang");
            }
            if (ngayDi <= ngayDen)
            {
                TempData["Message"] = "Ngày đi phải lớn hơn ngày đến!";
                return RedirectToAction("GioHang");
            }
            if (sld <= 0)
            {
                TempData["Message"] = "Số lượng phòng phải lớn hơn 0!";
                return RedirectToAction("GioHang");
            }
            var sl = db.Phongs.Count(p => p.MaBienThe == MaBT && p.TinhTrang.Trim() == "Trống");
            if (sld > sl)
            {
                TempData["Message"] = "Hiện tại không đủ số lượng phòng, mong quý khách thông cảm!!!";
                return RedirectToAction("GioHang");
            }
            #endregion
            Cart gh = (Cart)Session["GioHang"];
            gh.CapNhatNgay(MaBT, ngayDen, ngayDi, sld);
            return RedirectToAction("GioHang");
        }
    }
}
