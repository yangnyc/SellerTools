using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLDBApp.Data;
using SQLDBApp.Models.DataItems;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    /// <summary>
    /// Controller for managing product inventory.
    /// Provides CRUD operations for viewing and managing product data.
    /// </summary>
    public class InventoryController : Controller
    {
        /// <summary>
        /// Database context for accessing product data.
        /// </summary>
        private readonly DevSqlDBContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryController"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public InventoryController(DevSqlDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the inventory index page with a list of products.
        /// </summary>
        /// <returns>The index view with product list.</returns>
        [Route("[controller]/[action]")]
        public async Task<IActionResult> Index()
        {
            List<DataItemProduct> dataItemProduct = await _context.DataItemProduct.Where(x => x.Id < 10).ToListAsync();
            return View(dataItemProduct);
        }

        /// <summary>
        /// Displays detailed information for a specific product.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>The details view, or NotFound if product doesn't exist.</returns>
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

        /// <summary>
        /// Displays the product creation form.
        /// </summary>
        /// <returns>The create view.</returns>
        // GET: Items/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Creates a new product in the database.
        /// </summary>
        /// <param name="dataItemProduct">The product data to create.</param>
        /// <returns>Redirects to index on success, or returns create view with errors.</returns>
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

        /// <summary>
        /// Displays the product edit form.
        /// </summary>
        /// <param name="id">The product identifier to edit.</param>
        /// <returns>The edit view, or NotFound if product doesn't exist.</returns>
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
            if (dataItemProduct != null)
            {
                _context.DataItemProduct.Remove(dataItemProduct);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DataItemProductExists(long id)
        {
            return _context.DataItemProduct.Any(e => e.Id == id);
        }
    }
}
