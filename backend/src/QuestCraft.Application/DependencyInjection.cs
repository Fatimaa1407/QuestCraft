using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using QuestCraft.Application.Common.Behaviors;
using QuestCraft.Application.Features.Gamification;

namespace QuestCraft.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditLoggingBehavior<,>));

        services.AddScoped<IAchievementEvaluator, AchievementEvaluator>();
        services.AddScoped<IDailyQuestService, DailyQuestService>();
        services.AddScoped<IContentCompletionService, ContentCompletionService>();

        return services;
    }
}
