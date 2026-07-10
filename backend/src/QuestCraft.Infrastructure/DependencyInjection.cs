using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Infrastructure.CodeExecution;
using QuestCraft.Infrastructure.Excel;
using QuestCraft.Infrastructure.Identity;
using QuestCraft.Infrastructure.Persistence;

namespace QuestCraft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ICodeExecutionEngine, SubprocessCodeExecutionEngine>();
        services.AddSingleton<IExcelReader, ClosedXmlExcelReader>();
        services.AddSingleton<IExcelExportService, ClosedXmlExportService>();

        return services;
    }
}
