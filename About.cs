using System;
/*
General.CDN is a common library that turns either Amazon S3 or Azure Blob files into a quick and dirty CDN.
It will facilitate the storage of files in either cloud, along with local caching, and basic version checking.

*/

namespace General.CDN
{
    public class About
    {
        private void Example()
        {
            //Simple example for S3...
            AmazonS3Client objAWSClient = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);

            IFileServer server1 = new FileServerAmazonS3("LocalFolder", "http://localhostedurl.com", objAWSClient, "S3BucketName");
            IFileQuery file1 = new FileQuery("Desert.jpg", "uploads/images");
            server1.StoreImage("C:\\SomeFile.jpg", file1);
            if (server1.FileExistsInCDN(file1) || server1.FileExistsLocal(file1))
                server1.Delete(file1);

            //Simple example for Azure
            Microsoft.WindowsAzure.Storage.CloudStorageAccount objCDNClient
                = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureStorageConnectionString"].ConnectionString);
            FileServerAzure server1 = new FileServerAzure("LocalFolder", "http://localhostedurl.com", objCDNClient, "AzureBucketName");
            IFileQuery file1 = new FileQuery("Jellyfish.jpg", "Images");
            server1.StoreImage("C:\\SomeFile.jpg", file1);
            if (server1.FileExistsInCDN(file1) || server1.FileExistsLocal(file1))
                server1.Delete(file1);
        }
    }
}