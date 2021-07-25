using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Backend6.Data;
using Backend6.Models;
using Backend6.Models.ViewModels.ForumViewModels;
using Microsoft.AspNetCore.Identity;
using Backend6.Services;

namespace Backend6.Controllers
{
    public class ForumsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;

        public ForumsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            this.context = context;
            this.userPermissions = userPermissions;
            this.userManager = userManager;
        }

        // GET: Forums
        public async Task<IActionResult> Index()
        {
            return this.View(await this.context.ForumCategories
                .Include(x => x.Forums)
                .ThenInclude(f => f.Topics)
                .ToListAsync());
        }

        // GET: Forums/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var forum = await this.context.Forums
                .Include(f => f.Topics)
                .ThenInclude(t => t.Messages)
                .ThenInclude(m => m.Creator)
                .Include(f => f.Topics)
                .ThenInclude(t => t.Creator)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (forum == null)
            {
                return this.NotFound();
            }

            return this.View(forum);
        }

        // GET: Forums/Create
        public async Task<IActionResult> Create(Guid? categoryId)
        {
            if (categoryId == null || !this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }

            var category = await this.context.ForumCategories
                .SingleOrDefaultAsync(m => m.Id == categoryId);

            if (category == null)
            {
                return this.NotFound();
            }

            this.ViewBag.ForumCategory = category;
            return this.View(new ForumCreateModel());
        }

        // POST: Forums/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? categoryId, ForumCreateModel model)
        {
            if (categoryId == null || !this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }

            if (this.context.Forums.Any(x => x.CategoryID == categoryId && x.Name == model.Name))
            {
                this.ModelState.AddModelError("", "Forum with the same name already exists in this forum category.");
            }

            var category = await this.context.ForumCategories
                .SingleOrDefaultAsync(x => x.Id == categoryId);

            if (category == null)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                var forum = new Forum
                {
                    Name = model.Name,
                    Description = model.Description,
                    CategoryID = category.Id
                };

                this.context.Forums.Add(forum);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index", "Forums");
            }

            return this.View(model);
        }

        // GET: Forums/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || !this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }

            var forum = await this.context.Forums
                .SingleOrDefaultAsync(m => m.Id == id);

            if (forum == null)
            {
                return this.NotFound();
            }

            var model = new ForumEditModel
            {
                Name = forum.Name,
                Description = forum.Description
            };

            return this.View(model);
        }

        // POST: Forums/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, ForumEditModel model)
        {
            if (id == null || !this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }

            var forum = await this.context.Forums
                .SingleOrDefaultAsync(m => m.Id == id);

            if (forum == null)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                forum.Name = model.Name;
                forum.Description = model.Description;
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index", "Forums");
            }

            return this.View(model);
        }

        // GET: Forums/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || !this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }

            var forum = await this.context.Forums
                .SingleOrDefaultAsync(m => m.Id == id);

            if (forum == null)
            {
                return this.NotFound();
            }

            return this.View(forum);
        }

        // POST: Forums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if (id == null || !this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }

            var forum = await this.context.Forums
                .SingleOrDefaultAsync(m => m.Id == id);

            if (forum == null)
            {
                return this.NotFound();
            }

            this.context.Forums.Remove(forum);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Index", "Forums");
        }
    }
}
