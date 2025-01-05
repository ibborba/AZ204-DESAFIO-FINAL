using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace fnPostDataStorage
{
    public class FNDataStorage
    {
        private readonly ILogger<FNDataStorage> _logger;

        public FNDataStorage(ILogger<FNDataStorage> logger)
        {
            _logger = logger;
        }

        [Function("dataStorage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("A fun��o foi iniciada.");

            try
            {
                if (!req.Headers.TryGetValue("file-type", out var fileTypeHeader))
                {
                    _logger.LogInformation("Tipo do arquivo inv�lido.");
                    return new BadRequestObjectResult("O cabe�alho 'file-type' � obrigat�rio!");
                }   

                var fileType = fileTypeHeader.ToString();

                var form = await req.ReadFormAsync();
                var file = form.Files["file"];

                if (file == null || file.Length == 0)
                {
                    return new BadRequestObjectResult("O possui tamanho ZERO ou formato inv�lido.");
                }

                string blobName = file.FileName;

                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var containerName = fileType;

                BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
                await containerClient.CreateIfNotExistsAsync();
                await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);

                BlobClient blobClient = new BlobClient(connectionString, containerName, blobName);
                
                await blobClient.UploadAsync(file.OpenReadStream(), true);

                var blob = containerClient.GetBlobClient(blobName);

                _logger.LogInformation("Arquivo com upload conclu�do!");
                return new OkObjectResult(new
                {
                    Message = "Arquivo com upload conclu�do!",
                    BlobUri = blob.Uri
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
