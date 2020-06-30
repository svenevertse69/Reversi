using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ReversiApp.Models.Account
{
    public class UseRecoveryCode
    {
        [Required]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }
    }
}
