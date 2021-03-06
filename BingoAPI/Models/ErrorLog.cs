using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    public class ErrorLog
    {
        [Key]
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public string Controller { get; set; }
        public string Server { get; set; }
        public string Message { get; set; }
        public string InnerMessage { get; set; }
        public string ExtraData { get; set; }
        public string Url { get; set; }
        public string ActionMethod { get; set; }
        public string UserId { get; set; }
    }
}
