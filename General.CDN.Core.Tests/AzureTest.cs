using Microsoft.VisualStudio.TestTools.UnitTesting;
using General.CDN.Core;
using System.Threading.Tasks;

namespace General.CDN.Core.Tests
{
    [TestClass]
    public class AzureTest
    {

        [TestMethod]
        public async Task TestAzureCDN()
        {
            string strFishPath = TestContent.GetContentFilePhysicalPath("/Images/Jellyfish.jpg");
            Microsoft.Azure.Storage.CloudStorageAccount objCDNClient 
                = Microsoft.Azure.Storage.CloudStorageAccount.Parse(UnitTestContext.AzureStorageConnectionString);
            IFileServer server1 = new FileServerAzure("LocalFolder", "", objCDNClient, UnitTestContext.AzureBucket);
            
            //Test 1: Delete a file if it exists, then store an image, check it's existance, get it's properties, and compare file versions
            IFileQuery file1 = new FileQuery("Jellyfish.jpg", "Images");
            if (server1.FileExistsInCDN(file1) || server1.FileExistsLocal(file1))
                server1.Delete(file1);
            Assert.IsFalse(server1.FileExistsInCDN(file1));
            Assert.IsFalse(server1.FileExistsLocal(file1));
            file1.MetaData.Add("test", "Value");
            var result = server1.StoreImage(strFishPath, file1);
            string strResultURL = server1.GetCDNURL(file1);
            Assert.AreEqual(result.Uri.ToString(), strResultURL);
            var props = server1.GetFilePropertiesFromCDN(file1);
            props.MetaData.Remove("nothing");
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
            Assert.IsTrue(Model.URL.IsValid(strURL3));
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
        public async Task TestAzureOnlyNoLocal()
        {
            string strFishPath = TestContent.GetContentFilePhysicalPath("/Images/Jellyfish.jpg");
            Microsoft.Azure.Storage.CloudStorageAccount objCDNClient
                = Microsoft.Azure.Storage.CloudStorageAccount.Parse(UnitTestContext.AzureStorageConnectionString);
            IFileServer server1 = new FileServerAzure(objCDNClient, UnitTestContext.AzureBucket);

            //Test 1: Delete a file if it exists, then store an image, check it's existance, get it's properties, and compare file versions
            IFileQuery file1 = new FileQuery("Jellyfish.jpg", "Images");
            if (server1.FileExistsInCDN(file1) || server1.FileExistsLocal(file1))
                server1.Delete(file1);
            Assert.IsFalse(server1.FileExistsInCDN(file1));
            Assert.IsFalse(server1.FileExistsLocal(file1));
            file1.MetaData.Add("test", "Value");
            server1.StoreImage(strFishPath, file1);
            var props = server1.GetFilePropertiesFromCDN(file1);
            props.MetaData.Remove("nothing");
            Assert.IsFalse(server1.FileExistsLocal(file1));
            Assert.IsTrue(server1.FileExistsInCDN(file1));
            var props1CDN = server1.GetFilePropertiesFromCDN(file1);
            var props1Local = server1.GetFilePropertiesLocal(file1);
            Assert.IsNotNull(props1CDN);
            Assert.IsNull(props1Local);

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
            Assert.IsTrue(Model.URL.IsValid(strURL3));
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
            Assert.IsFalse(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));

            file4.FileName = "UploadFromImage.jpg";
            strURL = server1.GetCDNURL(file4);
            server1.StoreImage(imgKoala, file4);
            Assert.IsFalse(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));

            file4.FileName = "UploadFromImage.png";
            strURL = server1.GetCDNURL(file4);
            server1.StoreImage(imgKoala, file4, System.Drawing.Imaging.ImageFormat.Png);
            Assert.IsFalse(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));

            file4.FileName = "UploadFromStream.png";
            strURL = server1.GetCDNURL(file4);
            var stream = new System.IO.MemoryStream();
            imgKoala.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            server1.StoreImage(stream, file4);
            Assert.IsFalse(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));

            file4.FileName = "UploadFromFileAsFile.jpg";
            strURL = server1.GetCDNURL(file4);
            server1.StoreFile(strKoalaPath, file4);
            Assert.IsFalse(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));

            file4.FileName = "UploadFromStreamAsFile.jpg";
            strURL = server1.GetCDNURL(file4);
            var stream2 = new System.IO.MemoryStream();
            imgKoala.Save(stream2, System.Drawing.Imaging.ImageFormat.Jpeg);
            stream2.Position = 0;
            server1.StoreFile(stream2, file4);
            Assert.IsFalse(server1.FileExistsLocal(file4));
            Assert.IsTrue(server1.FileExistsInCDN(file4));
        }
    }
}
