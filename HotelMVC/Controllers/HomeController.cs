using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Sieć ponad 1000 apartamentów w całej Polsce!";
            
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Skontaktuj się z nami!";

            return View();
        }
    }
}