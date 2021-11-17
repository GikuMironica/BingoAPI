using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BingoAPI.Models
{
    public class BugScreenshot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Url { get; set; }
        public int BugId { get; set; }
        public Bug Bug { get; set; }
    }
}
