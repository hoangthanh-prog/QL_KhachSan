using QL_KhachSan.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_KhachSan.Areas.Admin.Controllers
{
    [AdminAuthorize]   // <-- bắt buộc đăng nhập
    public class AdminBaseController : Controller
    {
    }
}