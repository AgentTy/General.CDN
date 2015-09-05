using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using General.CDN;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.Configuration;

namespace General.CDN.Tests.Controllers
{
    [TestClass]
    public class SpeedTestAzureVsAWS
    {

        #region Settings From App Config
        private string AmazonS3Bucket { 
            get
            {
                return ConfigurationManager.AppSettings["AmazonS3Bucket"];
            }
        }

        private string AzureBucket
        {
            get
            {
                return ConfigurationManager.AppSettings["AzureBucket"];
            }
        }
        #endregion

        [TestMethod]
        public async Task TestAmazonCDN()
        {
       
            string strDesertPath = TestContent.GetContentFilePhysicalPath("/Images/Desert.jpg");
            AmazonS3Client objAWSClient = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);

            //Test 1: Delete a file if it exists, then store an image, check it's existance, get it's properties, and compare file versions
            IFileServer server1 = new FileServerAmazonS3("LocalFolder", "http://web.com", objAWSClient, AmazonS3Bucket);
            
            IFileQuery file1 = new FileQuery("Desert.jpg", "uploads/images");
            if (server1.FileExistsInCDN(file1) || server1.FileExistsLocal(file1))
                server1.Delete(file1);
            Assert.IsFalse(server1.FileExistsInCDN(file1));
            Assert.IsFalse(server1.FileExistsLocal(file1));
            server1.StoreImage(strDesertPath, file1);
            Assert.IsTrue(server1.FileExistsLocal(file1));
            Assert.IsTrue(server1.FileExistsInCDN(file1));
            var props1CDN = server1.GetFilePropertiesFromCDN(file1);
            var props1Local = server1.GetFilePropertiesLocal(file1);
            Assert.IsNotNull(props1CDN);
            Assert.IsNotNull(props1Local);
            Assert.IsTrue(props1Local.IsCurrentVersionOf(props1CDN));

            //Run Test1 with async modifiers
            IFileServer server2 = new FileServerAmazonS3("LocalFolder2", "http://web.com", objAWSClient, AmazonS3Bucket);
            IFileQuery file21 = new FileQuery("Desert.jpg", "SiteImageFile");
            if (server1.FileExistsInCDN(file21) || server1.FileExistsLocal(file21))
                server1.Delete(file21);
            Assert.IsFalse(server1.FileExistsInCDN(file21));
            Assert.IsFalse(server1.FileExistsLocal(file21));
            await server2.StoreImageAsync(strDesertPath, file21);
            Assert.IsTrue(server2.FileExistsLocal(file21));
            Assert.IsTrue(server2.FileExistsInCDN(file21));
            var props2CDN = server2.GetFilePropertiesFromCDN(file21);
            var props2Local = server2.GetFilePropertiesLocal(file21);
            Assert.IsNotNull(props2CDN);
            Assert.IsNotNull(props2Local);
            Assert.IsTrue(props2Local.IsCurrentVersionOf(props2CDN));
            
            //Test 2: Store a file from a Unicode text string, then retreive it and compare the strings
            IFileQuery file2 = new FileQuery("Test.txt", "Scripts");
            server1.StoreFileFromString("this is some text § how was that", file2);
            string strFile2Text = server1.LoadFileText(file2);
            Assert.AreEqual(strFile2Text, "this is some text § how was that");
            server1.DeleteFileLocal(file2);
            Assert.IsFalse(server1.FileExistsLocal(file2));
            string test = server1.LoadFileText(file2);
            Assert.AreEqual(test, "this is some text § how was that");

            //Test 3: Make sure that this file, which was never uploaded, reflects that reality
            IFileQuery file3 = new FileQuery("Redfish.jpg", "uploads/images");
            Assert.IsFalse(server1.FileExistsLocal(file3));
            Assert.IsFalse(server1.FileExistsInCDN(file3));
            var strURL3 = server1.GetCDNURL(file3);
            Assert.IsTrue(General.Model.URL.IsValid(strURL3));
            var props2 = server1.GetFilePropertiesFromCDN(file3);
            Assert.IsNull(props2);

            //Test 4: Test each upload method
            string strKoalaPath = TestContent.GetContentFilePhysicalPath("/Images/Koala.jpg");
            System.Drawing.Image imgKoala = System.Drawing.Image.FromFile(strKoalaPath);
            IFileQuery file4 = new FileQuery("Upload.jpg", "Images");
            string strURL;

