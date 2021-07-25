using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend6.Models
{
    public class ForumMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public String Text { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        [Required]
        public String CreatorId { get; set; }

        public ApplicationUser Creator { get; set; }

        public Guid TopicId { get; set; }

        public ForumTopic Topic { get; set; }

        public ICollection<ForumMessageAttachment> MessageAttachments { get; set; }
    }
}
