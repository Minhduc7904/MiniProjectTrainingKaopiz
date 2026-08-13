using Microsoft.Extensions.DependencyInjection;
using StudentService.Application.Features.Students.GetById;
using StudentService.Application.Features.Students.GetList;

namespace StudentService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStudentApplication(
        this IServiceCollection services)
    {
        services.AddScoped<GetStudentByIdHandler>();
        services.AddScoped<GetStudentsHandler>();
        return services;
    }
}
