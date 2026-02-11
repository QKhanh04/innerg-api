using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InnerG.Api.Data;
using InnerG.Api.Models;
using InnerG.Api.Repositories.Interfaces;

namespace InnerG.Api.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;


        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<AppUser?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<bool> IsUserExistAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.UserName == username);
        }


    }
}