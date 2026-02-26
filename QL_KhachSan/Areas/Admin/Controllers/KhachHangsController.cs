using Newtonsoft.Json;
using QL_KhachSan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QL_KhachSan.Areas.Admin.Controllers
{
    public class KhachHangsController : AdminBaseController
    {
        private QL_KhachSanEntities db = new QL_KhachSanEntities();

        // GET: Admin/KhachHangs
        public ActionResult Index(string search)
        {
            var dsKH = db.KhachHangs.AsQueryable();

            if (!String.IsNullOrEmpty(search))
            {
                dsKH = dsKH.Where(k =>
                    k.TenKH.Contains(search) ||
                    k.Gmail.Contains(search) ||
                    k.GioiTinh.Contains(search) ||
                    k.TinhThanh.Contains(search)
                );
            }

            return View(dsKH.ToList().OrderByDescending(n => n.MaKH));
        }

        // GET: Admin/KhachHangs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        // GET: Admin/KhachHangs/Create
        public async Task<ActionResult> Create()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync("https://provinces.open-api.vn/api/v1/");
                var provinces = JsonConvert.DeserializeObject<List<Province>>(response);
                ViewBag.Provinces = new SelectList(provinces, "name", "name");
            }
            return View();
        }

        // POST: Admin/KhachHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string TenKH, string Gmail, DateTime NgaySinh, string MatKhau, string Gender, string TinhThanh)
        {
            if (string.IsNullOrWhiteSpace(TenKH) ||
                string.IsNullOrWhiteSpace(Gmail) ||
                string.IsNullOrWhiteSpace(MatKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Tên, Gmail, Mật khẩu";
                return View();
            }

            string g = Gmail.Trim();

            bool existed = db.KhachHangs.Any(k => k.Gmail.Trim() == g);
            if (existed)
            {
                ViewBag.Error = "Gmail đã tồn tại";
                return View();
            }



            var kh = new KhachHang
            {
                TenKH = TenKH.Trim(),
                NgaySinh = NgaySinh,
                Gmail = g.Trim(),
                MatKhau = MatKhau.Trim(),
                GioiTinh = Gender,
                TinhThanh = TinhThanh
            };

            db.KhachHangs.Add(kh);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Admin/KhachHangs/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync("https://provinces.open-api.vn/api/v1/");
                var provinces = JsonConvert.DeserializeObject<List<Province>>(response);
                ViewBag.Provinces = new SelectList(provinces, "name", "name");
            }
            return View(khachHang);
        }

        [HttpPost]
        public ActionResult Edit(int MaKH, string TenKH, string Gmail, DateTime NgaySinh, string MatKhau, string Gender, string TinhThanh)
        {
            var kh = db.KhachHangs.Find(MaKH);
            if (kh == null) return HttpNotFound();

            string g = Gmail.Trim();

            bool existed = db.KhachHangs.Any(k => k.Gmail.Trim() == g && k.MaKH != MaKH);
            if (existed)
            {
                ViewBag.Error = "Gmail đã tồn tại";
                return View(kh);
            }

            kh.TenKH = TenKH.Trim();
            kh.NgaySinh = NgaySinh;
            kh.Gmail = g;
            kh.MatKhau = MatKhau.Trim();
            kh.GioiTinh = Gender;
            kh.TinhThanh = TinhThanh;

            UpdateModel(kh);
            db.SaveChanges();

            return RedirectToAction("Index");
        }


        // GET: Admin/KhachHangs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        // POST: Admin/KhachHangs/Delete/5
        [HttpPost]
        public ActionResult DeleteConfirmed(int id)
        {
            KhachHang khachHang = db.KhachHangs.Find(id);
            db.KhachHangs.Remove(khachHang);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult DatPhong(int MaKH, int? BienThe, int? LoaiPhong)
        {
            ViewBag.MaKH = MaKH;
            var dsBienThe = db.BienThePhongs.ToList();

            if (LoaiPhong != null)
            {
                dsBienThe = dsBienThe.Where(n => n.LoaiPhong.MaLoai == LoaiPhong).ToList();
            }

            ViewBag.HangPhongList = new SelectList(db.LoaiPhongs.ToList(), "MaLoai", "TenLoai");
            return View(dsBienThe);
        }

        [HttpPost]
        public ActionResult DatPhong(int maBienThe, int sld, DateTime? ngayDen, DateTime? ngayDi, int MaKH)
        {
            #region error warning
            if (ngayDen < DateTime.Today)
            {
                TempData["Message"] = "Ngày đến phải lớn hơn hoặc bằng ngày hiện tại!";
                return View("DatPhong", new { MaKH = MaKH });
            }
            if (ngayDi <= ngayDen)
            {
                TempData["Message"] = "Ngày đi phải lớn hơn ngày đến!";
                return View("DatPhong", new { MaKH = MaKH });
            }
            if (sld <= 0)
            {
                TempData["Message"] = "Số lượng phòng phải lớn hơn 0!";
                return View("DatPhong", new { MaKH = MaKH });
            }
            var ph = db.BienThePhongs.Find(maBienThe);
            var sl = ph.Phongs.Count(p => p.TinhTrang.Trim() == "Trống");
            if (sld > sl)
            {
                TempData["Message"] = "Hiện tại không đủ số lượng phòng!!!";
                return View();
            }
            #endregion                        

            var nv = (TaiKhoanNV)Session["Admin"];
            DateTime start = ngayDen ?? DateTime.Today;
            DateTime end = ngayDi ?? DateTime.Today.AddDays(1);
            if (end <= start)
            {
                end = start.AddDays(1);
            }
            int soNgay = (end - start).Days;

            decimal gia = ph.GiaBan ?? 0;

            var hd = new HoaDon
            {
                MaKH = MaKH,
                NgayLap = DateTime.Now,
                TinhTrang = true,
                TongTien = 0,
                MaNV = nv.MaNV
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

            TempData["Message"] = "Đặt phòng thành công!";
            TempData["NgayDen"] = start;
            TempData["NgayDi"] = end;

            return RedirectToAction("DatPhongThanhCong");
        }
        public ActionResult DatPhongThanhCong()
        {
            var nv = Session["Admin"] as TaiKhoanNV;
            var HD_MoiNhat = db.HoaDons
                                .OrderByDescending(hd => hd.MaHD)
                                .FirstOrDefault(n => n.MaNV == nv.MaNV);
            return View(HD_MoiNhat);
        }
    }
}