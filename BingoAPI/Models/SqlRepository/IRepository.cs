using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface IRepository<T>
    {
        public Task<bool> AddAsync(T entity);
        public Task DeleteAsync(int Id);
        public Task<IEnumerable<T>> GetAllAsync();

        public Task UpdateAsync(T entity);

        public abstract Task<T> GetByIdAsync(int id);
    }
}
