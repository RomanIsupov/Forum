using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend6.Models.ViewModels.ForumViewModels
{
    public class ForumEditModel
    {
        [Required]
        public String Name { get; set; }

        public String Description { get; set; }
    }
}
