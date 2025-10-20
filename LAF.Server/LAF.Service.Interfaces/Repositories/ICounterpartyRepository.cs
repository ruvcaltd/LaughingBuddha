using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LAF.DataAccess.Models;

namespace LAF.Service.Interfaces.Repositories
{
    public interface ICounterpartyRepository
    {
        Task<Counterparty> GetByIdAsync(int id);
        Task<Counterparty> GetByNameAsync(string name);
        Task<IEnumerable<Counterparty>> GetAllAsync();
        Task<IEnumerable<Counterparty>> FindAsync(Expression<Func<Counterparty, bool>> predicate);
        Task<Counterparty> AddAsync(Counterparty counterparty);
        Task UpdateAsync(Counterparty counterparty);
        Task DeleteAsync(int id);
        Task<IEnumerable<Counterparty>> GetActiveCounterpartiesAsync();
        Task<bool> CounterpartyExistsAsync(string name);
        Task<IEnumerable<Counterparty>> GetCounterpartiesByCreditRatingAsync(string creditRating);
    }
}