using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_KhachSan.Models
{
    public class CartItem
    {
        public int MaBienThe { get; set; }
        public string TenBienThe { get; set; }
        public string HangPhong { get; set; }
        public string HinhAnh { get; set; }
        public decimal GiaPhong { get; set; }
        public int SLPhong { get; set; }
        public DateTime NgayBD { get; set; }
        public DateTime NgayKT { get; set; }
        public decimal ThanhTien
        {
            get
            {
                TimeSpan SL_NgayDat = NgayKT - NgayBD;
                return SLPhong * (GiaPhong * SL_NgayDat.Days);
            }
        }
    }
    public class Cart
    {
        public List<CartItem> cart { get; set; }
        public Cart()
        {
            cart = new List<CartItem>();
        }
        public Cart(List<CartItem> gh)
        {
            cart = gh;
        }
        public void Them(int MaBT, string TenBT, int SLPH, DateTime NgayBD, DateTime NgayKT, decimal? GiaPhong, string anh, string hp)
        {
            var ph = cart.FirstOrDefault(i => i.MaBienThe == MaBT);
            if (ph == null)
            {
                var item = new CartItem
                {
                    MaBienThe = MaBT,
                    TenBienThe = TenBT,
                    HangPhong = hp,
                    HinhAnh = anh,
                    SLPhong = SLPH,
                    GiaPhong = GiaPhong ?? 0m,
                    NgayBD = NgayBD,
                    NgayKT = NgayKT
                };
                cart.Add(item);
            }
            else
            {
                ph.SLPhong++;
                ph.NgayBD = NgayBD;
                ph.NgayKT = NgayKT;
            }
        }
        public void Xoa(int MaBT)
        {
            var ph = cart.FirstOrDefault(i => i.MaBienThe == MaBT);
            if (ph != null)
            {
                cart.Remove(ph);
            }
        }
        public int SL_LoaiPhong()
        {
            return cart.Count;
        }

        public decimal TongTien()
        {
            return cart.Sum(n => n.ThanhTien);
        }
        public void CapNhatNgay(int MaBT, DateTime ngayDen, DateTime ngayDi, int sld)
        {
            var change = cart.FirstOrDefault(n => n.MaBienThe == MaBT);
            if (change != null)
            {
                change.NgayBD = ngayDen;
                change.NgayKT = ngayDi;
                change.SLPhong = sld;
            }
        }
    }
}