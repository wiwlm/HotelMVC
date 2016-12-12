using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;

namespace HotelMVC.Infrastructure
{
    public class AdminAuthAttribute : FilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var isAdmin = context.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByName(context.HttpContext.User.Identity.Name).Uprawnienie == 1;

                if (!isAdmin)
                {
                    context.Result = new HttpUnauthorizedResult();
                }
            }
            else
            {
                context.Result = new HttpUnauthorizedResult();
            }
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext context)
        {
            if (context.Result == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "Login" }, { "controller", "Account" } });
            }
        }
    }

    public class WlascicielAuthAttribute : FilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var upr = context.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByName(context.HttpContext.User.Identity.Name).Uprawnienie;

                var isWlasciciel = upr == 1 || upr == 2;

                if (!isWlasciciel)
                {
                    context.Result = new HttpUnauthorizedResult();
                }
            }
            else
            {
                context.Result = new HttpUnauthorizedResult();
            }
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext context)
        {
            if (context.Result == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "Login" }, { "controller", "Account" } });
            }
        }
    }
}