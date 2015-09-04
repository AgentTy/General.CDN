# General.CDN
General.CDN for .Net C# provides an easy interface that turns Amazon S3 or Azure Blob Storage into a quick and dirty cloud storage system. Use it to sync files between servers or backup to the cloud.


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

            string strURL, strLocalRelativePath, strPhysicalPath, strBasePath, strCDNPath;
            strURL = server1.GetLocalURL(file1);
            strPhysicalPath = server1.GetLocalDiskPath(file1);
            strLocalRelativePath = server1.GetLocalRelativePath(file1);
            strBasePath = server1.GetBasePath(file1);
            strCDNPath = server1.GetCDNPath(file1, true);
            var blnExistsLocal = server1.FileExistsLocal(file1);
            var enuExistsRemote = server1.FileExistsLocal_HTTPCheck(file1);
