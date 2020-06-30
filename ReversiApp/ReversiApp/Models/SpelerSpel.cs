using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiApp.Models
{
    public class SpelerSpel
    {
        public string SpelerId { get; set; }
        public Speler Speler{ get; set; }
        public int SpelID { get; set; }
        public Spel Spel { get; set; }
        public Kleur Kleur { get; set; }
    }
}
