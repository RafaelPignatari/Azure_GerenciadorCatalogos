using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace fnPostDataBase
{
    public class PostDatabase
    {
        private readonly ILogger<PostDatabase> _logger;

        public PostDatabase(ILogger<PostDatabase> logger)
        {
            _logger = logger;
        }

        [Function("movie")]
        [CosmosDBOutput("%DatabaseName%", "%ContainerName%", Connection = "CosmoDBConnection", CreateIfNotExists = true, PartitionKey = "/id")]
        public static async Task<object?> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //log.LogInformation("C# HTTP trigger function processed a request.");

            MovieRequest movie = null;

            var content = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                movie = JsonConvert.DeserializeObject<MovieRequest>(content);
            }
            catch (Exception ex)
            {
                //log.LogError(ex.Message);
                return new BadRequestObjectResult("Erro ao desserializar content.");
            }

            return JsonConvert.SerializeObject(movie);
        }
    }
}
