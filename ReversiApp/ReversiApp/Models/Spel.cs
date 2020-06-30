using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiApp.Models
{
    public class Spel : ISpel
    {

        private List<bool> richting = new List<bool>();

        public int SpelID { get; set; }
        public string Omschrijving { get; set; }
        [ScaffoldColumn(false)]
        public string Token { get; set; }
        public virtual ICollection<Speler> Spelers { get; set; }
        [NotMapped]
        [ScaffoldColumn(false)]
        public Kleur[][] Bord { get; set; }

        [ScaffoldColumn(false)]
        public Kleur AandeBeurt { get; set; }

        public virtual ICollection<SpelerSpel> SpelerSpel { get; set; }

        [Required]
        public string SerializedBord { get => JsonConvert.SerializeObject(Bord); set => Bord = JsonConvert.DeserializeObject<Kleur[][]>(value); }

        public Spel()
        {

            AandeBeurt = Kleur.Wit;

            Bord = new Kleur[][]
            {

                new Kleur[]{ Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen },
                new Kleur[]{ Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen },
                new Kleur[]{ Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen },
                new Kleur[]{ Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen },
                new Kleur[]{ Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen },
                new Kleur[]{ Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen },
                new Kleur[]{ Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen },
                new Kleur[]{ Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen, Kleur.Geen }

            };

            Bord[3][3] = Kleur.Wit;
            Bord[3][4] = Kleur.Zwart;
            Bord[4][3] = Kleur.Zwart;
            Bord[4][4] = Kleur.Wit;

            richting = InitRichtingen();

        }

        public bool Afgelopen()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (ZetMogelijk(i, j))
                    {
                        ResetRichtingen();
                        return false;
                    }
                }
            }
            return true;
        }

        public bool DoeZet(int rijZet, int kolomZet)
        {

            if (!ZetMogelijk(rijZet, kolomZet))
                return false;

            if (richting[0] && CheckRichting(rijZet, kolomZet, -1, 0, true))
                richting[0] = false;
            if (richting[1] && CheckRichting(rijZet, kolomZet, -1, 1, true))
                richting[1] = false;
            if (richting[2] && CheckRichting(rijZet, kolomZet, 0, 1, true))
                richting[2] = false;
            if (richting[3] && CheckRichting(rijZet, kolomZet, 1, 1, true))
                richting[3] = false;
            if (richting[4] && CheckRichting(rijZet, kolomZet, 1, 0, true))
                richting[4] = false;
            if (richting[5] && CheckRichting(rijZet, kolomZet, 1, -1, true))
                richting[5] = false;
            if (richting[6] && CheckRichting(rijZet, kolomZet, 0, -1, true))
                richting[6] = false;
            if (richting[7] && CheckRichting(rijZet, kolomZet, -1, -1, true))
                richting[7] = false;
            Bord[rijZet][kolomZet] = AandeBeurt;
            AandeBeurt = AandeBeurt == Kleur.Wit ? Kleur.Zwart : Kleur.Wit;
            return true;
        }

        public Kleur OverwegendeKleur()
        {
            int zwart = 0;
            int wit = 0;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (Bord[y][x] == Kleur.Wit)
                    {
                        wit++;
                    }
                    else if (Bord[y][x] == Kleur.Zwart)
                    {
                        zwart++;
                    }
                }
            }

            if (zwart < wit)
            {
                return Kleur.Wit;
            }
            else if (zwart > wit)
            {
                return Kleur.Zwart;
            }
            return Kleur.Geen;
        }

        public bool Pas()
        {
            AandeBeurt = AandeBeurt == Kleur.Wit ? Kleur.Zwart : Kleur.Wit;
            return true;
        }

        public bool ZetMogelijk(int rijZet, int kolomZet)
        {
            if ((rijZet < 0 || rijZet > 7 || kolomZet < 0 || kolomZet > 7) || Bord[rijZet][kolomZet] != Kleur.Geen)
            {
                return false;
            }

            Kleur tegenstander = AandeBeurt == Kleur.Wit ? Kleur.Zwart : Kleur.Wit;

            List<bool> richting = InitRichtingenChecks(rijZet, kolomZet, tegenstander);


            for (int i = 0; i < richting.Count; i++)
            {
                if (richting[i])
                {
                    this.richting[i] = true;
                }
            }

            for (int i = 0; i < this.richting.Count; i++)
            {
                if (this.richting[i])
                {
                    return true;
                }
            }

            return false;
        }

        private void ResetRichtingen()
        {
            richting[0] = false;
            richting[1] = false;
            richting[2] = false;
            richting[3] = false;
            richting[4] = false;
            richting[5] = false;
            richting[6] = false;
            richting[7] = false;
        }


        private bool CheckRichting(int rij, int kolom, int y, int x, bool flip = false)
        {
            if (flip)
            {
                Bord[rij][kolom] = AandeBeurt;
                rij += y;
                kolom += x;
            }
            while (rij >= 0 && rij < 8 && kolom >= 0 && kolom < 8)
            {
                if (Bord[rij][kolom] == Kleur.Geen)
                {
                    return false;
                }

                else if (Bord[rij][kolom] == AandeBeurt)
                {
                    return true;
                }

                else if (flip)
                {
                    Bord[rij][kolom] = Bord[rij][kolom] == Kleur.Wit ? Kleur.Zwart : Kleur.Wit;
                }

                rij += y;
                kolom += x;
            }
            return false;
        }

        private List<bool> InitRichtingenChecks(int y, int x, Kleur tegenstander)
        {

            List<bool> richtingen = new List<bool>();

            richtingen.Add(y - 1 > 0 && Bord[y - 1][x] == tegenstander && CheckRichting(y - 2, x, -1, 0)); //richting[1]ord
            richtingen.Add(y - 1 > 0 && x + 1 < 8 && Bord[y - 1][x + 1] == tegenstander && CheckRichting(y - 2, x + 2, -1, 1)); //richting[1]ord-Oost
            richtingen.Add(x + 1 < 8 && Bord[y][x + 1] == tegenstander && CheckRichting(y, x + 2, 0, 1)); //Oost
            richtingen.Add(y + 1 < 8 && x + 1 < 8 && Bord[y + 1][x + 1] == tegenstander && CheckRichting(y + 2, x + 2, 1, 1)); //Zuid-Oost
            richtingen.Add(y + 1 < 8 && Bord[y + 1][x] == tegenstander && CheckRichting(y + 2, x, 1, 0)); //Zuid
            richtingen.Add(y + 1 < 8 && x - 1 > 0 && Bord[y + 1][x - 1] == tegenstander && CheckRichting(y + 2, x - 2, 1, -1)); //Zuid-West
            richtingen.Add(x - 1 > 0 && Bord[y][x - 1] == tegenstander && CheckRichting(y, x - 2, 0, -1)); //West
            richtingen.Add(y - 1 > 0 && x - 1 > 0 && Bord[y - 1][x - 1] == tegenstander && CheckRichting(y - 2, x - 2, -1, -1)); //richting[1]ord-West


            return richtingen;

        }

        private List<bool> InitRichtingen()
        {

            List<bool> richtingen = new List<bool>();

            for (int i = 0; i < 8; i++)
            {
                richtingen.Add(false);
            }

            return richtingen;

        }

    }
}
