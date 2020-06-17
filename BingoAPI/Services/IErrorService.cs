using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IErrorService
    {
        public Task<bool> AddErrorAsync(ErrorLog errorLog);
    }
}
