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
    public class ForumCategoriesController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;

        public ForumCategoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            this.context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        }

        // GET: ForumCategories/Create
        public IActionResult Create()
        {
            if (!this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }
            return this.View(new ForumCategoryCreateModel());
        }

        // POST: ForumCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumCategoryCreateModel model)
        {
            if (!this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }
            if (this.ModelState.IsValid)
            {
                var category = new ForumCategory
                {
                    Name = model.Name
                };

                this.context.ForumCategories.Add(category);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index", "Forums");
            }

            return this.View(model);
        }

        // GET: ForumCategories/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || !this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }

            var category = await this.context.ForumCategories
                .SingleOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return this.NotFound();
            }

            var model = new ForumCategoryEditModel
            {
                Name = category.Name
            };

            return this.View(model);
        }

        // POST: ForumCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, ForumCategoryEditModel model)
        {
            if (id == null || !this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }

            var category = await this.context.ForumCategories
                .SingleOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                category.Name = model.Name;
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index", "Forums");
            }

            return this.View(model);
        }

        // GET: ForumCategories/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || !this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }

            var category = await this.context.ForumCategories
                .SingleOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return this.NotFound();
            }

            return this.View(category);
        }

        // POST: ForumCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if (id == null || !this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return this.NotFound();
            }

            var category = await this.context.ForumCategories
                .SingleOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return this.NotFound();
            }

            this.context.ForumCategories.Remove(category);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Index", "Forums");
        }
    }
}
