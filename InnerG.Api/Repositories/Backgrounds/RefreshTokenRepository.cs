using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InnerG.Api.Data;

namespace InnerG.Api.Repositories.Backgrounds
{
    public interface IRefreshTokenRepository
    {
        Task<int> CleanupExpiredTokensAsync(CancellationToken ct);
    }

    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CleanupExpiredTokensAsync(CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            var expiredTokens = await _context.RefreshTokens
                .Where(x => x.Expires < now || x.IsRevoked)
                .ToListAsync(ct);

            if (!expiredTokens.Any())
                return 0;

            _context.RefreshTokens.RemoveRange(expiredTokens);
            return await _context.SaveChangesAsync(ct);
        }
    }
}