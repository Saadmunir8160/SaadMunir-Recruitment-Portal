using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using RecruitmentAPI.Infrastructure.Data;
using RecruitmentAPI.Infrastructure.Repository.Command.Base;
using RecruitmentAPI.Infrastructure.Repository.Query.Base;
using RecruitmentAPI.Infrastructure.Services;
using Serilog;

namespace RecruitmentAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RecruitmentDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("RecruitmentDB"),
                b => b.MigrationsAssembly(typeof(RecruitmentDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
        services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));

        // File Storage
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Claude AI — named HttpClient so timeout/key is configured once
        services.AddHttpClient<IClaudeAiService, ClaudeAiService>(client =>
        {
            var baseUrl = configuration["ClaudeAi:BaseUrl"] ?? "https://api.anthropic.com/v1/messages";
            client.BaseAddress = new Uri(baseUrl);
            var timeout = configuration.GetValue<int>("ClaudeAi:TimeoutSeconds", 60);
            client.Timeout = TimeSpan.FromSeconds(timeout);
        });

        // CV Parsing
        services.AddScoped<ICvParsingService, CvParsingService>();

        // AI Matching
        services.AddScoped<IMatchingService, MatchingService>();

        // OCR Verification
        services.AddScoped<IOcrVerificationService, OcrVerificationService>();

        // Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.File("logs/recruitment-api-.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .CreateLogger();

        services.AddSingleton(Log.Logger);

        return services;
    }
}
