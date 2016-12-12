using HotelMVC.Controllers;
using HotelMVC.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMVC
{
    public static class GlobalObjects
    {
        public static ApplicationUser LoggedUser
        {
            get
            {
                var store = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var userManager = new UserManager<ApplicationUser>(store);
                var id = HttpContext.Current.User.Identity.GetUserId();

                if (String.IsNullOrEmpty(id)) return new ApplicationUser();

                ApplicationUser user = userManager.FindById(id);

                if (user == null) return new ApplicationUser();

                return user;
            }
        }
    }
}