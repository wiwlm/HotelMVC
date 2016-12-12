using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMVC.Models
{
    public class ApartamentyEditViewModel
    {
        public Apartamenty Apartament { get; set; }
        public List<Udogodnienia> WszystkieUdogodnienia { get; set; }
        public List<Udogodnienia> WybraneUdogodnienia { get; set; }
        public int[] WybraneUdogodeniniaIds { get; set; }
    }

    public class ApartamentyFilterViewModel
    {
        public string Miasto { get; set; }

        [Display(Name = "Udogodnienia")]
        public int[] WybraneUdogodeniniaIds { get; set; }

        [Display(Name = "Ilość osób")]
        public int? IleOsob { get; set; }

        [Display(Name = "Cena od")]
        public decimal? CenaOd { get; set; }
        [Display(Name = "Cena do")]
        public decimal? CenaDo { get; set; }

        [Display(Name = "Data od")]
        [DataType(DataType.Date)]
        public DateTime DataOd { get; set; }
        [Display(Name = "Data do")]
        [DataType(DataType.Date)]
        public DateTime DataDo { get; set; }
    }

    public class ApartamentyDisplayViewModel : Apartamenty
    {
        public ApartamentyDisplayViewModel()
        { }

        public ApartamentyDisplayViewModel(Apartamenty ap)
        {
            this.IdApartamentu = ap.IdApartamentu;
            this.Cena = ap.Cena;
            this.IdWlasciciel = ap.IdWlasciciel;
            this.IloscOsob = ap.IloscOsob;
            this.KodPocztowy = ap.KodPocztowy;
            this.Miasto = ap.Miasto;
            this.Nazwa = ap.Nazwa;
            this.Opis = ap.Opis;
            this.UdogodnieniaApartamenty = ap.UdogodnieniaApartamenty;
            this.Ulica = ap.Ulica;
            this.Wizyty = ap.Wizyty;
        }

        [Display(Name = "Właściciel")]
        public string WlascicielImieNazwisko
        {
            get
            {
                var store = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var userManager = new UserManager<ApplicationUser>(store);
                ApplicationUser user = userManager.FindById(this.IdWlasciciel);

                if (user == null) return "";

                return user.Name + " " + user.Surname;
            }
        }

        public string Udogodnienia
        {
            get
            {
                if (this.UdogodnieniaApartamenty == null)
                    return String.Empty;
                else
                    return String.Join(", ", this.UdogodnieniaApartamenty.Select(u => u.Udogodnienie.Nazwa));
            }
        }

        [Display(Name = "Ilość wizyt")]
        public int IloscWizyt
        {
            get
            {
                if (this.Wizyty == null)
                    return 0;
                else
                    return this.Wizyty.Count(w => w.DataDo < DateTime.Today && w.Potwierdzona == true);
            }
        }

        [DisplayFormat(DataFormatString = "{0:N1}")]
        public decimal Ocena
        {
            get
            {
                if (this.Wizyty == null || !this.Wizyty.Any(w => w.Ocena != 0))
                    return 0M;
                else
                    return this.Wizyty.Where(w => w.Ocena != 0).Average(u => (decimal)u.Ocena);
            }
        }
    }

    public class ApartamentyReservationViewModel : ApartamentyDisplayViewModel
    {
        public ApartamentyReservationViewModel() { }

        public ApartamentyReservationViewModel(Apartamenty ap) : base(ap) { }

        [Display(Name = "Data od")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yy}")]
        public DateTime DataOd { get; set; }

        [Display(Name = "Data do")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yy}")]
        public DateTime DataDo { get; set; }
    }

    public class WizytyDisplayViewModel : Wizyty
    {
        public WizytyDisplayViewModel()
        { }

        public WizytyDisplayViewModel(Wizyty w)
        {
            this.Apartament = w.Apartament;
            this.DataDo = w.DataDo;
            this.DataOd = w.DataOd;
            this.DataRezerwacji = w.DataRezerwacji;
            this.DataWplaty = w.DataWplaty;
            this.DataKomentarz = w.DataKomentarz;
            this.DataOdpowiedz = w.DataOdpowiedz;
            this.IdApartamentu = w.IdApartamentu;
            this.IdKlient = w.IdKlient;
            this.IdWizyty = w.IdWizyty;
            this.Komentarz = w.Komentarz;
            this.Ocena = w.Ocena;
            this.Odpowiedz = w.Odpowiedz;
            this.Potwierdzona = w.Potwierdzona;
        }

        [Display(Name = "Klient")]
        public string KlientImieNazwisko
        {
            get
            {
                var store = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var userManager = new UserManager<ApplicationUser>(store);
                ApplicationUser user = userManager.FindById(this.IdKlient);
                return user.Name + " " + user.Surname;
            }
        }

        [Display(Name = "Potwierdzona")]
        public string PotwierdzonaString
        {
            get
            {
                if (this.Potwierdzona == true) return "Tak";
                else if (this.Potwierdzona == false) return "Nie";
                else  return "Oczekuje";
            }
        }
    }
}