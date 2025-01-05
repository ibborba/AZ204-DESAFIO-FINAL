using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace fnGetMovieDetail
{
    public class FNGetMovieDetail
    {
        private readonly ILogger<FNGetMovieDetail> _logger;
        private readonly CosmosClient _cosmosClient;

        public FNGetMovieDetail(ILogger<FNGetMovieDetail> logger, CosmosClient cosmosClient)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
        }

        [Function("detail")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var databaseName = Environment.GetEnvironmentVariable("DatabaseName");
            var containerName = Environment.GetEnvironmentVariable("ContainerName");

            var container = _cosmosClient.GetContainer(databaseName, containerName);

            var id = req.Query["id"];
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = '{id}'");

            var results = container.GetItemQueryIterator<MovieResult>(query);

            if (results.HasMoreResults)
            {
                var item = await results.ReadNextAsync();

                var responseMessage = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await responseMessage.WriteAsJsonAsync(item);

                return responseMessage;
            }

            throw new BadHttpRequestException("Falha na chamada!");
        }
    }
}
