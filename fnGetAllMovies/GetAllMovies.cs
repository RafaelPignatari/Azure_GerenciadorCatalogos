using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Cosmos;
using System.Net;
using Newtonsoft.Json;

namespace fnGetAllMovies
{
    public class GetAllMovies
    {
        private readonly ILogger<GetAllMovies> _logger;

        public GetAllMovies(ILogger<GetAllMovies> logger)
        {
            _logger = logger;
        }

        [Function("all")]
        public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req,
        ILogger log)
        {
            try
            {
                //log.LogInformation("C# HTTP trigger function processed a request.");
                var connectionString = Environment.GetEnvironmentVariable("CosmoDBConnection");

                using (var cosmosClient = new CosmosClient(connectionString))
                {
                    var container = cosmosClient.GetContainer("DioFlixDB", "movies");
                    var query = "SELECT * FROM c";
                    var querydefinition = new QueryDefinition(query);
                    var result = container.GetItemQueryIterator<MovieResult>(querydefinition);
                    var results = new List<MovieResult>();

                    while (result.HasMoreResults)
                    {
                        foreach (var item in await result.ReadNextAsync())
                            results.Add(item);
                    }

                    var responseMessage = req.CreateResponse(HttpStatusCode.OK);
                    await responseMessage.WriteAsJsonAsync(JsonConvert.SerializeObject(results));

                    return responseMessage;
                }
            }
            catch (Exception ex)
            {
                //log.LogError(ex, "C# HTTP trigger function processed a request.");

                return null;
            }
        }
    }
}