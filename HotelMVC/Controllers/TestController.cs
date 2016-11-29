using HotelMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMVC.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index(string nazwa = "nazwa domyślna", int il = 0)
        {
            TestModel model = new TestModel()
            {
                nazwa = nazwa,
                ilosc = il,
                data = DateTime.Now,
                waga = 0.0
            };

            ViewData["dana1"] = "Cześć, ja jestem przekazana przez ViewData.";
            ViewBag.Dana2 = "Cześć, ja jestem przekazana przez ViewBag.";

            return View(model);
        }

        //POST: Test
        [HttpPost]
        public ActionResult Index(TestModel model)
        {
            //tu można wykonać różne operacje na modelu,
            //np. zapisać go w bazie danych

            return RedirectToAction("Index","Home");
        }
    }
}