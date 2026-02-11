using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InnerG.Api.Data;
using InnerG.Api.Repositories.Backgrounds;

namespace InnerG.Api.Services.Backgrounds
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RefreshTokenCleanupService> _logger;

        public RefreshTokenCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<RefreshTokenCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Chạy ngay khi app start
            await CleanupAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // ⏱ 24 giờ
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                await CleanupAsync(stoppingToken);
            }
        }

        private async Task CleanupAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

            var deleted = await repo.CleanupExpiredTokensAsync(ct);

            _logger.LogInformation("RefreshToken cleanup removed {Count} tokens", deleted);
        }
    }

}