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
    public class ForumTopicsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;

        public ForumTopicsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            this.context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        }

        // GET: ForumTopics/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics
                .Include(f => f.Creator)
                .Include(f => f.Forum)
                .Include(f => f.Messages)
                .ThenInclude(m => m.Creator)
                .Include(f => f.Messages)
                .ThenInclude(m => m.MessageAttachments)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (topic == null)
            {
                return this.NotFound();
            }

            return this.View(topic);
        }

        // GET: ForumTopics/Create
        public async Task<IActionResult> Create(Guid? forumId)
        {
            if (forumId == null || !this.HttpContext.User.Identity.IsAuthenticated)
            {
                return this.NotFound();
            }

            var forum = await this.context.Forums
                .SingleOrDefaultAsync(f => f.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            this.ViewBag.Forum = forum;
            return this.View(new ForumTopicCreateModel());
        }

        // POST: ForumTopics/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? forumId, ForumTopicCreateModel model)
        {
            var user = await this.userManager.GetUserAsync(this.HttpContext.User);
            if (forumId == null || !this.HttpContext.User.Identity.IsAuthenticated)
            {
                return this.NotFound();
            }

            if (this.context.ForumTopics.Any(x => x.ForumId == forumId && x.Name == model.Name))
            {
                this.ModelState.AddModelError("", "Topic with the same name already exists in this forum. Please specify another topic name.");
            }

            var forum = await this.context.Forums
                .Include(f => f.Topics).ThenInclude(t => t.Creator)
                .SingleOrDefaultAsync(x => x.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid && user != null)
            {
                var forumTopic = new ForumTopic
                {
                    CreatorId = user.Id,
                    Name = model.Name,
                    ForumId = forum.Id,
                    Created = DateTime.UtcNow
                };

                this.context.ForumTopics.Add(forumTopic);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Details", "Forums", new { id = forum.Id });
            }

            return this.View(model);
        }

        // GET: ForumTopics/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics
                .SingleOrDefaultAsync(m => m.Id == id);

            if (topic == null || !this.userPermissions.CanEditForumTopic(topic))
            {
                return this.NotFound();
            }

            var model = new ForumTopicEditModel
            {
                Name = topic.Name
            };

            this.ViewBag.topicId = topic.Id;
            return this.View(model);
        }

        // POST: ForumTopics/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, ForumTopicEditModel model)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics
                .SingleOrDefaultAsync(x => x.Id == id);

            if (topic == null || !this.userPermissions.CanEditForumTopic(topic))
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                topic.Name = model.Name;
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Details", new { id = topic.Id });
            }

            this.ViewBag.topicId = topic.Id;
            return this.View(model);
        }

        // GET: ForumTopics/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics
                .Include(f => f.Creator)
                .Include(f => f.Forum)
                .Include(f => f.Messages)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (topic == null || !this.userPermissions.CanEditForumTopic(topic))
            {
                return this.NotFound();
            }

            this.ViewBag.topicId = topic.Id;
            return this.View(topic);
        }

        // POST: ForumTopics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics
                .SingleOrDefaultAsync(m => m.Id == id);

            if (topic == null || !this.userPermissions.CanEditForumTopic(topic))
            {
                return this.NotFound();
            }

            this.ViewBag.topicId = topic.Id;
            this.context.ForumTopics.Remove(topic);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Details", "Forums", new { id = topic.ForumId });
        }
    }
}
