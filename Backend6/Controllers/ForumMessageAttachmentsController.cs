using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Backend6.Data;
using Backend6.Models;
using Microsoft.AspNetCore.Identity;
using Backend6.Services;
using Microsoft.AspNetCore.Hosting;
using Backend6.Models.ViewModels.ForumViewModels;
using System.IO;
using Microsoft.Net.Http.Headers;

namespace Backend6.Controllers
{
    public class ForumMessageAttachmentsController : Controller
    {
        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;
        private readonly IHostingEnvironment hostingEnvironment;

        public ForumMessageAttachmentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserPermissionsService userPermissions,
            IHostingEnvironment hostingEnvironment)   
        {
            this.context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: ForumMessageAttachments/Create
        public async Task<IActionResult> Create(Guid? messageId)
        {
            if (messageId == null)
            {
                return this.NotFound();
            }

            var message = await this.context.ForumMessages.
                SingleOrDefaultAsync(m => m.Id == messageId);

            if (message == null || !this.userPermissions.CanEditForumMessage(message))
            {
                return this.NotFound();
            }

            this.ViewBag.Message = message;
            return this.View(new ForumMessageAttachmentCreateModel());
        }

        // POST: ForumMessageAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? messageId, ForumMessageAttachmentCreateModel model)
        {
            if (messageId == null)
            {
                return this.NotFound();
            }

            var message = await this.context.ForumMessages
                .SingleOrDefaultAsync(m => m.Id == messageId);

            if (message == null || !this.userPermissions.CanEditForumMessage(message))
            {
                return this.NotFound();
            }

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Value.Trim('"'));
            var fileExt = Path.GetExtension(fileName);

            if (!ForumMessageAttachmentsController.AllowedExtensions.Contains(fileExt))
            {
                this.ModelState.AddModelError(nameof(model.File), "This file type is prohibited.");
            }

            if (this.ModelState.IsValid)
            {
                var messageAttachment = new ForumMessageAttachment
                {
                    MessageId = message.Id,
                    Created = DateTime.UtcNow,
                };

                var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "attachments/forum", messageAttachment.Id.ToString("N") + fileExt);
                messageAttachment.FilePath = $"/attachments/forum/{messageAttachment.Id:N}{fileExt}";
                messageAttachment.FileName = fileName;

                using (var fileStream = new FileStream(attachmentPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.File.CopyToAsync(fileStream);
                }

                message.Modified = DateTime.UtcNow;

                this.context.ForumMessageAttachments.Add(messageAttachment);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Details", "ForumTopics", new { id = message.TopicId });
            }

            this.ViewBag.Message = message;
            return this.View(model);
        }        

        // GET: ForumMessageAttachments/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var attachment = await this.context.ForumMessageAttachments
                .Include(f => f.Message)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (attachment == null || !this.userPermissions.CanEditForumMessage(attachment.Message))
            {
                return this.NotFound();
            }

            this.ViewBag.topicId = attachment.Message.TopicId;
            return this.View(attachment);
        }

        // POST: ForumMessageAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var attachment = await this.context.ForumMessageAttachments
                .Include(a => a.Message)
                .SingleOrDefaultAsync(a => a.Id == id);

            if (attachment == null || !this.userPermissions.CanEditForumMessage(attachment.Message))
            {
                return this.NotFound();
            }

            var attachmentPath = this.hostingEnvironment.WebRootPath + attachment.FilePath;

            attachment.Message.Modified = DateTime.UtcNow;

            this.ViewBag.topicId = attachment.Message.TopicId;
            System.IO.File.Delete(attachmentPath);
            this.context.ForumMessageAttachments.Remove(attachment);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Details", "ForumTopics", new { id = attachment.Message.TopicId });
        }
    }
}
