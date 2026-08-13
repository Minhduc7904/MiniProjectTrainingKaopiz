using Microsoft.Extensions.DependencyInjection;
using StudentService.Application.Features.Students.GetById;

namespace StudentService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStudentApplication(
        this IServiceCollection services)
    {
        services.AddScoped<GetStudentByIdHandler>();
        return services;
    }
}
