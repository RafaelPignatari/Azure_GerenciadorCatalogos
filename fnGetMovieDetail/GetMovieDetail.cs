using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace fnGetAllMovies
{
    public class GetMovieDetail
    {
        private readonly ILogger<GetMovieDetail> _logger;

        public GetMovieDetail(ILogger<GetMovieDetail> logger)
        {
            _logger = logger;
        }

        [Function("detail")]
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
                    var id = req.Query["id"];
                    var query = "SELECT * FROM c WHERE c.id = @id";
                    var querydefinition = new QueryDefinition(query).WithParameter("@id", id);
                    var result = container.GetItemQueryIterator<MovieResult>(querydefinition);
                    var results = new List<MovieResult>();

                    while (result.HasMoreResults)
                    {
                        foreach (var item in await result.ReadNextAsync())
                            results.Add(item);
                    }

                    var responseMessage = req.CreateResponse(HttpStatusCode.OK);
                    await responseMessage.WriteAsJsonAsync(results.FirstOrDefault());

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