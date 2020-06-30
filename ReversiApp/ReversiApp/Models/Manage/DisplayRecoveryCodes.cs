using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiApp.Models.Manage
{
    public class DisplayRecoveryCodes
    {
        [Required]
        public IEnumerable<string> Codes { get; set; }
    }
}
