using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InnerG.Api.Models;

namespace InnerG.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<AppUser?> GetUserByUsernameAsync(string username);
        public Task<bool> IsUserExistAsync(string username);

    }
}