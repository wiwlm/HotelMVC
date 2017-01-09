using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HotelMVC.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using LinqKit;
using System.Globalization;
using HotelMVC.Infrastructure;
using System.Web.Routing;

namespace HotelMVC.Controllers
{
    [Authorize]
    public class ApartamentyController : Controller
    {
        private EntityContext db = new EntityContext();

        [AllowAnonymous]
        // GET: Apartamenty
        public ActionResult Index()
        {
            var lista = new List<SelectListItem>();
            lista.Add(new SelectListItem() { Value = "", Text = "---" });
            var rez = db.Apartamenty.Select(x => x.Miasto).Distinct().ToList().Select(m => new SelectListItem() { Value = m, Text = m }).ToList();
            lista.AddRange(rez);

            ViewData["MiastaList"] = lista;
            ViewData["UdogodnieniaList"] = db.Udogodnienia.ToList();

            var model = new ApartamentyFilterViewModel()
            {
                WybraneUdogodeniniaIds = new int[] { },
                DataDo = DateTime.Today,
                DataOd = DateTime.Today
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(ApartamentyFilterViewModel model)
        {
            var lista = new List<SelectListItem>();
            lista.Add(new SelectListItem() { Value = "", Text = "---" });
            var rez = db.Apartamenty.Select(x => x.Miasto).Distinct().ToList().Select(m => new SelectListItem() { Value = m, Text = m }).ToList();
            lista.AddRange(rez);

            ViewData["MiastaList"] = lista;
            ViewData["UdogodnieniaList"] = db.Udogodnienia.ToList();

            if (model.WybraneUdogodeniniaIds == null) model.WybraneUdogodeniniaIds = new int[] { };

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Apartament(int Id, string dataOd, string dataDo)
        {
            var ap = db.Apartamenty.Include("Wizyty").Include("UdogodnieniaApartamenty.Udogodnienie").First(x => x.IdApartamentu == Id);

            var model = new ApartamentyReservationViewModel(ap)
            {
                DataOd = this.StringToDateTime(dataOd),
                DataDo = this.StringToDateTime(dataDo),
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Apartament(ApartamentyReservationViewModel model)
        {
            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account",
                    new
                    {
                        returnUrl = Request.Url.AbsolutePath + "?dataOd=" + this.DateTimeToString(model.DataOd) + "&dataDo=" + this.DateTimeToString(model.DataDo)
                    });
            }

            var ap = db.Apartamenty.Include("UdogodnieniaApartamenty.Udogodnienie").First(x => x.IdApartamentu == model.IdApartamentu);

            if (model.DataOd > model.DataDo)
            {
                ViewData["errorInfo"] = "Nie można zarezerwować apartamentu. Data od jest większa niż data do.";

                model = new ApartamentyReservationViewModel(ap)
                {
                    DataOd = model.DataOd,
                    DataDo = model.DataDo
                };

                return View(model);
            }

            if (db.Wizyty == null || !db.Wizyty.Any(w => w.IdApartamentu == model.IdApartamentu && !(w.DataOd > model.DataDo || w.DataDo < model.DataOd)))
            {
                if (ap.Wizyty == null)
                    ap.Wizyty = new List<Wizyty>();

                db.Wizyty.Add(new Wizyty()
                {
                    IdApartamentu = model.IdApartamentu,
                    DataOd = model.DataOd,
                    DataDo = model.DataDo,
                    DataRezerwacji = DateTime.Today,
                    IdKlient = userId
                });

                db.SaveChanges();

                return RedirectToAction("MojeWizyty");
            }
            else
            {
                ViewData["errorInfo"] = "Nie można zarezerwować apartamentu. W dniach " + DateTimeToString(model.DataOd) + " - " + DateTimeToString(model.DataDo) + " jest on niedostępny.";

                model = new ApartamentyReservationViewModel(ap)
                {
                    DataOd = model.DataOd,
                    DataDo = model.DataDo
                };

                return View(model);
            }
        }

        [WlascicielAuth]
        public ActionResult MojeApartamenty()
        {
            List<ApartamentyDisplayViewModel> ap =
                db.Apartamenty
                .Include("Wizyty")
                .Include("UdogodnieniaApartamenty.Udogodnienie").ToList()
                .Where(a => a.IdWlasciciel == User.Identity.GetUserId())
                .Select(a => new ApartamentyDisplayViewModel(a)).ToList();

            if (ap.SelectMany(a => a.Wizyty).Any(w => w.Potwierdzona == null))
            {
                ViewData["Oczekuje"] = ap.SelectMany(a => a.Wizyty).Count(w => w.Potwierdzona == null);
            }

            return View(ap);
        }

        public ActionResult MojeWizyty(int? Id)
        {
            if (Id.HasValue)
            {
                ViewData["idWizyty"] = Id.Value;
            }

            List<WizytyDisplayViewModel> wiz =
                db.Wizyty.Include("Apartament").ToList()
                .Where(w => w.IdKlient == User.Identity.GetUserId())
                .Select(w => new WizytyDisplayViewModel(w)).ToList();

            return View(wiz);
        }

        [WlascicielAuth]
        public ActionResult MojeApartamentyWizyty(int? Id)
        {
            if (Id.HasValue)
            {
                ViewData["idWizyty"] = Id.Value;
            }

            List<WizytyDisplayViewModel> wiz =
                db.Wizyty.Include("Apartament").ToList()
                .Where(w => w.Apartament.IdWlasciciel == User.Identity.GetUserId())
                .OrderBy(x => x.Potwierdzona)
                .ThenBy(y => y.DataOd)
                .Select(w => new WizytyDisplayViewModel(w)).ToList();

            return View(wiz);
        }

        [WlascicielAuth]
        public ActionResult Potwierdz(int? Id)
        {
            if (Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Wizyty wiz = db.Wizyty.Include("Apartament").ToList().First(w => w.IdWizyty == Id);

            if (wiz == null)
            {
                return HttpNotFound();
            }

            if (wiz.Apartament.IdWlasciciel != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            wiz.Potwierdzona = true;
            db.SaveChanges();

            return RedirectToAction("MojeApartamentyWizyty");
        }

        [WlascicielAuth]
        public ActionResult Odrzuc(int? Id)
        {
            if (Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Wizyty wiz = db.Wizyty.Include("Apartament").ToList().First(w => w.IdWizyty == Id);

            if (wiz == null)
            {
                return HttpNotFound();
            }

            if (wiz.Apartament.IdWlasciciel != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            wiz.Potwierdzona = false;
            db.SaveChanges();

            return RedirectToAction("MojeApartamentyWizyty");
        }

        [WlascicielAuth]
        // GET: Apartamenty/Details/5
        public ActionResult Details(int? id, int? idWizyty)
        {
            if (idWizyty.HasValue)
            {
                ViewData["idWizyty"] = idWizyty.Value;
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Apartamenty apartament =
                db.Apartamenty
                .Include("Wizyty")
                .Include("UdogodnieniaApartamenty.Udogodnienie")
                .First(x => x.IdApartamentu == id);

            if (apartament == null)
            {
                return HttpNotFound();
            }

            if (apartament.IdWlasciciel != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            ApartamentyDisplayViewModel ap = new ApartamentyDisplayViewModel(apartament);

            return View(ap);
        }

        [WlascicielAuth]
        // GET: Apartamenty/Create
        public ActionResult Create()
        {
            ApartamentyEditViewModel model = new ApartamentyEditViewModel()
            {
                Apartament = new Apartamenty(),
                WszystkieUdogodnienia = db.Udogodnienia.ToList(),
                WybraneUdogodeniniaIds = new int[] { },
                WybraneUdogodnienia = new List<Udogodnienia>()
            };

            return View(model);
        }

        [WlascicielAuth]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ApartamentyEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Apartamenty ap = model.Apartament;

                ap.IdWlasciciel = User.Identity.GetUserId();
                db.Apartamenty.Add(ap);
                db.SaveChanges();

                List<UdogodnieniaApartamenty> udogodnieniaApartamenty = new List<UdogodnieniaApartamenty>();

                foreach (var u in model.WybraneUdogodeniniaIds)
                {
                    db.UdogodnieniaApartamenty.Add(new UdogodnieniaApartamenty()
                    {
                        IdApartamentu = ap.IdApartamentu,
                        IdUdogodnienia = u
                    });
                }

                db.SaveChanges();
                return RedirectToAction("MojeApartamenty");
            }

            model.WszystkieUdogodnienia = db.Udogodnienia.ToList();
            model.WybraneUdogodnienia = db.Udogodnienia.ToList().Where(x => model.WybraneUdogodeniniaIds.Contains(x.IdUdogodnienia)).ToList();
            return View(model);
        }

        // GET: Apartamenty/Edit/5
        [WlascicielAuth]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Apartamenty apartament = db.Apartamenty.Find(id);

            if (apartament == null)
            {
                return HttpNotFound();
            }

            if (apartament.IdWlasciciel != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            ApartamentyEditViewModel model = new ApartamentyEditViewModel()
            {
                Apartament = apartament,
                WszystkieUdogodnienia = db.Udogodnienia.ToList(),
                WybraneUdogodnienia = db.UdogodnieniaApartamenty.Where(x => x.IdApartamentu == id).Select(u => u.Udogodnienie).ToList(),
            };

            model.WybraneUdogodeniniaIds = model.WybraneUdogodnienia.Select(x => x.IdUdogodnienia).ToArray();

            return View(model);
        }

        [WlascicielAuth]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApartamentyEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Apartamenty ap_new = model.Apartament;

                Apartamenty ap_old = db.Apartamenty.Find(ap_new.IdApartamentu);

                if (ap_old.IdWlasciciel != User.Identity.GetUserId())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                }

                ap_old.Cena = ap_new.Cena;
                ap_old.IloscOsob = ap_new.IloscOsob;
                ap_old.KodPocztowy = ap_new.KodPocztowy;
                ap_old.Miasto = ap_new.Miasto;
                ap_old.Nazwa = ap_new.Nazwa;
                ap_old.Opis = ap_new.Opis;
                ap_old.Ulica = ap_new.Ulica;
                ap_old.IdWlasciciel = User.Identity.GetUserId();
                db.SaveChanges();

                List<UdogodnieniaApartamenty> udogodnieniaApartamentyOld = db.UdogodnieniaApartamenty.Where(x => x.IdApartamentu == ap_new.IdApartamentu).ToList();
                db.UdogodnieniaApartamenty.RemoveRange(udogodnieniaApartamentyOld);

                List<UdogodnieniaApartamenty> udogodnieniaApartamenty = new List<UdogodnieniaApartamenty>();

                foreach (var u in model.WybraneUdogodeniniaIds)
                {
                    db.UdogodnieniaApartamenty.Add(new UdogodnieniaApartamenty()
                    {
                        IdApartamentu = ap_new.IdApartamentu,
                        IdUdogodnienia = u
                    });
                }

                db.SaveChanges();
                return RedirectToAction("MojeApartamenty");
            }

            model.WszystkieUdogodnienia = db.Udogodnienia.ToList();

            if (model.WybraneUdogodeniniaIds == null)
                model.WybraneUdogodeniniaIds = new int[] { };
            else
                model.WybraneUdogodnienia = db.Udogodnienia.ToList().Where(x => model.WybraneUdogodeniniaIds.Contains(x.IdUdogodnienia)).ToList();

            return View(model);
        }

        [WlascicielAuth]
        // GET: Apartamenty/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Apartamenty apartament =
                db.Apartamenty
                .Include("UdogodnieniaApartamenty.Udogodnienie")
                .First(x => x.IdApartamentu == id);

            if (apartament == null)
            {
                return HttpNotFound();
            }

            if (apartament.IdWlasciciel != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            ApartamentyDisplayViewModel ap = new ApartamentyDisplayViewModel(apartament);

            return View(ap);
        }

        // POST: Apartamenty/Delete/5
        [WlascicielAuth]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Apartamenty apartament = db.Apartamenty.Find(id);

            if (apartament.IdWlasciciel != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            db.Apartamenty.Remove(apartament);
            db.SaveChanges();
            return RedirectToAction("MojeApartamenty");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ApartamentyLista(ApartamentyFilterViewModel filtr)
        {
            var predicate1 = PredicateBuilder.New<Apartamenty>(true);

            if (!String.IsNullOrEmpty(filtr.Miasto)) { predicate1 = predicate1.And(a => a.Miasto == filtr.Miasto); }
            if (filtr.CenaOd.HasValue && filtr.CenaOd != 0) { predicate1 = predicate1.And(a => a.Cena >= filtr.CenaOd); }
            if (filtr.CenaDo.HasValue && filtr.CenaDo != 0) { predicate1 = predicate1.And(a => a.Cena <= filtr.CenaDo); }
            if (filtr.IleOsob.HasValue && filtr.IleOsob != 0) { predicate1 = predicate1.And(a => a.IloscOsob == filtr.IleOsob); }

            var predicate2 = PredicateBuilder.New<Apartamenty>(true);
            predicate2 = predicate2.And(a => a.Wizyty == null || !a.Wizyty.Any(w => w.Potwierdzona != false && !(w.DataOd > filtr.DataDo || w.DataDo < filtr.DataOd)));

            if (filtr.WybraneUdogodeniniaIds != null)
                foreach (var item in filtr.WybraneUdogodeniniaIds)
                {
                    predicate2 = predicate2.And(a => a.UdogodnieniaApartamenty != null && a.UdogodnieniaApartamenty.Any(x => x.IdUdogodnienia == item));
                }

            var result = db.Apartamenty.Where(predicate1)
                .Include("Wizyty")
                .Include("UdogodnieniaApartamenty.Udogodnienie").ToList()
                .Where(predicate2)
                .Select(a => new ApartamentyDisplayViewModel(a));

            if (result.Any())
            {
                result = result.OrderByDescending(x => x.Ocena).ThenBy(y => y.Nazwa).ToList();
            }

            ViewData["dataOd"] = this.DateTimeToString(filtr.DataOd);
            ViewData["dataDo"] = this.DateTimeToString(filtr.DataDo);

            return PartialView("_ApartamentyLista", result);
        }

        [ChildActionOnly]
        public ActionResult Komentarz(int Id)
        {
            if (Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Wizyty wiz = db.Wizyty.Include("Apartament").ToList().First(w => w.IdWizyty == Id);

            if (wiz == null)
            {
                return HttpNotFound();
            }

            if (wiz.IdKlient != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            WizytyDisplayViewModel rez = new WizytyDisplayViewModel(wiz);

            ViewData["OcenaList"] = new List<SelectListItem>()
            {
                new SelectListItem() {Value = "5", Text = "5" },
                new SelectListItem() {Value = "4", Text = "4" },
                new SelectListItem() {Value = "3", Text = "3" },
                new SelectListItem() {Value = "2", Text = "2" },
                new SelectListItem() {Value = "1", Text = "1" },
        };

            return PartialView("_Komentarz", rez);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Komentarz(WizytyDisplayViewModel model)
        {
            if (model.IdWizyty == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Wizyty wiz = db.Wizyty.Include("Apartament").ToList().First(w => w.IdWizyty == model.IdWizyty);

            if (wiz == null)
            {
                return HttpNotFound();
            }

            if (wiz.IdKlient != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            wiz.Komentarz = model.Komentarz;
            wiz.Ocena = model.Ocena;
            wiz.DataKomentarz = DateTime.Now;

            db.SaveChanges();
            return RedirectToAction("MojeWizyty");
        }

        [WlascicielAuth]
        [ChildActionOnly]
        public ActionResult Odpowiedz(int Id)
        {
            if (Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Wizyty wiz = db.Wizyty.Include("Apartament").ToList().First(w => w.IdWizyty == Id);

            if (wiz == null)
            {
                return HttpNotFound();
            }

            if (wiz.Apartament.IdWlasciciel != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            WizytyDisplayViewModel rez = new WizytyDisplayViewModel(wiz);


            return PartialView("_Odpowiedz", rez);
        }

        [WlascicielAuth]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Odpowiedz(WizytyDisplayViewModel model)
        {
            if (model.IdWizyty == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Wizyty wiz = db.Wizyty.Include("Apartament").ToList().First(w => w.IdWizyty == model.IdWizyty);

            if (wiz == null)
            {
                return HttpNotFound();
            }

            if (wiz.Apartament.IdWlasciciel != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            wiz.Odpowiedz = model.Odpowiedz;
            wiz.DataOdpowiedz = DateTime.Now;

            db.SaveChanges();
            
            return RedirectToRoute("MojeApartamentyWizyty");
        }

        public string DateTimeToString(DateTime date)
        {
            return date.ToString("MM-dd-yyyy");
        }

        public DateTime StringToDateTime(string date)
        {
            return DateTime.ParseExact(date, "MM-dd-yyyy", System.Globalization.CultureInfo.CurrentCulture);
        }
    }
}
