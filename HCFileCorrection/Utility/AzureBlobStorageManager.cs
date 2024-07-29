using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public class AzureBlobStorageManager
{
    private string _storageAccountConnectionString;
    private BlobServiceClient _blobServiceClient;
    private string _containerName;

    public AzureBlobStorageManager(IConfiguration configuration, string containerName)
    {
        _storageAccountConnectionString = configuration["StorageAccountConnectionString"];
        _blobServiceClient = new BlobServiceClient(_storageAccountConnectionString);
        _containerName = containerName;
    }

    public async Task UploadFileToBlobAsync(string blobName, byte[] fileBytes)
    {
        // Get a reference to the blob container
        BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

        // Get a reference to the blob
        BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

        // Upload the file to the blob
        using (MemoryStream stream = new MemoryStream(fileBytes))
        {
            await blobClient.UploadAsync(stream, true);
        }
    }
}
