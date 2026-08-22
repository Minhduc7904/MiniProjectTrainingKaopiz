using Microsoft.Extensions.DependencyInjection;
using StudentService.Application.UseCases.Auth.Register;
using StudentService.Application.UseCases.Auth.Login;
using StudentService.Application.UseCases.Auth.GetMe;
using StudentService.Application.UseCases.Students.GetById;
using StudentService.Application.UseCases.Students.GetList;
using StudentService.Application.UseCases.Students.GetSummary;

namespace StudentService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStudentApplication(
        this IServiceCollection services)
    {
        services.AddScoped<GetStudentByIdHandler>();
        services.AddScoped<GetStudentsHandler>();
        services.AddScoped<GetStudentsSummaryHandler>();
        services.AddScoped<RegisterStudentHandler>();
        services.AddScoped<LoginStudentHandler>();
        services.AddScoped<GetCurrentStudentHandler>();
        return services;
    }
}
