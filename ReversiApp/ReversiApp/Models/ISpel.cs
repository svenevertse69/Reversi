using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiApp.Models
{

    public interface ISpel
    {
        public int SpelID { get; set; }
        public string Omschrijving { get; set; }
        public string Token { get; set; }
        public ICollection<Speler> Spelers { get; set; }
        public Kleur[][] Bord { get; set; }
        public Kleur AandeBeurt { get; set; }
        public bool Pas();
        public bool Afgelopen();
        public Kleur OverwegendeKleur();
        public bool ZetMogelijk(int rijZet, int kolomZet);
        public bool DoeZet(int rijZet, int kolomZet);
    }
}
