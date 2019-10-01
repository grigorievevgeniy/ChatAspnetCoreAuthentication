using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Models
{
    public class PrivateRoom
    {
        [Key]
        public string User1Id { get; set; }
        [Key]
        public string User2Id { get; set; }
    }
}