            file4.FileName = "UploadFromFile.jpg";
            strURL = server1.GetCDNURL(file4);
            server1.StoreImage(strKoalaPath, file4);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));

            file4.FileName = "UploadFromImage.jpg";
            strURL = server1.GetCDNURL(file4);
            server1.StoreImage(imgKoala, file4);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));

            file4.FileName = "UploadFromImage.png";
            strURL = server1.GetCDNURL(file4);
            server1.StoreImage(imgKoala, file4, System.Drawing.Imaging.ImageFormat.Png);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));

            file4.FileName = "UploadFromStream.png";
            strURL = server1.GetCDNURL(file4);
            var stream = new System.IO.MemoryStream();
            imgKoala.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            server1.StoreImage(stream, file4);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));

            file4.FileName = "UploadFromFileAsFile.jpg";
            strURL = server1.GetCDNURL(file4);
            server1.StoreFile(strKoalaPath, file4);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));

            file4.FileName = "UploadFromStreamAsFile.jpg";
            strURL = server1.GetCDNURL(file4);
            var stream2 = new System.IO.MemoryStream();
            imgKoala.Save(stream2, System.Drawing.Imaging.ImageFormat.Jpeg);
            stream2.Position = 0;
            server1.StoreFile(stream2, file4);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));

        }


        [TestMethod]
        public async Task TestAzureCDN()
        {
            string strFishPath = TestContent.GetContentFilePhysicalPath("/Images/Jellyfish.jpg");
            Microsoft.WindowsAzure.Storage.CloudStorageAccount objCDNClient 
                = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureStorageConnectionString"].ConnectionString);
            IFileServer server1 = new FileServerAzure("LocalFolder", "", objCDNClient, AzureBucket);
            
            //Test 1: Delete a file if it exists, then store an image, check it's existance, get it's properties, and compare file versions
            IFileQuery file1 = new FileQuery("Jellyfish.jpg", "Images");
            if (server1.FileExistsInCDN(file1) || server1.FileExistsLocal(file1))
                server1.Delete(file1);
            Assert.IsFalse(server1.FileExistsInCDN(file1));
            Assert.IsFalse(server1.FileExistsLocal(file1));
            server1.StoreImage(strFishPath, file1);
            Assert.IsTrue(server1.FileExistsLocal(file1));
            Assert.IsTrue(server1.FileExistsInCDN(file1));
            var props1CDN = server1.GetFilePropertiesFromCDN(file1);
            var props1Local = server1.GetFilePropertiesLocal(file1);
            Assert.IsNotNull(props1CDN);
            Assert.IsNotNull(props1Local);
            Assert.IsTrue(props1Local.IsCurrentVersionOf(props1CDN));

            //Test 2: Store a file from a Unicode text string, then retreive it and compare the strings
            IFileQuery file2 = new FileQuery("Test.txt", "Scripts");
            server1.StoreFileFromString("this is some text § how was that", file2);
            //server2.StoreFileFromString("some text", file2);
            string strFile2Text = server1.LoadFileText(file2);
            Assert.AreEqual(strFile2Text, "this is some text § how was that");
            server1.DeleteFileLocal(file2);
            Assert.IsFalse(server1.FileExistsLocal(file2));
            string test = server1.LoadFileText(file2);
            Assert.AreEqual(test, "this is some text § how was that");
            
            //Test 3: Make sure that this file, which was never uploaded, reflects that reality
            IFileQuery file3 = new FileQuery("Redfish.jpg", "uploads/images");
            Assert.IsFalse(server1.FileExistsLocal(file3));
            Assert.IsFalse(server1.FileExistsInCDN(file3));
            var strURL3 = server1.GetCDNURL(file3);
            Assert.IsTrue(General.Model.URL.IsValid(strURL3));
            var props2 = server1.GetFilePropertiesFromCDN(file3);
            Assert.IsNull(props2);
            
            //Test 4: Test each upload method
            string strKoalaPath = TestContent.GetContentFilePhysicalPath("/Images/Koala.jpg");
            System.Drawing.Image imgKoala = System.Drawing.Image.FromFile(strKoalaPath);
            IFileQuery file4 = new FileQuery("Upload.jpg", "Images");
            string strURL;

            file4.FileName = "UploadFromFile.jpg";
            strURL = server1.GetCDNURL(file4);
            server1.StoreImage(strKoalaPath, file4);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));

            file4.FileName = "UploadFromImage.jpg";
            strURL = server1.GetCDNURL(file4);
            server1.StoreImage(imgKoala, file4);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));

            file4.FileName = "UploadFromImage.png";
            strURL = server1.GetCDNURL(file4);
            server1.StoreImage(imgKoala, file4, System.Drawing.Imaging.ImageFormat.Png);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));

            file4.FileName = "UploadFromStream.png";
            strURL = server1.GetCDNURL(file4);
            var stream = new System.IO.MemoryStream();
            imgKoala.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            server1.StoreImage(stream, file4);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));

            file4.FileName = "UploadFromFileAsFile.jpg";
            strURL = server1.GetCDNURL(file4);
            server1.StoreFile(strKoalaPath, file4);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));

            file4.FileName = "UploadFromStreamAsFile.jpg";
            strURL = server1.GetCDNURL(file4);
            var stream2 = new System.IO.MemoryStream();
            imgKoala.Save(stream2, System.Drawing.Imaging.ImageFormat.Jpeg);
            stream2.Position = 0;
            server1.StoreFile(stream2, file4);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
            Assert.IsFalse(server1.FileExpiredLocal(file4));
        }
    }
}
