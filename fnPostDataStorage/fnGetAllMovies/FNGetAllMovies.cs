using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace fnGetAllMovies
{
    public class FNGetAllMovies
    {
        private readonly ILogger<FNGetAllMovies> _logger;
        private readonly CosmosClient _cosmosClient;

        public FNGetAllMovies(ILogger<FNGetAllMovies> logger, CosmosClient cosmosClient)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
        }

        [Function("all")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var databaseName = Environment.GetEnvironmentVariable("DatabaseName");
            var containerName = Environment.GetEnvironmentVariable("ContainerName");

            var container = _cosmosClient.GetContainer(databaseName, containerName);

            var query = new QueryDefinition($"SELECT * FROM c");

            var results = container.GetItemQueryIterator<MovieResult>(query);

            var movies = new List<MovieResult>();

            while (results.HasMoreResults)
            {
                foreach (var item in await results.ReadNextAsync())
                {
                    movies.Add(item);
                }
            }

            var responseMessage = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await responseMessage.WriteAsJsonAsync(movies);

            return responseMessage;

            throw new BadHttpRequestException("Falha na chamada!");
        }
    }
}
