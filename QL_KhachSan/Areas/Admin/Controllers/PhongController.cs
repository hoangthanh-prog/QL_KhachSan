using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using QL_KhachSan.Models;
using QL_KhachSan.Models.ViewModels;

namespace QL_KhachSan.Areas.Admin.Controllers
{
    public class PhongController : AdminBaseController
    {
        private QL_KhachSanEntities db = new QL_KhachSanEntities();

        // ======================== INDEX =========================
        public ActionResult Index(int? BienThe, int? LoaiPhong, string[] TinhTrang)
        {
            var phongs = db.Phongs.AsQueryable();

            if (BienThe != null)
            {
                phongs = phongs.Where(n => n.MaBienThe == BienThe);
            }
            if (LoaiPhong != null)
            {
                phongs = phongs.Where(n => n.BienThePhong.LoaiPhong.MaLoai == LoaiPhong);
            }
            if (TinhTrang != null && TinhTrang.Any())
            {
                phongs = phongs.Where(p => TinhTrang.Contains(p.TinhTrang));
            }
            ViewBag.HangPhongList = new SelectList(db.LoaiPhongs.ToList(), "MaLoai", "TenLoai");
            ViewBag.BienThePhongList = new SelectList(db.BienThePhongs.ToList(), "MaBienThe", "BienThe");

            return View(phongs.ToList());
        }
        public ActionResult TraPhong(int id)
        {
            var phong = db.Phongs.Find(id);
            if (phong == null)
            {
                return HttpNotFound();
            }
            phong.TinhTrang = "Trống";
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult NhanPhong(int id)
        {
            var phong = db.Phongs.Find(id);
            if (phong == null)
            {
                return HttpNotFound();
            }
            phong.TinhTrang = "Đã đặt";
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // ======================== DETAILS =========================
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var phong = db.Phongs
                          .Include(p => p.BienThePhong)
                          .FirstOrDefault(p => p.MaPhong == id);

            if (phong == null)
                return HttpNotFound();

            var dsHinh = new List<HinhAnh>();

            if (phong.MaBienThe.HasValue)
            {
                dsHinh = db.HinhAnhs
                           .Where(h => h.MaBienThe == phong.MaBienThe.Value)
                           .OrderBy(h => h.Stt)
                           .ToList();
            }

            var vm = new PhongDetailVM
            {
                Phong = phong,
                DanhSachHinh = dsHinh
            };

            return View(vm);
        }

        // ======================== CREATE =========================
        public ActionResult Create()
        {
            LoadDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Phong model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdown(model.MaBienThe, model.TinhTrang);
                return View(model);
            }

            db.Phongs.Add(model);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // ======================== EDIT =========================
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var phong = db.Phongs.Find(id);
            if (phong == null)
                return HttpNotFound();

            LoadDropdown(phong.MaBienThe, phong.TinhTrang);
            return View(phong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Phong model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdown(model.MaBienThe, model.TinhTrang);
                return View(model);
            }

            var entity = db.Phongs.Find(model.MaPhong);
            if (entity == null)
                return HttpNotFound();

            entity.TenPhong = model.TenPhong;
            entity.MaBienThe = model.MaBienThe;
            entity.TinhTrang = model.TinhTrang;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // ======================== DELETE =========================
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var phong = db.Phongs
                          .Include(p => p.BienThePhong)
                          .FirstOrDefault(p => p.MaPhong == id);

            if (phong == null)
                return HttpNotFound();

            return View(phong);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            bool daSuDung = db.ChiTietHDs.Any(ct => ct.MaPhong == id);

            if (daSuDung)
            {
                ModelState.AddModelError("",
                    "Không thể xóa phòng vì phòng này đã phát sinh hóa đơn.");

                var phong = db.Phongs
                              .Include(p => p.BienThePhong)
                              .FirstOrDefault(p => p.MaPhong == id);

                return View("Delete", phong);
            }

            var phongXoa = db.Phongs.Find(id);
            db.Phongs.Remove(phongXoa);
            db.SaveChanges();

            return RedirectToAction("Index");
        }



        // ======================== DROPDOWN =========================
        private void LoadDropdown(int? selectedBienThe = null, string selectedTinhTrang = null)
        {
            ViewBag.MaBienThe = new SelectList(
                db.BienThePhongs.ToList(),
                "MaBienThe",
                "BienThe",
                selectedBienThe
            );

            ViewBag.TinhTrang = new SelectList(
                new List<string> { "Trống", "Đã đặt", "Đang bảo trì" },
                selectedTinhTrang
            );
        }

        // ======================== THỐNG KÊ =========================
        public ActionResult TongSoPhong()
        {
            return View(db.Phongs.ToList());
        }

        // ======================== DISPOSE =========================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
