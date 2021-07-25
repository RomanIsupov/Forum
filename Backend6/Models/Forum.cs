using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend6.Models
{
    public class Forum
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public String Name { get; set; }

        public String Description { get; set; }

        public Guid CategoryID { get; set; }

        public ForumCategory Category { get; set; }

        public ICollection<ForumTopic> Topics { get; set; }
    }
}
