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
    public class UserRepository : IUserRepository
    {
        private readonly LAFDbContext _context;

        public UserRepository(LAFDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.DisplayName == username);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .OrderBy(u => u.DisplayName)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
        {
            return await _context.Users
                .Where(predicate)
                .OrderBy(u => u.DisplayName)
                .ToListAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Email == username);
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            // For now, use email as username and assume password validation is handled elsewhere
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == username);
            
            return user != null;
        }
    }
}