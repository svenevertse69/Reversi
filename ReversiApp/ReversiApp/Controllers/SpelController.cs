using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ReversiApp.DAL;
using ReversiApp.Models;

namespace ReversiApp.Controllers
{
    public class SpelController : Controller
    {
        private readonly ReversiContext _context;
        private readonly UserManager<Speler> _userManager;

        public SpelController(ReversiContext context, UserManager<Speler> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Spel
        public async Task<IActionResult> Index()
        {

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var result = await _context.SpelerSpel.AnyAsync(sc => sc.SpelerId == user.Id);
            var spel = from ss in _context.SpelerSpel
                       where ss.SpelerId == user.Id
                       select ss.SpelID;

            if(!result)
            {
                return View(await _context.Spel.
                    Include(s => s.SpelerSpel)
                    .ThenInclude(ss => ss.Speler).ToListAsync());
            }
            else
            {
                return RedirectToAction("Play", new { spel});
            }
            
        }

        public async Task<IActionResult> SearchGame()
        {
            return View(await _context.Spel.
                Include(s => s.SpelerSpel)
                .ThenInclude(ss => ss.Speler).ToListAsync());
        }

        // GET: Spel/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spel = await _context.Spel
                .FirstOrDefaultAsync(m => m.SpelID == id);
            if (spel == null)
            {
                return NotFound();
            }

            return View(spel);
        }

        // GET: Spel/Create
        public IActionResult Create()
        {

            return View();
        }

        // POST: Spel/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpelID,Omschrijving")] Spel spel)
        {
            
            if (ModelState.IsValid)
            {
                _context.Add(spel);
                await _context.SaveChangesAsync();

                var user = await _userManager.GetUserAsync(HttpContext.User);

                _context.Add(new SpelerSpel() { SpelID = spel.SpelID, SpelerId = user.Id, Kleur = Kleur.Wit });

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }


            return View(spel);
        }

        // GET: Spel/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spel = await _context.Spel.FindAsync(id);
            if (spel == null)
            {
                return NotFound();
            }
            return View(spel);
        }

        // POST: Spel/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SpelID,Omschrijving")] Spel spel)
        {
            if (id != spel.SpelID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(spel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpelExists(spel.SpelID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(spel);
        }


        // POST: Spel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var spel = await _context.Spel.FindAsync(id);
            _context.Spel.Remove(spel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Join (int id)
        {
            var s = await _context.Spel.FindAsync(id);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var check = 0;
            var color = Kleur.Zwart;

            var spel = from ss in _context.SpelerSpel
                       where ss.SpelerId == user.Id
                       select ss.SpelID;

            foreach (var item in _context.SpelerSpel)
            {
                if(item.SpelID == id)
                {
                    check++;
                    color = item.Kleur;
                }
            }

            if(check < 2)
            {

                
                color = color == Kleur.Wit ? Kleur.Zwart : Kleur.Wit;

                _context.Add(new SpelerSpel() { SpelID = s.SpelID, SpelerId = user.Id, Kleur = color });
                await _context.SaveChangesAsync();
                return RedirectToAction("Play", new { spel });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }


        }

        public async Task<IActionResult> Leave(int id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var spel = await _context.Spel.FindAsync(id);
            var toRemove = await _context.SpelerSpel.FindAsync(user.Id, spel.SpelID);

            _context.SpelerSpel.Remove(toRemove);

            await _context.SaveChangesAsync();

            var hasEntry = await _context.SpelerSpel.AnyAsync(ss => ss.SpelID == spel.SpelID);

            if (!hasEntry)
            {
                _context.Spel.Remove(spel);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
            
        }

        public ActionResult Play(int spel)
        {
            ViewBag.Message = spel;
            return View(_context.Spel
                .Include(s => s.SpelerSpel)
                    .ThenInclude(ss => ss.Speler).ToList());
        }


        private bool SpelExists(int id)
        {
            return _context.Spel.Any(e => e.SpelID == id);
        }
    }
}
