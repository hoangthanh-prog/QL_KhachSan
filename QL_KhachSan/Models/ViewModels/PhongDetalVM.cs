using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_KhachSan.Models.ViewModels
{
    public class PhongDetailVM
    {
        public Phong Phong { get; set; }
        public List<HinhAnh> DanhSachHinh { get; set; }
    }
}