using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.User
{
    public class PaginationQuery
    {
        public PaginationQuery()
        {
            PageNumber = 1;
            PageSize = 50;
        }

        public PaginationQuery(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            if (pageSize <= 50) PageSize = pageSize; else PageSize = 50;
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        [Required]
        public int Id { get; set; }
    }
}
