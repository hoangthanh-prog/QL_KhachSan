using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QL_KhachSan.Models;

namespace QL_KhachSan.Areas.Admin.Controllers
{
    public class BienThePhongController : AdminBaseController
    {
        private QL_KhachSanEntities db = new QL_KhachSanEntities();

        // ================= INDEX =================
        public ActionResult Index()
        {
            var list = db.BienThePhongs
                         .Include(x => x.HinhAnhs).OrderByDescending(x => x.MaBienThe)
                         .ToList();
            return View(list);
        }

        // ================= CREATE =================
        public ActionResult Create()
        {
            ViewBag.HangPhongList = new SelectList(db.LoaiPhongs.ToList(), "MaLoai", "TenLoai");
            return View();
        }

        [HttpPost]
        public ActionResult Create(BienThePhong model, IEnumerable<HttpPostedFileBase> files, int HangPhong)
        {
            // 1️⃣ Validate model
            if (!ModelState.IsValid)
                return View(model);

            // 3️⃣ Add entity
            model.MaLoai = HangPhong;
            db.BienThePhongs.Add(model);

            db.SaveChanges();

            // 5️⃣ Upload ảnh (nếu có)
            UploadImages(model.MaBienThe, files);

            // 6️⃣ Thông báo thành công
            TempData["Success"] = "✅ Thêm biến thể phòng thành công!";

            return RedirectToAction("Index");
        }



        // ================= EDIT =================
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = db.BienThePhongs
                          .Include(x => x.HinhAnhs)
                          .FirstOrDefault(x => x.MaBienThe == id);

            if (model == null)
                return HttpNotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BienThePhong model, IEnumerable<HttpPostedFileBase> files)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = db.BienThePhongs.Find(model.MaBienThe);
            if (entity == null)
                return HttpNotFound();

            entity.BienThe = model.BienThe;
            entity.MoTa = model.MoTa;
            entity.GiaBan = model.GiaBan;

            db.SaveChanges();

            UploadImages(model.MaBienThe, files);

            TempData["Success"] = "✏️ Cập nhật biến thể phòng thành công!";
            return RedirectToAction("Index");
        }


        // ================= DELETE =================
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = db.BienThePhongs.Find(id);
            if (model == null)
                return HttpNotFound();

            return View(model);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var bienThe = db.BienThePhongs
                            .Include(x => x.HinhAnhs)
                            .FirstOrDefault(x => x.MaBienThe == id);

            if (bienThe == null)
                return HttpNotFound();

            return View(bienThe);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var bienThe = db.BienThePhongs
                            .Include(x => x.Phongs)
                            .Include(x => x.HinhAnhs)
                            .FirstOrDefault(x => x.MaBienThe == id);

            if (bienThe == null)
                return HttpNotFound();

            if (bienThe.Phongs.Any())
            {
                ModelState.AddModelError("",
                    "❌ Không thể xóa vì biến thể này đang được sử dụng.");
                return View(bienThe);
            }

            if (bienThe.HinhAnhs.Any())
            {
                ModelState.AddModelError("",
                    "❌ Vui lòng xóa hết hình ảnh trước khi xóa biến thể.");
                return View(bienThe);
            }

            db.BienThePhongs.Remove(bienThe);
            db.SaveChanges();

            TempData["Success"] = "🗑️ Xóa biến thể phòng thành công!";
            return RedirectToAction("Index");
        }



        // ================= DELETE IMAGE =================
        public ActionResult DeleteImage(int maBienThe, int stt)
        {
            var img = db.HinhAnhs
                        .FirstOrDefault(x => x.MaBienThe == maBienThe && x.Stt == stt);

            if (img != null)
            {
                string path = Path.Combine(Server.MapPath("~/Content/images"), img.HinhAnh1);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                db.HinhAnhs.Remove(img);
                db.SaveChanges();

                TempData["Success"] = "🖼️ Xóa hình ảnh thành công!";
            }

            return RedirectToAction("Edit", new { id = maBienThe });
        }

        // ================= UPLOAD IMAGE =================
        private void UploadImages(int maBienThe, IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null) return;

            string folderPath = Server.MapPath("~/Content/images");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            int stt = db.HinhAnhs
                        .Where(x => x.MaBienThe == maBienThe)
                        .Select(x => (int?)x.Stt)
                        .Max() ?? 0;

            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string ext = Path.GetExtension(file.FileName).ToLower();
                    if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext))
                        continue;

                    string fileName = Guid.NewGuid() + ext;
                    string fullPath = Path.Combine(folderPath, fileName);

                    file.SaveAs(fullPath);

                    db.HinhAnhs.Add(new HinhAnh
                    {
                        MaBienThe = maBienThe,
                        Stt = ++stt,
                        HinhAnh1 = fileName
                    });
                }
            }
            db.SaveChanges();
        }
    }
}
