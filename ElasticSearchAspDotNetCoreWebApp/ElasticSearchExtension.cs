using ElasticSearch.Models;
using Nest;

namespace ElasticSearch
{
    public static class ElasticSearchExtension
    {
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            string baseUrl = configuration["ElasticSettings:baseUrl"];
            string index = configuration["ElasticSettings:defaultIndex"];

            var settings = new ConnectionSettings(new Uri(baseUrl ?? ""))
                .PrettyJson()
                .CertificateFingerprint("08c960a6b5107ca5c32b97662b0d68a38a4dc9ce397c2503831749c37e980b92")
                .BasicAuthentication("elastic", "GpZlUJJ551v3w-iNUGuf")
                .DefaultIndex(index);
            settings.EnableApiVersioningHeader();
            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);

            CreateIndex(client, index);
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings
                .DefaultMappingFor<BookModel>(m => m
                    .Ignore(p => p.Link)
                );
        }

        private static void CreateIndex(IElasticClient client, string indexName)
        {
            var createIndexResponse = client.Indices.Create(indexName,
                index => index.Map<BookModel>(x => x.AutoMap())
            );
        }
    }
}
