using Newtonsoft.Json;
using QL_KhachSan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QL_KhachSan.Controllers
{
    public class AccountController : Controller
    {
        QL_KhachSanEntities db = new QL_KhachSanEntities();

        // ================= ĐĂNG KÝ KHÁCH HÀNG =================

        public async Task<ActionResult> DangKyKhachHang()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync("https://provinces.open-api.vn/api/v1/");
                var provinces = JsonConvert.DeserializeObject<List<Province>>(response);
                ViewBag.Provinces = new SelectList(provinces, "name", "name");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DangKyKhachHang(string TenKH, string Gmail, DateTime NgaySinh, string MatKhau, string Gender, string TinhThanh)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync("https://provinces.open-api.vn/api/v1/");
                var provinces = JsonConvert.DeserializeObject<List<Province>>(response);
                ViewBag.Provinces = new SelectList(provinces, "name", "name");
            }

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
            if (NgaySinh >= DateTime.Now)
            {
                ViewBag.Error = "Ngày sinh không hợp lệ";
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

            Session["MaKH"] = kh.MaKH;
            Session["TenKH"] = kh.TenKH;
            Session["LoaiTK"] = "KhachHang";

            return RedirectToAction("Index", "Home");
        }

        // ================= ĐĂNG NHẬP KHÁCH HÀNG =================

        [HttpGet]
        public ActionResult DangNhapKhachHang()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhapKhachHang(string Gmail, string MatKhau)
        {
            if (string.IsNullOrWhiteSpace(Gmail) ||
                string.IsNullOrWhiteSpace(MatKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Gmail và mật khẩu";
                return View();
            }

            string g = Gmail.Trim();
            string mk = MatKhau.Trim();

            var kh = db.KhachHangs
                       .FirstOrDefault(k => k.Gmail.Trim() == g && k.MatKhau.Trim() == mk);

            if (kh == null)
            {
                ViewBag.Error = "Sai Gmail hoặc mật khẩu";
                return View();
            }

            Session["MaKH"] = kh.MaKH;
            Session["TenKH"] = kh.TenKH;
            Session["LoaiTK"] = "KhachHang";
            Cart cart = new Cart();
            Session["GioHang"] = cart;
            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> ChinhSuaThongTin()
        {
            var MaKh = Session["MaKH"];
            var kh = db.KhachHangs.Find(MaKh);
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync("https://provinces.open-api.vn/api/v1/");
                var provinces = JsonConvert.DeserializeObject<List<Province>>(response);
                ViewBag.Provinces = new SelectList(provinces, "name", "name");
            }
            return View(kh);
        }

        public ActionResult ThongTinKhach()
        {
            var MaKh = Session["MaKH"];
            var kh = db.KhachHangs.Find(MaKh);
            return View(kh);
        }
        [HttpPost]
        public ActionResult ChinhSuaThongTin(string TenKH, DateTime NgaySinh, string Gender, string TinhThanh)
        {
            var MaKh = Session["MaKH"];
            var kh = db.KhachHangs.Find(MaKh);

            kh.TenKH = TenKH;
            kh.NgaySinh = NgaySinh;
            kh.GioiTinh = Gender;
            kh.TinhThanh = TinhThanh;

            UpdateModel(kh);
            db.SaveChanges();
            return RedirectToAction("ThongTinKhach");
        }
        // ================= ĐĂNG XUẤT =================
        public ActionResult DoiMatKhau()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DoiMatKhau(string MatKhauCu, string MatKhauMoi, string XacNhanMatKhau)
        {
            var MaKh = Session["MaKH"];
            var kh = db.KhachHangs.Find(MaKh);
            if (kh.MatKhau.Trim() != MatKhauCu)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng!";
                return View();
            }
            if (MatKhauMoi != XacNhanMatKhau)
            {
                ViewBag.Error = "Xác nhận mật khẩu không đúng!";
                return View();
            }
            kh.MatKhau = MatKhauMoi;
            UpdateModel(kh);
            db.SaveChanges();
            return RedirectToAction("ThongTinKhachHang");
        }
        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
