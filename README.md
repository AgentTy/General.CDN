# General.CDN
General.CDN for .Net C# provides an easy interface that turns Amazon S3 or Azure Blob Storage into a quick and dirty cloud storage system. Use it to sync files between servers or backup to the cloud.

           In order to compile and run this solution, you may need to do the following...
           1. Enable NuGet package restore for the solution
           2. Remove and re-install the following NuGet packages
                      a. AWS SDK for .Net
                      b. Windows Azure Storage
                      c. Microsoft Azure Configuration Manager
           3. Create an App.config file in the UnitTest project (use App.config.public for example)
           4. Update App.config to use your AWS settings and/or Azure connection strings


           //Simple example for S3...
            AmazonS3Client objAWSClient = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);
            IFileServer server1 = new FileServerAmazonS3("/LocalFolder/"
            , "http://localhostedurl.com", objAWSClient, "S3BucketName");

            IFileQuery file1 = new FileQuery("Desert.jpg", "uploads/images");
            server1.StoreImage("C:\\SomeFile.jpg", file1);
            //Cleanup
            if (server1.FileExistsInCDN(file1) || server1.FileExistsLocal(file1))
                server1.Delete(file1);
            


            //Simple example for Azure
            Microsoft.WindowsAzure.Storage.CloudStorageAccount objCDNClient
                = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(
                ConfigurationManager.ConnectionStrings["AzureStorageConnectionString"].ConnectionString);
            IFileServer server2 = new FileServerAzure("/LocalFolder/"
            , "http://localhostedurl.com", objCDNClient, "AzureBucketName");

            IFileQuery file2 = new FileQuery("Jellyfish.jpg", "Images");
            server2.StoreImage("C:\\SomeFile.jpg", file2);
            //Cleanup
            if (server2.FileExistsInCDN(file2) || server2.FileExistsLocal(file2))
                server2.Delete(file2);

            




            //Look at all the ways an IFileQuery can be turned into useful paths, the file server setup 
            //is designed so that every node can store it's files in different disk paths, but a given 
            //FileQuery will find the correct file at any node, regardless of their specific configurations. 
            //The "Base" path is the portion that is common to all nodes, local and remote.

            IFileServer server3 = new FileServerAzure("/Uploads/", "http://www.testdomain.com", null, "AzureBucketName");
            IFileQuery file3 = new FileQuery("Awesome.jpg", "Images");
            string strURL, strLocalRelativePath, strPhysicalPath, strBasePath, strCDNPath;

            strURL = server3.GetLocalURL(file3);
            //http://www.testdomain.com/Uploads/Images/Awesome.jpg

            strPhysicalPath = server3.GetLocalDiskPath(file3);
            //C:\\Users\\MyUser\\Documents\\General.CDN\\General.CDN.Tests\\bin\\Debug\\Uploads\\Images\\Awesome.jpg

            strLocalRelativePath = server3.GetLocalRelativePath(file3);
            //Uploads/Images/Awesome.jpg

            strBasePath = server3.GetBasePath(file3);
            //Images/Awesome.jpg

            strCDNPath = server3.GetCDNPath(file3, true);
            //unittest/Images/Awesome.jpg

            var blnExistsLocal = server3.FileExistsLocal(file3);
            var enuExistsHTTP = server3.FileExistsLocal_HTTPCheck(file3);
            var blnExistsRemote = server3.FileExistsInCDN(file3);
            
            
            //Would you like to know if a file on your local node is up to date... try this...
            //IsCurrentVersionOf uses MD5 hash matching for files less than 10MB
            //if the file is larger or MD5 is not available in remote server... file byte size will be used for comparison
            var props1CDN = server3.GetFilePropertiesFromCDN(file3);
            var props1Local = server3.GetFilePropertiesLocal(file3);
            bool blnUpToDate = props1Local.IsCurrentVersionOf(props1CDN); 
