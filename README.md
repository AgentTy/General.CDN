# General.CDN
General.CDN for .Net C# provides an easy interface that turns Amazon S3 or Azure Blob Storage into a quick and dirty cloud storage system. Use it to sync files between servers or backup to the cloud.


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
            IFileServer server1 = new FileServerAzure("/LocalFolder/"
            , "http://localhostedurl.com", objCDNClient, "AzureBucketName");
            
            IFileQuery file1 = new FileQuery("Jellyfish.jpg", "Images");
            server1.StoreImage("C:\\SomeFile.jpg", file1);
            
            //Cleanup
            if (server1.FileExistsInCDN(file1) || server1.FileExistsLocal(file1))
                server1.Delete(file1);




            //Look at all the ways an IFileQuery can be turned into useful paths, the file server setup 
            //is designed so that every node can store it's files in different disk paths, but a given 
            //FileQuery will find the correct file at any node, regardless of their specific configurations. 
            //The "Base" path is the portion that is common to all nodes, local and remote.
            IFileServer server1 = new FileServerAzure("/Uploads/", "http://www.testdomain.com", null, "AzureBucketName");
            IFileQuery file1 = new FileQuery("Awesome.jpg", "Images");     
            string strURL, strLocalRelativePath, strPhysicalPath, strBasePath, strCDNPath;
            
            strURL = server1.GetLocalURL(file1); 
                        //http://www.testdomain.com/Uploads/Images/Awesome.jpg
                        
            strPhysicalPath = server1.GetLocalDiskPath(file1); 
                        //C:\\Users\\MyUser\\Documents\\General.CDN\\General.CDN.Tests\\bin\\Debug\\Uploads\\Images\\Awesome.jpg
                        
            strLocalRelativePath = server1.GetLocalRelativePath(file1); 
                        //Uploads/Images/Awesome.jpg
                        
            strBasePath = server1.GetBasePath(file1); 
                        //Images/Awesome.jpg
                        
            strCDNPath = server1.GetCDNPath(file1, true); 
                        //unittest/Images/Awesome.jpg
                        
            var blnExistsLocal = server1.FileExistsLocal(file1);
            var enuExistsRemote = server1.FileExistsLocal_HTTPCheck(file1);

            //Would you like to know if a file on your local node is up to date... try this...
            //IsCurrentVersionOf uses MD5 hash matching for files less than 10MB
            //if the file is larger or MD5 is not available in remote server... file byte size will be used for comparison
            var props1CDN = server1.GetFilePropertiesFromCDN(file1);
            var props1Local = server1.GetFilePropertiesLocal(file1);
            bool blnUpToDate = props1Local.IsCurrentVersionOf(props1CDN); 
           
