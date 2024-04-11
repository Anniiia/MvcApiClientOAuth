using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MvcApiClientOAuth.Filters
{
    public class AuthorizeEmpleadosAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity.IsAuthenticated == false)
            {
                RouteValueDictionary routeLogin = new RouteValueDictionary(new
                {
                    controller = "Managed",
                    action = "Login"
                });
                context.Result = new RedirectToRouteResult(routeLogin);
            }
        }
    }
}
