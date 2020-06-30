using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiApp.Models
{
    public class SpelDTO
    {

        public int SpelID { get; set; }
        public string Omschrijving { get; set; }
        public string Token { get; set; }
        public Kleur [][] Bord { get; set; }
        public Kleur AanDeBeurt { get; set; }
        public string SerializedBord { get => JsonConvert.SerializeObject(Bord); set => Bord = JsonConvert.DeserializeObject<Kleur[][]>(value); }


    }
}
