using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace fnPostDataStorage
{
    public class PostDataStorage
    {
        private readonly ILogger<PostDataStorage> _logger;

        public PostDataStorage(ILogger<PostDataStorage> logger)
        {
            _logger = logger;
        }

        [Function("DataStorage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Processando a imagem no storage");	

            try
            {
                if (!req.Headers.TryGetValue("file-type", out var fileTypeHeader))
                {
                    return new BadRequestObjectResult("O cabeçalho 'file-type' é obrigatório");
                }

                var fileType = fileTypeHeader.ToString();
                var form = await req.ReadFormAsync();
                var file = form.Files["file"];

                if (file == null || file.Length == 0)
                {
                    return new BadRequestObjectResult("O arquivo não foi enviado");
                }

                string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                string containerName = fileType;

                BlobClient blobClient = new BlobClient(connectionString, containerName, file.FileName);
                BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);

                await containerClient.CreateIfNotExistsAsync();
                await containerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);

                string blobName = file.FileName;
                var blob = containerClient.GetBlobClient(blobName);

                using (var stream = file.OpenReadStream())
                {
                    await blob.UploadAsync(stream, true);
                }

                _logger.LogInformation("Enviado com sucesso!");

                return new OkObjectResult(new {
                    Message = "Arquivo armazenado com sucesso!",
                    BlobURI = blob.Uri
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar a imagem no storage");
                return new BadRequestObjectResult("Erro ao processar a imagem no storage");
            }

            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
