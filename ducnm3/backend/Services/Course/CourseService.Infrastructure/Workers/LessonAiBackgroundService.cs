// File: backend/Services/Course/CourseService.Infrastructure/Workers/LessonAiBackgroundService.cs
// Mục đích: Worker nền của Course Service: liên tục nhận việc trích xuất tài liệu và job AI của Lesson rồi xử lý.

using CourseService.Application.UseCases.LessonAi.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CourseService.Infrastructure.Workers;

public sealed class LessonAiBackgroundService(IServiceScopeFactory scopeFactory, ILogger<LessonAiBackgroundService> logger) : BackgroundService
{
    /// <summary>Số vòng lặp xử lý song song; mỗi vòng xử lý tuần tự từng việc.</summary>
    public const int WorkerCount = 2;

    public static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(2);
    public static readonly TimeSpan ErrorDelay = TimeSpan.FromSeconds(10);

    private static readonly Action<ILogger, Exception?> LogLoopFailed = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(3002, "LessonAiWorkerLoopFailed"),
        "Lesson AI worker loop failed; retrying shortly.");

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.WhenAll(Enumerable.Range(0, WorkerCount).Select(_ => RunLoopAsync(stoppingToken)));

    private async Task RunLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool worked;
                using (var scope = scopeFactory.CreateScope())
                {
                    worked = await scope.ServiceProvider.GetRequiredService<LessonAiWorkRunner>().RunNextAsync(stoppingToken);
                }

                if (!worked) await Task.Delay(IdleDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                LogLoopFailed(logger, exception);
                try
                {
                    await Task.Delay(ErrorDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
}
