using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    public class UsersPaginationQuery
    {
        public UsersPaginationQuery()
        {
            PageNumber = 1;
            PageSize = 50;
        }
        public UsersPaginationQuery(int pageNumber, int pageSize)
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
