using AccountService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccountService.Infrastructure;

/// <summary>
/// Design-time factory: enables generating/applying migrations with
/// 'dotnet ef' without booting the API (standard pattern).
/// </summary>
public sealed class AccountDbContextFactory : IDesignTimeDbContextFactory<AccountDbContext>
{
    public AccountDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AccountDbContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=bank_accounts;Username=admin;Password=adminpassword")
            .Options;

        return new AccountDbContext(options);
    }
}