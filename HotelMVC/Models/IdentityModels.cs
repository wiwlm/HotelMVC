using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace HotelMVC.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Imię")]
        [MaxLength(255)]
        public string Name { get; set; }

        [Display(Name = "Nazwisko")]
        [MaxLength(255)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Pole 'Uprawnienie' jest wymagane")]
        public int Uprawnienie { get; set; }

        [Display(Name = "Uprawnienie")]
        public string UprawnienieString
        {
            get
            {
                string rez = string.Empty;

                switch (this.Uprawnienie)
                {
                    case 1:
                        rez = "Administrator"; break;
                    case 2:
                        rez = "Właściciel"; break;
                    case 3:
                        rez = "Użytkownik"; break;
                    default:
                        break;
                }

                return rez;
            }
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<HotelMVC.Models.Apartamenty> Apartamenties { get; set; }
    }
}