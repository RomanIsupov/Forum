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
    public class ForumMessagesController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;

        public ForumMessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            this.context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        }

        // GET: ForumMessages/Create
        public async Task<IActionResult> Create(Guid? topicId)
        {
            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics
                .SingleOrDefaultAsync(x => x.Id == topicId);

            if (topic == null || !this.HttpContext.User.Identity.IsAuthenticated)
            {
                return this.NotFound();
            }

            this.ViewBag.ForumTopic = topic;
            return this.View(new ForumMessageCreateModel());
        }

        // POST: ForumMessages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? topicId, ForumMessageCreateModel model)
        {
            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics
                .SingleOrDefaultAsync(x => x.Id == topicId);

            if (topic == null || !this.HttpContext.User.Identity.IsAuthenticated)
            {
                return this.NotFound();
            }

            var user = await this.userManager.GetUserAsync(this.HttpContext.User);

            if (this.ModelState.IsValid)
            {
                var now = DateTime.UtcNow;
                var message = new ForumMessage
                {
                    TopicId = topic.Id,
                    CreatorId = user.Id,
                    Created = now,
                    Modified = now,
                    Text = model.Text
                };

                this.context.Add(message);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Details", "ForumTopics", new { id = topic.Id });
            }

            this.ViewBag.ForumTopic = topic;
            return this.View(model);
        }

        // GET: ForumMessages/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var message = await this.context.ForumMessages
                .Include(m => m.Topic)
                .SingleOrDefaultAsync(m => m.Id == id);
            
            if (message == null || !this.userPermissions.CanEditForumMessage(message))
            {
                return this.NotFound();
            }
            
            var model = new ForumMessageEditModel
            {
                Text = message.Text
            };

            this.ViewBag.ForumTopic = message.Topic;
            return this.View(model);
        }

        // POST: ForumMessages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, ForumMessageEditModel model)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var message = await this.context.ForumMessages
                .SingleOrDefaultAsync(m => m.Id == id);

            if (message == null || !this.userPermissions.CanEditForumMessage(message))
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                message.Text = model.Text;
                message.Modified = DateTime.UtcNow;
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Details", "ForumTopics", new { id = message.TopicId });
            }

            this.ViewBag.topicId = message.TopicId;
            return this.View(model);
        }

        // GET: ForumMessages/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var message = await this.context.ForumMessages
                .Include(f => f.Creator)
                .Include(f => f.Topic)
                .Include(m => m.MessageAttachments)
                .SingleOrDefaultAsync(m => m.Id == id);

            var topicId = message.TopicId;

            if (message == null || !this.userPermissions.CanEditForumMessage(message))
            {
                return this.NotFound();
            }

            this.ViewBag.topicId = message.TopicId;
            return this.View(message);
        }

        // POST: ForumMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var message = await this.context.ForumMessages
                .SingleOrDefaultAsync(m => m.Id == id);

            if (message == null || !this.userPermissions.CanEditForumMessage(message))
            {
                return this.NotFound();
            }

            this.ViewBag.topicId = message.TopicId;
            this.context.ForumMessages.Remove(message);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Details", "ForumTopics", new { id = message.TopicId });
        }
    }
}
