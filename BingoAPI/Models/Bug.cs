using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BingoAPI.Models
{
    [Table("Bugs")]
    public class Bug
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Int64 Timestamp { get; set; }

        public string ReporterId { get; set; }

        public AppUser Reporter { get; set; }

        public List<BugScreenshot> BugScreenshots { get; set; }
    } 
}
