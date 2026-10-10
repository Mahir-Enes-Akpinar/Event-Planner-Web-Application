using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Services;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        foreach (var roleName in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }

        if (scope.ServiceProvider.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            var context = scope.ServiceProvider.GetRequiredService<Context>();
            var unconfirmedUsers = await context.Kullanicilar
                .IgnoreQueryFilters()
                .Where(user => !user.IsDeleted && !user.EmailConfirmed)
                .ToListAsync();
            if (unconfirmedUsers.Count > 0)
            {
                foreach (var user in unconfirmedUsers)
                    user.EmailConfirmed = true;
                await context.SaveChangesAsync();
            }
        }

        // Yeni kurulumlarda kategori tablosu boş kalmasın; etkinlik oluşturma
        // ekranı en azından kullanılabilir bir başlangıç kataloğuyla açılsın.
        var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
        var defaultCategories = new[]
        {
            "Spor", "Sanat", "Eğitim", "Müzik", "Sosyal", "Teknoloji", "Doğa", "Gönüllülük"
        };
        var existingCategoryNames = await dbContext.Kategoriler
            .IgnoreQueryFilters()
            .Select(category => category.Name)
            .ToListAsync();
        var missingCategories = defaultCategories
            .Where(name => !existingCategoryNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            .Select(name => new Kategori { Name = name })
            .ToList();
        if (missingCategories.Count > 0)
        {
            dbContext.Kategoriler.AddRange(missingCategories);
            await dbContext.SaveChangesAsync();
        }

        var adminEmail = configuration["BootstrapAdmin:Email"];
        var adminPassword = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword)) return;

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new Kullanici
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                Gender = "Belirtilmedi",
                BirthDate = new DateTime(2000, 1, 1),
                Role = RoleType.Admin
            };
            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}
