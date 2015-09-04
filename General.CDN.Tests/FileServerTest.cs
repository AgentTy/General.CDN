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
    public class FileServerTest
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
        public void TestImagePathVariants()
        {
            IFileServer server1 = new FileServerAzure("/Uploads/", "http://www.testdomain.com", null, AzureBucket);
            IFileQuery file1 = new FileQuery("Floorplan_24_635550193570112158.jpg", "FloorplanImageFile");

            IFileServer server2 = new FileServerAzure("/Uploads/", "http://www.anotherdomain.com", null, AzureBucket);
            IFileQuery file2 = new FileQuery("triple_platinum_weddings_11_82691_photo_101.png", "ExPhotoImageFile", "/");

            string strURL, strLocalRelativePath, strPhysicalPath, strBasePath, strCDNPath;
            bool blnExistsLocal = false;
            General.Model.URL.URLCheckExistsResult enuExistsRemote = General.Model.URL.URLCheckExistsResult.Unknown;
            // Assert
            strURL = server1.GetLocalURL(file1);
            strPhysicalPath = server1.GetLocalDiskPath(file1);
            strLocalRelativePath = server1.GetLocalRelativePath(file1);
            strBasePath = server1.GetBasePath(file1);
            strCDNPath = server1.GetCDNPath(file1, true);
            blnExistsLocal = server1.FileExistsLocal(file1);
            enuExistsRemote = server1.FileExistsLocal_HTTPCheck(file1);

            Assert.IsNotNull(strURL);
            Assert.AreEqual(strURL, "http://www.testdomain.com/Uploads/FloorplanImageFile/Floorplan_24_635550193570112158.jpg");
            Assert.IsNotNull(strPhysicalPath);
            Assert.IsNotNull(strLocalRelativePath);
            Assert.IsNotNull(strBasePath);
            Assert.IsNotNull(strCDNPath);
            Assert.IsTrue(Uri.IsWellFormedUriString(strURL, UriKind.Absolute));
            Assert.IsTrue(strPhysicalPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1);
            Assert.IsTrue(Uri.IsWellFormedUriString(strLocalRelativePath, UriKind.Relative));
            Assert.IsTrue(Uri.IsWellFormedUriString(strBasePath, UriKind.Relative));
            Assert.IsTrue(Uri.IsWellFormedUriString(strCDNPath, UriKind.Relative));
            Assert.IsTrue(General.Model.URL.IsValid(strURL));
            //Assert.IsTrue(enuExistsRemote == General.Model.URL.URLCheckExistsResult.Exists);

            strURL = server2.GetLocalURL(file2);
            strPhysicalPath = server2.GetLocalDiskPath(file2);
            strLocalRelativePath = server2.GetLocalRelativePath(file2);
            strBasePath = server2.GetBasePath(file2);
            strCDNPath = server1.GetCDNPath(file1, true);
            blnExistsLocal = server2.FileExistsLocal(file2);
            enuExistsRemote = server2.FileExistsLocal_HTTPCheck(file2);

            Assert.IsNotNull(strURL);
            Assert.AreEqual(strURL, "http://www.anotherdomain.com/Uploads/ExPhotoImageFile/triple_platinum_weddings_11_82691_photo_101.png");
            Assert.IsNotNull(strPhysicalPath);
            Assert.IsNotNull(strLocalRelativePath);
            Assert.IsNotNull(strBasePath);
            Assert.IsNotNull(strCDNPath);
            Assert.IsTrue(Uri.IsWellFormedUriString(strURL, UriKind.Absolute));
            Assert.IsTrue(strPhysicalPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1);
            Assert.IsTrue(Uri.IsWellFormedUriString(strLocalRelativePath, UriKind.Relative));
            Assert.IsTrue(Uri.IsWellFormedUriString(strBasePath, UriKind.Relative));
            Assert.IsTrue(Uri.IsWellFormedUriString(strCDNPath, UriKind.Relative));
            Assert.IsTrue(General.Model.URL.IsValid(strURL));
            //Assert.IsTrue(enuExistsRemote == General.Model.URL.URLCheckExistsResult.Exists);

            //This should not exist because it's the wrong server for the file.
            //Assert.IsTrue(server2.FileExistsLocal_HTTPCheck(file1) == General.Model.URL.URLCheckExistsResult.DoesNotExist);

        }

        [TestMethod]
        public async Task TestWriteImagesLocal()
        {
            string strTulipsPath = TestContent.GetContentFilePhysicalPath("/Images/Tulips.jpg");

            IFileServer server1 = new FileServerLocal("Temp", "");
            IFileQuery file1 = new FileQuery("Tulips.jpg", "SiteImageFile");

            for (int i = 0; i < 100; i++)
            {
                server1.StoreImage(strTulipsPath, file1);
                Assert.IsTrue(server1.FileExistsLocal(file1));
                server1.Delete(file1);
                System.Threading.Thread.Sleep(5); //It seems like Delete returns before the delete is known by the file system, here's some buffer.
                Assert.IsFalse(server1.FileExistsLocal(file1));
                
            }

            for (int i = 0; i < 100; i++)
            {
                var tskStore = server1.StoreImageAsync(strTulipsPath, file1);
                await tskStore;
                Assert.IsTrue(server1.FileExistsLocal(file1));
                var tskDelete = server1.DeleteAsync(file1);
                await tskDelete;
                System.Threading.Thread.Sleep(5); //It seems like Delete returns before the delete is known by the file system, here's some buffer.
                Assert.IsFalse(server1.FileExistsLocal(file1));
            }
            Assert.IsFalse(server1.FileExistsLocal(file1));

            IFileQuery file2 = new FileQuery("TulipsFromImage.jpg", "SiteImageFile");
            IFileQuery file3 = new FileQuery("TulipsFromStream.jpg", "SiteImageFile");
            IFileQuery file4 = new FileQuery("TulipsFromImageWithTypeChange.png", "SiteImageFile");  

            //Test System.Drawing.Image
            using(System.Drawing.Image objImage = System.Drawing.Image.FromFile(strTulipsPath))
            {
                await server1.StoreImageAsync(objImage, file2);
            }
            Assert.IsTrue(server1.FileExistsLocal(file2));

            //Test System.Stream
            using(System.IO.Stream stmImage = new System.IO.FileStream(strTulipsPath, System.IO.FileMode.Open))
            {
                await server1.StoreImageAsync(stmImage, file3, System.Drawing.Imaging.ImageFormat.Jpeg);
                stmImage.Close();
            }
            Assert.IsTrue(server1.FileExistsLocal(file3));

            await server1.StoreImageAsync(System.Drawing.Image.FromFile(strTulipsPath), file4, System.Drawing.Imaging.ImageFormat.Png);
            Assert.IsTrue(server1.FileExistsLocal(file4));
            
        }

        [TestMethod]
        public void TestWriteFilesLocal()
        {
            string strTestJSPath = TestContent.GetContentFilePhysicalPath("/Scripts/jQuery.validate.js");

            IFileServer server1 = new FileServerLocal("Temp", "");
            IFileQuery file1 = new FileQuery("validate.js", "Scripts");
            IFileQuery file2 = new FileQuery("validate2.js", "Scripts");
            IFileQuery file3 = new FileQuery("validate3.js", "Scripts");

            server1.StoreFile(strTestJSPath, file1);
            Assert.IsTrue(server1.FileExistsLocal(file1));

            server1.StoreFileAsync(strTestJSPath, file2).ContinueWith((t) => {
                Assert.IsTrue(server1.FileExistsLocal(file2));
            });

            //Test System.Stream
            using (System.IO.Stream stmFile = new System.IO.FileStream(strTestJSPath, System.IO.FileMode.Open))
            {
                server1.StoreFile(stmFile, file3);
                stmFile.Close();
            }
            Assert.IsTrue(server1.FileExistsLocal(file3));

            IFileQuery file4 = new FileQuery("text.txt", "Docs");
            server1.StoreFileFromString("this is some text § how was that", file4);
            string strLoaded = server1.LoadFileText(file4);
            Assert.AreEqual(strLoaded, "this is some text § how was that");

            IFileQuery file5 = new FileQuery("textEncodedAscii.txt", "Docs");
            server1.StoreFileFromString("this is some text § how was that", file5, Encoding.ASCII);
            strLoaded = server1.LoadFileText(file5);
            Assert.AreEqual(strLoaded, "this is some text ? how was that");
        }

        /*
        [TestMethod]
        public void TestWriteFilesLocalNetwork()
        {
            string strTestJSPath = TestContent.GetContentFilePhysicalPath("/Scripts/jQuery.validate.js");

            IFileServer server1 = new FileServerLocal("\\\\NETWORKNAME\\Documents\\Test", "");
            IFileQuery file1 = new FileQuery("validate.js", "Scripts");

            server1.StoreFile(strTestJSPath, file1);
            Assert.IsTrue(server1.FileExistsLocal(file1));
        }
        */

    }
}
