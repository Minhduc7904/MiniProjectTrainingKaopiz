using AiService.Application.Common.Errors;
using AiService.Application.UseCases.Providers.GetList;

namespace AiService.UnitTests.Application.Providers;

public sealed class GetAiProvidersQueryTests
{
    [Test]
    public void CreateAppliesDefaultsAndNormalizesFilters()
    {
        var query = GetAiProvidersQuery.Create(" active ", "gemini", "  main ", null, null);

        Assert.Multiple(() =>
        {
            Assert.That(query.Status, Is.EqualTo("ACTIVE"));
            Assert.That(query.ProviderType, Is.EqualTo("GEMINI"));
            Assert.That(query.Search, Is.EqualTo("main"));
            Assert.That(query.Page, Is.EqualTo(1));
            Assert.That(query.PageSize, Is.EqualTo(20));
        });
    }

    [TestCase("DISABLED", null, 1, 20, "status")]
    [TestCase(null, "SKYNET", 1, 20, "providerType")]
    [TestCase(null, null, 0, 20, "page")]
    [TestCase(null, null, 1, 101, "pageSize")]
    public void CreateRejectsInvalidValues(string? status, string? providerType, int page, int pageSize, string expectedField)
    {
        var exception = Assert.Throws<AiApplicationException>(() => GetAiProvidersQuery.Create(status, providerType, null, page, pageSize));

        Assert.That(exception!.Details!.Select(detail => detail.Field), Does.Contain(expectedField));
    }
}
