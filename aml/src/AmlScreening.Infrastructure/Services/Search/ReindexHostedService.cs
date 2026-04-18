using AmlScreening.Application.Interfaces;
using AmlScreening.Infrastructure.Options;
using AmlScreening.Infrastructure.Persistence;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HttpMethod = Elastic.Transport.HttpMethod;

namespace AmlScreening.Infrastructure.Services.Search;

/// <summary>
/// On startup: ensures the Elasticsearch index exists. If the indexed doc count differs
/// from the SQL count, performs a full reindex from the database. Failures are logged
/// but never crash the host (ES may be temporarily unavailable in dev).
/// </summary>
public class ReindexHostedService : IHostedService
{
    private static readonly string[] RequiredPlugins = { "analysis-icu", "analysis-phonetic" };

    private readonly IServiceProvider _services;
    private readonly ILogger<ReindexHostedService> _logger;

    public ReindexHostedService(IServiceProvider services, ILogger<ReindexHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var indexer = scope.ServiceProvider.GetRequiredService<ISanctionEntryIndexer>();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var client = scope.ServiceProvider.GetRequiredService<ElasticsearchClient>();
            var esOptions = scope.ServiceProvider.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;

            if (!await PreflightAsync(client, esOptions, cancellationToken))
            {
                return;
            }

            await indexer.EnsureIndexAsync(cancellationToken);

            var dbCount = await ctx.SanctionListEntries.AsNoTracking().LongCountAsync(cancellationToken);
            var esCount = await indexer.CountAsync(cancellationToken);

            if (dbCount == esCount)
            {
                _logger.LogInformation("ES index in sync with DB ({Count} entries)", dbCount);
                return;
            }

            _logger.LogInformation("ES index out of sync (DB={Db}, ES={Es}); reindexing...", dbCount, esCount);
            var entries = ctx.SanctionListEntries.AsNoTracking().AsAsyncEnumerable();
            await indexer.ReindexAllAsync(await ToListAsync(entries, cancellationToken), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initial Elasticsearch reindex failed; screening searches may return empty until resolved.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Verifies Elasticsearch is reachable and has the analyzers we depend on.
    /// Returns true to continue with EnsureIndex/reindex; false to abort with an actionable log message.
    /// </summary>
    private async Task<bool> PreflightAsync(
        ElasticsearchClient client,
        ElasticsearchOptions options,
        CancellationToken cancellationToken)
    {
        PingResponse ping;
        try
        {
            ping = await client.PingAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Elasticsearch is unreachable at {Url}. Start the container with " +
                "'docker compose up -d elasticsearch' (build first with 'docker compose build elasticsearch' " +
                "to install analysis-icu + analysis-phonetic plugins from docker/elasticsearch.Dockerfile).",
                options.Url);
            return false;
        }

        if (!ping.IsValidResponse)
        {
            _logger.LogError(ping.ApiCallDetails?.OriginalException,
                "Elasticsearch ping failed at {Url} (status={Status}). Start the container with " +
                "'docker compose up -d elasticsearch' (build first with 'docker compose build elasticsearch' " +
                "to install analysis-icu + analysis-phonetic plugins from docker/elasticsearch.Dockerfile). Debug: {Debug}",
                options.Url,
                ping.ApiCallDetails?.HttpStatusCode?.ToString() ?? "no-response",
                ping.DebugInformation);
            return false;
        }

        var missing = await GetMissingPluginsAsync(client, cancellationToken);
        if (missing.Count > 0)
        {
            _logger.LogError(
                "Elasticsearch is reachable but is missing required plugin(s): {Missing}. " +
                "The current container appears to be the stock elasticsearch image. " +
                "Stop it, rebuild with 'docker compose build elasticsearch' (which installs " +
                "analysis-icu + analysis-phonetic from docker/elasticsearch.Dockerfile), then " +
                "'docker compose up -d elasticsearch'. Index creation would otherwise fail with " +
                "'unknown filter type [phonetic]' or 'unknown token filter type [icu_folding]'.",
                string.Join(", ", missing));
            return false;
        }

        return true;
    }

    private async Task<List<string>> GetMissingPluginsAsync(
        ElasticsearchClient client,
        CancellationToken cancellationToken)
    {
        try
        {
            var resp = await client.Transport.RequestAsync<StringResponse>(
                HttpMethod.GET,
                "/_cat/plugins?h=component&format=json",
                cancellationToken);

            if (!resp.ApiCallDetails.HasSuccessfulStatusCode || string.IsNullOrWhiteSpace(resp.Body))
            {
                _logger.LogWarning("Could not retrieve installed ES plugins (status={Status}); skipping plugin pre-check.",
                    resp.ApiCallDetails.HttpStatusCode?.ToString() ?? "no-response");
                return new List<string>();
            }

            var body = resp.Body;
            var missing = new List<string>();
            foreach (var plugin in RequiredPlugins)
            {
                if (body.IndexOf(plugin, StringComparison.OrdinalIgnoreCase) < 0)
                    missing.Add(plugin);
            }
            return missing;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to query ES installed plugins; skipping plugin pre-check.");
            return new List<string>();
        }
    }

    private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source, CancellationToken ct)
    {
        var list = new List<T>();
        await foreach (var item in source.WithCancellation(ct)) list.Add(item);
        return list;
    }
}
