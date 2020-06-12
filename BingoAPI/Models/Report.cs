using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Int64 Timestamp { get; set; }

        public string Reason { get; set; }

        public string Message { get; set; }

        public string ReporterId { get; set; }

        public AppUser Reporter { get; set; }

        public string ReportedHostId { get; set; }

        public AppUser ReportedHost { get; set; }

        public int PostId { get; set; }

        public Post Post { get; set; }
    }
}
