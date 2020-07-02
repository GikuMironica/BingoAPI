using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    public class PostsPaginationQuery
    {
        public PostsPaginationQuery()
        {
            PageNumber = 1;
            PageSize = 50;
        }
        public PostsPaginationQuery(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            if (pageSize <= 50)
                PageSize = pageSize;
            else PageSize = 50;
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}

