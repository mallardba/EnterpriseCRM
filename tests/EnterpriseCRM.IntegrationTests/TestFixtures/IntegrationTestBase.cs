using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using EnterpriseCRM.Infrastructure.Data;
using System.Text.Json;

namespace EnterpriseCRM.IntegrationTests.TestFixtures;

public abstract class IntegrationTestBase
{
    // TODO: Add WebApplicationFactory integration once Program class is accessible
    // This base class will be implemented when we resolve the Program class accessibility issue
    
    protected T? DeserializeResponse<T>(string content)
    {
        if (string.IsNullOrEmpty(content))
            return default;

        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    protected T DeserializeResponseOrThrow<T>(string content)
    {
        if (string.IsNullOrEmpty(content))
            throw new InvalidOperationException("Response content is empty");

        var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null)
            throw new InvalidOperationException("Failed to deserialize response");

        return result;
    }
}
