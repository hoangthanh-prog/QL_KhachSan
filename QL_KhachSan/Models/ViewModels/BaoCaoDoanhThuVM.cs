using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_KhachSan.Models.ViewModels
{
    public class BaoCaoDoanhThuVM
    {
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }

        public decimal TongDoanhThu { get; set; }

        public List<HoaDon> DanhSachHoaDon { get; set; }
    }
}