using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLDBApp.Data;
using SQLDBApp.Models.DataItems;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    public class InventoryController : Controller
    {
        private readonly DevSqlDBContext _context;

        [Route("[controller]/[action]")]
        public async Task<IActionResult> Index()
        {
            List<DataItemProduct> dataItemProduct = await _context.DataItemProduct.Where(x => x.Id < 10).ToListAsync();
            return View(dataItemProduct);
        }

        public InventoryController(DevSqlDBContext context)
        {
            _context = context;
        }



        // GET: Items/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dataItemProduct = await _context.DataItemProduct
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dataItemProduct == null)
            {
                return NotFound();
            }

            return View(dataItemProduct);
        }

        // GET: Items/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Url,ImgMain,ImgsGalleryHtml,Item,Model,HighlightsHtml,DetailsHtml,SpecificationsHtml,PriceBuyDef,PriceBuyCurrent,PriceSellDef,PriceSellCurrent,PriceSellMin,PriceSellMax,IsAllBackOrdered,DateLastAvail,DateOfferExp,OfferInfoHtml,UnitOfMeas,ProductOptionsHtml,FullHtml,IsCollectedFull,IsActive,DateLastUpdated")] DataItemProduct dataItemProduct)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dataItemProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dataItemProduct);
        }

        // GET: Items/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dataItemProduct = await _context.DataItemProduct.FindAsync(id);
            if (dataItemProduct == null)
            {
                return NotFound();
            }
            return View(dataItemProduct);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Title,Url,ImgMain,ImgsGalleryHtml,Item,Model,HighlightsHtml,DetailsHtml,SpecificationsHtml,PriceBuyDef,PriceBuyCurrent,PriceSellDef,PriceSellCurrent,PriceSellMin,PriceSellMax,IsAllBackOrdered,DateLastAvail,DateOfferExp,OfferInfoHtml,UnitOfMeas,ProductOptionsHtml,FullHtml,IsCollectedFull,IsActive,DateLastUpdated")] DataItemProduct dataItemProduct)
        {
            if (id != dataItemProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dataItemProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DataItemProductExists(dataItemProduct.Id))
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
            return View(dataItemProduct);
        }

        // GET: Items/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dataItemProduct = await _context.DataItemProduct
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dataItemProduct == null)
            {
                return NotFound();
            }

            return View(dataItemProduct);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var dataItemProduct = await _context.DataItemProduct.FindAsync(id);
            _context.DataItemProduct.Remove(dataItemProduct);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DataItemProductExists(long id)
        {
            return _context.DataItemProduct.Any(e => e.Id == id);
        }
    }
}
