using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace blob_quickstart
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Azure Blob Storage - .NET quickstart sample\n");

            // Run the examples asynchronously, wait for the results before proceeding
            ProcessAsync().GetAwaiter().GetResult();

            Console.WriteLine("Press any key to exit the sample application.");
            Console.ReadLine();
        }

        private static async Task ProcessAsync()
        {
            // Retrieve the connection string for use with the application. The storage 
            // connection string is stored in an environment variable.
//            string storageConnectionString = Environment.GetEnvironmentVariable("StorageConnReg");
//            string storageConnectionString = Environment.GetEnvironmentVariable("StorageConnHns");
            string storageConnectionString = Environment.GetEnvironmentVariable("StorageConnHnsMultiProtocol");


            // Check whether the connection string can be parsed.
            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                // Get reference to the blob container named "csv-stream"
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("csv-stream");

                // Location for downloaded files (subfolder in current user's desktop)
                string downloadFolder = "csv-downloads";
                string localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), downloadFolder);
                string fileName, destination = null;
                if (!Directory.Exists(localPath))
                {
                    Directory.CreateDirectory(localPath);
                }

                // Iterate through blobs in the container, downloading each to local filesystem
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    // Get the value of the continuation token returned by the listing call.
                    blobContinuationToken = results.ContinuationToken;
                    foreach (IListBlobItem item in results.Results)
                    {
                        fileName = Path.GetFileName(item.Uri.LocalPath);
                        destination = Path.Combine(localPath, fileName);
                        Console.WriteLine("Downloading blob {0} to local destination {1}", item.Uri, destination);
                        CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(fileName);
                        await blob.DownloadToFileAsync(destination, FileMode.Create);
                    }
                } while (blobContinuationToken != null); // Loop while the continuation token is not null.
            }
            else
            {
                // Otherwise, let the user know that they need to define the environment variable.
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add an environment variable named 'CONNECT_STR' with your storage " +
                    "connection string as a value.");
                Console.WriteLine("Press any key to exit the application.");
                Console.ReadLine();
            }
        }
    }
}