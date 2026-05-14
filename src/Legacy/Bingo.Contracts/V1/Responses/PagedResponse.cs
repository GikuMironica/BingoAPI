using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }

        public PagedResponse()
        {
            // default
        }

        public PagedResponse(IEnumerable<T> data)
        {
            this.Data = data;
        }

        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }

        public string NextPage { get; set; }

        public string PreviousPage { get; set; }
    }
}
