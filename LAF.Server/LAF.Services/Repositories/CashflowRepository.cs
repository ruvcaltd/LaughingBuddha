using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LAF.DataAccess.Data;
using LAF.DataAccess.Models;
using LAF.Service.Interfaces.Repositories;

namespace LAF.Services.Repositories
{
    public class CashflowRepository : ICashflowRepository
    {
        private readonly LAFDbContext _context;

        public CashflowRepository(LAFDbContext context)
        {
            _context = context;
        }

        public async Task<Cashflow> GetByIdAsync(int id)
        {
            return await _context.Cashflows
                .Include(cf => cf.CashAccount)
                .Include(cf => cf.Fund)
                .Include(cf => cf.Trade)
                .FirstOrDefaultAsync(cf => cf.Id == id);
        }

        public async Task<IEnumerable<Cashflow>> GetAllAsync()
        {
            return await _context.Cashflows
                .Include(cf => cf.CashAccount)
                .Include(cf => cf.Fund)
                .Include(cf => cf.Trade)
                .OrderByDescending(cf => cf.CashflowDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cashflow>> FindAsync(Expression<Func<Cashflow, bool>> predicate)
        {
            return await _context.Cashflows
                .Include(cf => cf.CashAccount)
                .Include(cf => cf.Fund)
                .Include(cf => cf.Trade)
                .Where(predicate)
                .OrderByDescending(cf => cf.CashflowDate)
                .ToListAsync();
        }

        public async Task<Cashflow> AddAsync(Cashflow cashflow)
        {
            _context.Cashflows.Add(cashflow);
            await _context.SaveChangesAsync();
            return cashflow;
        }

        public async Task UpdateAsync(Cashflow cashflow)
        {
            _context.Cashflows.Update(cashflow);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cashflow = await _context.Cashflows.FindAsync(id);
            if (cashflow != null)
            {
                _context.Cashflows.Remove(cashflow);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Cashflow>> GetCashflowsByAccountAsync(int cashAccountId)
        {
            return await _context.Cashflows
                .Include(cf => cf.CashAccount)
                .Include(cf => cf.Fund)
                .Include(cf => cf.Trade)
                .Where(cf => cf.CashAccountId == cashAccountId)
                .OrderByDescending(cf => cf.CashflowDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cashflow>> GetCashflowsByFundAsync(int fundId)
        {
            return await _context.Cashflows
                .Include(cf => cf.CashAccount)
                .Include(cf => cf.Fund)
                .Include(cf => cf.Trade)
                .Where(cf => cf.FundId == fundId)
                .OrderByDescending(cf => cf.CashflowDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cashflow>> GetCashflowsByDateRangeAsync(int cashAccountId, DateTime startDate, DateTime endDate)
        {
            return await _context.Cashflows
                .Include(cf => cf.CashAccount)
                .Include(cf => cf.Fund)
                .Include(cf => cf.Trade)
                .Where(cf => cf.CashAccountId == cashAccountId && cf.CashflowDate >= startDate && cf.CashflowDate <= endDate)
                .OrderByDescending(cf => cf.CashflowDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cashflow>> GetCashflowsByTradeAsync(int repoTradeId)
        {
            return await _context.Cashflows
                .Include(cf => cf.CashAccount)
                .Include(cf => cf.Fund)
                .Include(cf => cf.Trade)
                .Where(cf => cf.TradeId == repoTradeId)
                .OrderByDescending(cf => cf.CashflowDate)
                .ToListAsync();
        }

        public async Task<decimal> GetNetCashflowByAccountAsync(int cashAccountId, DateTime asOfDate)
        {
            return await _context.Cashflows
                .Where(cf => cf.CashAccountId == cashAccountId && cf.CashflowDate <= asOfDate)
                .SumAsync(cf => cf.Amount);
        }
    }
}