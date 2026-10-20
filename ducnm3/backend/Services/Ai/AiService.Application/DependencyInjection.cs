// File: backend/Services/Ai/AiService.Application/DependencyInjection.cs
// Mục đích: Đăng ký dependency injection cho use case thuộc Application layer.

using AiService.Application.UseCases.Bindings.Delete;
using AiService.Application.UseCases.Bindings.GetDetail;
using AiService.Application.UseCases.Bindings.GetList;
using AiService.Application.UseCases.Bindings.Upsert;
using AiService.Application.UseCases.Models.Create;
using AiService.Application.UseCases.Models.Delete;
using AiService.Application.UseCases.Models.GetDetail;
using AiService.Application.UseCases.Models.GetList;
using AiService.Application.UseCases.Models.Update;
using AiService.Application.UseCases.Options;
using AiService.Application.UseCases.Providers.Create;
using AiService.Application.UseCases.Providers.Delete;
using AiService.Application.UseCases.Providers.GetDetail;
using AiService.Application.UseCases.Providers.GetList;
using AiService.Application.UseCases.Providers.HealthCheck;
using AiService.Application.UseCases.Providers.Update;
using Microsoft.Extensions.DependencyInjection;

namespace AiService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAiApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateAiProviderHandler>();
        services.AddScoped<UpdateAiProviderHandler>();
        services.AddScoped<DeleteAiProviderHandler>();
        services.AddScoped<GetAiProviderDetailHandler>();
        services.AddScoped<GetAiProvidersHandler>();
        services.AddScoped<CheckAiProviderHealthHandler>();
        services.AddScoped<CreateAiModelHandler>();
        services.AddScoped<UpdateAiModelHandler>();
        services.AddScoped<DeleteAiModelHandler>();
        services.AddScoped<GetAiModelDetailHandler>();
        services.AddScoped<GetAiModelsHandler>();
        services.AddScoped<UpsertAiServiceBindingHandler>();
        services.AddScoped<DeleteAiServiceBindingHandler>();
        services.AddScoped<GetAiServiceBindingHandler>();
        services.AddScoped<GetAiServiceBindingsHandler>();
        services.AddSingleton<GetAiOptionsHandler>();
        return services;
    }
}
