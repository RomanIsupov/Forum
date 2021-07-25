using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Backend6.Models.ViewModels.ForumViewModels
{
    public class ForumMessageAttachmentCreateModel
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
