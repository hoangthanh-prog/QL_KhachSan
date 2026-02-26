using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_KhachSan.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TongSoPhong { get; set; }
        public int SoPhongDaDat { get; set; }
        public int SoPhongTrong { get; set; }
        public decimal DoanhThuThang { get; set; }

        // 👉 Thêm các field UI đang dùng
        public int TangPhongThangNay { get; set; }      // +8 phòng trong tháng
        public double TiLeLapDay { get; set; }          // % lắp đầy
        public int NhanPhongHomNay { get; set; }
        public int KhachDuKienNgayMai { get; set; }
        public decimal TangTruongDoanhThu { get; set; } // tăng doanh thu

        public List<PhongTheoLoaiVM> PhongTheoLoai { get; set; }        
    }

    public class PhongTheoLoaiVM
    {
        public string TenLoai { get; set; }
        public int Tong { get; set; }
        public int DaDat { get; set; }

        // auto calc %
        public int PhanTram => Tong > 0 ? (int)((double)DaDat / Tong * 100) : 0;
    }
}
