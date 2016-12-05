using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace HotelMVC.Models
{
    public class EntityContext : DbContext
    {
        public EntityContext() : base("DefaultConnection")
        {
        }

        public DbSet<Apartamenty> Apartamenty { get; set; }

        public DbSet<Udogodnienia> Udogodnienia { get; set; }

        public DbSet<UdogodnieniaApartamenty> UdogodnieniaApartamenty { get; set; }

        public DbSet<Wizyty> Wizyty { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId);
            modelBuilder.Entity<IdentityRole>().HasKey<string>(r => r.Id);
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId });

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }

    public class EntityInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<EntityContext>
    {
        protected override void Seed(EntityContext context)
        {
            var udogodnieniaOld = context.Udogodnienia.ToList();
            context.Udogodnienia.RemoveRange(udogodnieniaOld);

            var udogodnienia = new List<Udogodnienia>
            {
            new Udogodnienia{Nazwa="Basen"},
            new Udogodnienia{Nazwa="WiFi"},
            new Udogodnienia{Nazwa="Sauna"},
            new Udogodnienia{Nazwa="Bilard"},
            new Udogodnienia{Nazwa="Kręgle"},
            new Udogodnienia{Nazwa="Spa"},
            new Udogodnienia{Nazwa="Darmowy parking"},
            new Udogodnienia{Nazwa="Darmowe śniadanie"}
            };

            context.Udogodnienia.AddRange(udogodnienia);
            context.SaveChanges();
        }
    }

    public class Apartamenty
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdApartamentu { get; set; }

        public string Nazwa { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Cena { get; set; }

        [Display(Name = "Ilość osób")]
        public int IloscOsob { get; set; }

        public string Opis { get; set; }

        public string Ulica { get; set; }

        public string Miasto { get; set; }

        [Display(Name = "Kod pocztowy")]
        public string KodPocztowy { get; set; }

        [MaxLength(128)]
        public string IdWlasciciel { get; set; }

        [Display(Name = "Udogodnienia")]
        public ICollection<UdogodnieniaApartamenty> UdogodnieniaApartamenty { get; set; }

        public ICollection<Wizyty> Wizyty { get; set; }
    }

    public class Udogodnienia
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUdogodnienia { get; set; }

        public string Nazwa { get; set; }
    }

    public class UdogodnieniaApartamenty
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUdogodnieniaApartamentu { get; set; }

        [ForeignKey("Apartament")]
        public int IdApartamentu { get; set; }

        [ForeignKey("Udogodnienie")]
        public int IdUdogodnienia { get; set; }

        public Apartamenty Apartament { get; set; }

        public Udogodnienia Udogodnienie { get; set; }
    }

    public class Wizyty
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdWizyty { get; set; }

        [ForeignKey("Apartament")]
        public int IdApartamentu { get; set; }

        [Display(Name = "Data rezerwacji")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime DataRezerwacji { get; set; }

        [Display(Name = "Data wpłaty")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? DataWplaty { get; set; }

        [Display(Name = "Data od")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime DataOd { get; set; }

        [Display(Name = "Data do")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime DataDo { get; set; }

        public string Komentarz { get; set; }

        public int Ocena { get; set; }

        [Display(Name = "Odpowiedź")]
        public string Odpowiedz { get; set; }

        [MaxLength(128)]
        public string IdKlient { get; set; }

        public Apartamenty Apartament { get; set; }
    }

}