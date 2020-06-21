using BingoAPI.Data;
using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class ErrorService : IErrorService
    {
        private readonly ErrorDataContext context;

        public ErrorService(ErrorDataContext context)
        {
            this.context = context;
        }

        public async Task<bool> AddErrorAsync(ErrorLog errorLog)
        {
            await context.AddAsync(errorLog);
            return await context.SaveChangesAsync() > 0;
        }
    }
}
