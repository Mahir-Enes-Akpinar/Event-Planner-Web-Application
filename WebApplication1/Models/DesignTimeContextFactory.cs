using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApplication1.Models;

public sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<Context>
{
    public Context CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=EventPlannerDesign;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<Context>()
            .UseNpgsql(connectionString)
            .Options;
        return new Context(options);
    }
}
