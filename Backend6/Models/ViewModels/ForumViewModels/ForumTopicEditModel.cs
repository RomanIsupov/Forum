using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend6.Models.ViewModels.ForumViewModels
{
    public class ForumTopicEditModel
    {
        [Required]
        public String Name { get; set; }
    }
}
