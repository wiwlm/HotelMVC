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

namespace HotelMVC.Controllers
{
    public class ApartamentyController : Controller
    {
        private EntityContext db = new EntityContext();

        // GET: Apartamenty
        public ActionResult Index()
        {
            var lista = new List<SelectListItem>();
            lista.Add(new SelectListItem() { Value = "", Text = "---" });
            var rez = db.Apartamenty.Select(x => x.Miasto).Distinct().ToList().Select(m => new SelectListItem() { Value = m, Text = m }).ToList();
            lista.AddRange(rez);
            ViewData["MiastaList"] = lista;

            ViewData["UdogodnieniaList"] = db.Udogodnienia.ToList();

            return View(new ApartamentyFilterViewModel() { WybraneUdogodeniniaIds = new int[] { } });
        }

        [HttpPost]
        public ActionResult Index(ApartamentyFilterViewModel model)
        {
            var lista = new List<SelectListItem>();
            lista.Add(new SelectListItem() { Value = "", Text = "---" });
            var rez = db.Apartamenty.Select(x => x.Miasto).Distinct().ToList().Select(m => new SelectListItem() { Value = m, Text = m }).ToList();
            lista.AddRange(rez);
            ViewData["MiastaList"] = lista;

            ViewData["UdogodnieniaList"] = db.Udogodnienia.ToList();

            return View(model);
        }

        public ActionResult MojeApartamenty()
        {
            List<ApartamentyDisplayViewModel> ap =
                db.Apartamenty
                .Include("UdogodnieniaApartamenty.Udogodnienie").ToList()
                .Where(a => a.IdWlasciciel == User.Identity.GetUserId())
                .Select(a => new ApartamentyDisplayViewModel(a)).ToList();

            return View(ap);
        }

        public ActionResult MojeWizyty()
        {
            List<WizytyDisplayViewModel> wiz =
                db.Wizyty.Include("Apartament").ToList()
                .Where(w => w.IdKlient == User.Identity.GetUserId())
                .Select(w => new WizytyDisplayViewModel(w)).ToList();

            return View(wiz);
        }

        // GET: Apartamenty/Details/5
        public ActionResult Details(int? id)
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

            ApartamentyDisplayViewModel ap = new ApartamentyDisplayViewModel(apartament);

            return View(ap);
        }

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

        // POST: Apartamenty/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

            ApartamentyEditViewModel model = new ApartamentyEditViewModel()
            {
                Apartament = apartament,
                WszystkieUdogodnienia = db.Udogodnienia.ToList(),
                WybraneUdogodnienia = db.UdogodnieniaApartamenty.Where(x => x.IdApartamentu == id).Select(u => u.Udogodnienie).ToList(),
            };

            model.WybraneUdogodeniniaIds = model.WybraneUdogodnienia.Select(x => x.IdUdogodnienia).ToArray();

            return View(model);
        }

        // POST: Apartamenty/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApartamentyEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Apartamenty ap = model.Apartament;

                ap.IdWlasciciel = User.Identity.GetUserId();
                db.Apartamenty.Add(ap);
                db.SaveChanges();

                List<UdogodnieniaApartamenty> udogodnieniaApartamentyOld = db.UdogodnieniaApartamenty.Where(x => x.IdApartamentu == ap.IdApartamentu).ToList();
                db.UdogodnieniaApartamenty.RemoveRange(udogodnieniaApartamentyOld);

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

            if (model.WybraneUdogodeniniaIds == null)
                model.WybraneUdogodeniniaIds = new int[] { };
            else
                model.WybraneUdogodnienia = db.Udogodnienia.ToList().Where(x => model.WybraneUdogodeniniaIds.Contains(x.IdUdogodnienia)).ToList();

            return View(model);
        }

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

            ApartamentyDisplayViewModel ap = new ApartamentyDisplayViewModel(apartament);

            return View(ap);
        }

        // POST: Apartamenty/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Apartamenty apartament = db.Apartamenty.Find(id);
            db.Apartamenty.Remove(apartament);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [ChildActionOnly]
        public ActionResult ApartamentyLista(ApartamentyFilterViewModel filtr)
        {
            var predicate1 = PredicateBuilder.New<Apartamenty>(true);

            if (!String.IsNullOrEmpty(filtr.Miasto)) { predicate1 = predicate1.And(a => a.Miasto == filtr.Miasto); }
            if (filtr.CenaOd.HasValue && filtr.CenaOd != 0) { predicate1 = predicate1.And(a => a.Cena >= filtr.CenaOd); }
            if (filtr.CenaDo.HasValue && filtr.CenaDo != 0) { predicate1 = predicate1.And(a => a.Cena <= filtr.CenaDo); }
            if (filtr.IleOsob.HasValue && filtr.IleOsob != 0) { predicate1 = predicate1.And(a => a.IloscOsob == filtr.IleOsob); }

            var predicate2 = PredicateBuilder.New<Apartamenty>(true);
            predicate2 = predicate2.And(a => a.Wizyty != null && !a.Wizyty.Any(w => !(w.DataOd > filtr.DataDo || w.DataDo < filtr.DataOd)));

            if (filtr.WybraneUdogodeniniaIds != null)
                foreach (var item in filtr.WybraneUdogodeniniaIds)
                {
                    predicate2 = predicate2.And(a => a.UdogodnieniaApartamenty != null && a.UdogodnieniaApartamenty.Any(x => x.IdUdogodnienia == item));
                }

            var result = db.Apartamenty.Where(predicate1)
                .Include("UdogodnieniaApartamenty.Udogodnienie").ToList()
                .Where(predicate2)
                .Select(a => new ApartamentyDisplayViewModel(a));

            if (result.Any())
            {
                result = result.OrderBy(x => x.Ocena).ThenBy(y => y.Nazwa).ToList();
            }

            return PartialView("_ApartamentyLista", result);
        }
    }
}
