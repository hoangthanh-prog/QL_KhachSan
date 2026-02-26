using System.Web;
using System.Web.Mvc;

namespace QL_KhachSan.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;

            // ✅ Cho phép Account/Login đi qua
            if (controller == "Account" && action == "Login")
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // ❌ Chưa đăng nhập
            if (HttpContext.Current.Session["ADMIN"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "area", "Admin" },
                        { "controller", "Account" },
                        { "action", "Login" }
                    });
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
