using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReversiApp.Models
{

    public enum Kleur 
    { 

        Geen,
        Wit,
        Zwart

    }

    public class Speler : IdentityUser
    {
     

        [Required]
        [StringLength(50, ErrorMessage = "Veld mag niet meer dan 50 tekens bevatten")]
        public string Naam { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Veld moet tussen de 8 en 50 karakters bevatten")]
        public string Wachtwoord { get; set; }
        [ScaffoldColumn(false)]
        public string Token { get; set; }
        [NotMapped]
        [ScaffoldColumn(false)]
        public Kleur Kleur { get; set; }

        public virtual ICollection<SpelerSpel> SpelerSpel { get; set; }

    }
}
