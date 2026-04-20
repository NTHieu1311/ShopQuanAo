using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ShopQuanAo.Filters 
{
    public class BlockAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity.IsAuthenticated && user.IsInRole("Admin"))
            {
                context.Result = new RedirectResult("/Admin");
            }

            base.OnActionExecuting(context);
        }
    }
}