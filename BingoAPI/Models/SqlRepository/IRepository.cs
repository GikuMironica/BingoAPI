using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface IRepository<T>
    {
        public Task<bool> Add(T entity);
        public Task Delete(int Id);
        public Task<IEnumerable<T>> GetAll();

        public Task Update(T entity);

        public abstract Task<T> GetById(int id);
    }
}
