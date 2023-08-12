namespace ElasticSearchWithAspDotNetCore;

public static class ElasticSearchExtension
{
    public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["ElasticSettings:Url"];
        var index = configuration["ElasticSettings:DefaultIndex"];

        var settings = new ConnectionSettings(new Uri(baseUrl))
            .PrettyJson()
            .CertificateFingerprint("ElasticSettings:CertificateFingerprint")
            .BasicAuthentication("ElasticSettings:User", "ElasticSettings:Password")
            .DefaultIndex(index);

        settings.EnableApiVersioningHeader();
        AddDefaultMappings(settings);
        var client = new ElasticClient(settings);
        services.AddSingleton<IElasticClient>(client);
        CreateIndex(client, index);
    }

    private static void AddDefaultMappings(ConnectionSettings settings)
    {
        settings.DefaultMappingFor<BookModel>(m => m.Ignore(p => p.Link));
    }

    private static void CreateIndex(IElasticClient client, string indexName)
    {
        client.Indices.Create(indexName, index => index.Map<BookModel>(x => x.AutoMap()));
    }
}
