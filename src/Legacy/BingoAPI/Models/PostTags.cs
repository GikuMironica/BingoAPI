using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    public class PostTags
    {
        public virtual Tag Tag { get; set; }

        public int TagId { get; set; }

        public virtual Post Post { get; set; }

        public int PostId { get; set; }
    }
}
