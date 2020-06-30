using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReversiApp.DAL;
using ReversiApp.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReversiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReversiController : Controller
    {

        private readonly ReversiContext _context;
        private readonly UserManager<Speler> _userManager;

        public ReversiController(ReversiContext context, UserManager<Speler> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SpelDTO>> Get(int id)
        {
            var result = await _context.Spel.FindAsync(id);

            if (result != null)
            {
                SpelDTO spel = new SpelDTO
                {
                    SpelID = result.SpelID,
                    Omschrijving = result.Omschrijving,
                    Token = result.Token,
                    Bord = result.Bord,
                    AanDeBeurt = result.AandeBeurt,
                    SerializedBord = result.SerializedBord

                };
                return spel;
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<Speler>> GetUser()
        {
            var result = await _userManager.GetUserAsync(HttpContext.User);

            if(result != null)
            {
                return result;
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet("[action]/{spelerId}/{spelId}")]
        public async Task<ActionResult<Kleur>> GetKleur (string spelerId, int spelId)
        {
            var result = await _context.SpelerSpel.FindAsync(spelerId, spelId);

            if(result != null)
            {
                return result.Kleur;
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<Kleur>> Beurt(int id)
        {

            var result = await _context.Spel.FindAsync(id);
            
            if (result != null)
            {
                return result.AandeBeurt;
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<int[]>> Stats (int id)
        {
            var result = await _context.Spel.FindAsync(id);

            if(result != null)
            {
                int zwart = 0;
                int wit = 0;

                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (result.Bord[y][x] == Kleur.Wit)
                        {
                            wit++;
                        }
                        else if (result.Bord[y][x] == Kleur.Zwart)
                        {
                            zwart++;
                        }
                    }
                }

                int[] stats = { wit, zwart };
                return stats;

            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<bool>> Ready (int id)
        {
            var result = await _context.Spel.FindAsync(id);
            int counter = 0;

            foreach(var item in _context.SpelerSpel)
            {
                if(result.SpelID == item.SpelID)
                {
                    counter++;
                }
            }

            if(counter > 1)
            {
                return true;
            }
            else
            {
                return NotFound();
            }

        } 


        [HttpPut("[action]/{id}")]
        public async Task<ActionResult<Spel>> Pas (int id)
        {

            var result = await _context.Spel.FindAsync(id);

            if (result != null)
            {
                if (result.Pas())
                {
                    _context.Update(result);
                    await _context.SaveChangesAsync();
                    
                }
                return result;
            }
            else
            {
                return NotFound();
            }

        }

        [HttpPut("[action]/{id}/{posx}/{posy}")]
        public async Task<ActionResult<Spel>> Zet (int id, int posx, int posy)
        {

            var result = await _context.Spel.FindAsync(id);

            if(result != null) 
            {
                if(result.DoeZet(posy, posx)) 
                {
                    _context.Update(result);
                    await _context.SaveChangesAsync();
                }        
                return result;
            }
            else
            {
                return NotFound();
            }

        }

    }

}
