using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    public class UserVoucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int DrinkVoucherId { get; set; }
        public DrinkVoucher DrinkVoucher { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
