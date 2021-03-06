using BingoAPI.Data;
using BingoAPI.Models;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class ErrorService : IErrorService
    {
        private readonly ErrorDataContext _context;

        public ErrorService(ErrorDataContext context)
        {
            this._context = context;
        }

        public async Task<bool> AddErrorAsync(ErrorLog errorLog)
        {
            await _context.AddAsync(errorLog);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
