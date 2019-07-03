using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General.CDN
{
    public class FileServerAzure : FileServerLocal, IFileServer
    {

        #region Properties
        
        private CloudStorageAccount _objAzureStorageAccount;
        public CloudStorageAccount AzureStorageAccount
        {
            get { return _objAzureStorageAccount; }
            set
            {
                _objAzureStorageAccount = value;
                InitAzure();
            }
        }

        public CloudBlobClient AzureBlobClient { get; set; }

        public bool CDNOnly { get { return this.Settings.DisableLocalStorage; } }

        #endregion

        #region Constructors
        public FileServerAzure(FileServerSettings objSettings)
            : base(objSettings)
        {

        }

        public FileServerAzure(FileServerSettings objSettings, CloudStorageAccount objCDNClient)
            : base(objSettings)
        {
            AzureStorageAccount = objCDNClient;
        }

        public FileServerAzure(CloudStorageAccount objCDNClient, string strCDNBucket)
    : base(DisableLocalStorage: true)
        {
            AzureStorageAccount = objCDNClient;
            this.Settings.CDNStorageBucket = strCDNBucket;
        }

        public FileServerAzure(string strLocalStoragePath, string strLocalHostedURL, CloudStorageAccount objCDNClient, string strCDNBucket)
            : base(strLocalStoragePath, strLocalHostedURL)
        {
            AzureStorageAccount = objCDNClient;
            this.Settings.CDNStorageBucket = strCDNBucket;
        }
        #endregion

        #region Misc
        protected void InitAzure()
        {
            //Azure Configuration
            if (AzureStorageAccount != null)
            {
                AzureBlobClient = AzureStorageAccount.CreateCloudBlobClient();
            }
            else
            {
                AzureBlobClient = null;
            }
        }

        protected CloudBlockBlob GetBlob(IFileQuery qryFile)
        {
            CloudBlobContainer objAzureContainer = AzureBlobClient.GetContainerReference(Settings.CDNStorageBucket);
            CloudBlockBlob blockBlob = objAzureContainer.GetBlockBlobReference(GetCDNPath(qryFile, false));
            return blockBlob;
        }
        #endregion

        #region File Versioning Methods
        public override bool FileExpiredLocal(IFileQuery qryFile)
        {
            if (CDNOnly)
                return true;

            var propsCDN = GetFilePropertiesFromCDN(qryFile);
            if (propsCDN == null)
                return false; //First, default to false if there is no object in the CDN
            var propsLocal = GetFilePropertiesLocal(qryFile);
            if (propsLocal == null)
                return true; //Next, default to true if there is no object locally, but it can be retrieved from the CDN.
            return !propsLocal.IsCurrentVersionOf(propsCDN); //Finally compare the files when both are available
        }

        public override bool FileExistsInCDN(IFileQuery qryFile)
        {
            return GetBlob(qryFile).Exists();
        }

        public override FileProperties GetFilePropertiesFromCDN(IFileQuery qryFile)
        {
            var blob = GetBlob(qryFile);
            try
            {
                blob.FetchAttributes();
                FileProperties props = new FileProperties();
                props.MetaData = blob.Metadata;
                props.Source = blob.Properties;
                props.Length = blob.Properties.Length;
                props.ContentMD5 = blob.Properties.ContentMD5;
                props.ETag = blob.Properties.ETag;
                props.ContentType = blob.Properties.ContentType;
                props.LastModified = blob.Properties.LastModified;
                props.Created = props.LastModified;
                props.URL = blob.Uri.ToString();
                return props;
            }
            catch (Microsoft.Azure.Storage.StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)System.Net.HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }
        #endregion

        #region GetCDNUrl
        public override string GetCDNURL(IFileQuery qryFile)
        {
            var blob = GetBlob(qryFile);
            return blob.Uri.ToString();
        }
        #endregion

        #region SaveFromCDN / PushToCDN
        private void SaveFromCDN(IFileQuery qryFile)
        {
            var blockBlobToDownload = GetBlob(qryFile);
            using (var fileStream = System.IO.File.OpenWrite(GetAndCreateLocalDiskPath(qryFile)))
            {
                blockBlobToDownload.DownloadToStream(fileStream);
            }
        }

        private void PushToCDN(IFileQuery qryFile)
        {
            StoreFile(GetLocalDiskPath(qryFile), qryFile);
        }
        #endregion

        #region ImportIfNeeded
        public bool ImportIfNeeded(IFileQuery qryFile)
        {
            if (CDNOnly)
                return false;

            if(FileExistsLocal(qryFile) && !FileExistsInCDN(qryFile))
            {
                PushToCDN(qryFile);
                return true;
            }
            return false;
        }
        #endregion

        #region Load Overrides
        public override System.IO.Stream LoadFileStream(IFileQuery qryFile)
        {
            if (CDNOnly)
            {
                var blob = GetBlob(qryFile);
                var stream = new System.IO.MemoryStream();
                blob.DownloadToStream(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
            else
            {
                if (FileExpiredLocal(qryFile))
                    SaveFromCDN(qryFile);
                return base.LoadFileStream(qryFile);
            }
        }

        public override byte[] LoadFileBytes(IFileQuery qryFile)
        {
            if (CDNOnly)
            {
                var blob = GetBlob(qryFile);
                byte[] fileContent = new byte[blob.Properties.Length];
                blob.DownloadToByteArray(fileContent, 0);
                return fileContent;
            }
            else
            {
                if (FileExpiredLocal(qryFile))
                    SaveFromCDN(qryFile);
                return base.LoadFileBytes(qryFile);
            }
        }

        public override string LoadFileText(IFileQuery qryFile)
        {
            if (CDNOnly)
            {
                var blob = GetBlob(qryFile);
                return blob.DownloadText();
            }
            else
            {
                if (FileExpiredLocal(qryFile))
                    SaveFromCDN(qryFile);
                return base.LoadFileText(qryFile);
            }
        }

        public override string LoadFileText(IFileQuery qryFile, System.Text.Encoding encoding)
        {
            if (CDNOnly)
            {
                var blob = GetBlob(qryFile);
                return blob.DownloadText(encoding);
            }
            else
            {
                if (FileExpiredLocal(qryFile))
                    SaveFromCDN(qryFile);
                return base.LoadFileText(qryFile, encoding);
            }
        }

        public override System.Drawing.Image LoadImage(IFileQuery qryFile)
        {
            if (CDNOnly)
            {
                return Image.FromStream(LoadFileStream(qryFile));
            }
            else
            {
                if (FileExpiredLocal(qryFile))
                    SaveFromCDN(qryFile);
                return base.LoadImage(qryFile);
            }
        }
        #endregion

        #region Store Overrides
        private CloudBlockBlob GetBlobForStorage(IFileQuery qryFileDestination)
        {
            var blockBlob = GetBlob(qryFileDestination);
            try
            {
                blockBlob.Properties.ContentType = Web.MimeTypeMap.GetMimeType(System.IO.Path.GetExtension(qryFileDestination.FileName));
            }
            catch { }

            #region Meta Data
            blockBlob.Metadata.Add("FileName", qryFileDestination.FileName);
            if (!String.IsNullOrWhiteSpace(qryFileDestination.Folder))
                blockBlob.Metadata.Add("Folder", qryFileDestination.Folder);
            if (!String.IsNullOrWhiteSpace(qryFileDestination.SubFolder))
                blockBlob.Metadata.Add("SubFolder", qryFileDestination.SubFolder);

            foreach (var item in qryFileDestination.MetaData)
                blockBlob.Metadata.Add(item);
            #endregion

            return blockBlob;
        }

        public override FileServerResult StoreFile(string strSourceFilePath, IFileQuery qryFileDestination)
        {
            var result = new FileServerResult(true);
            var blob = GetBlobForStorage(qryFileDestination);
            blob.UploadFromFile(strSourceFilePath);
            if(!CDNOnly)
                result = base.WriteFileLocal(strSourceFilePath, qryFileDestination);
            result.Uri = blob.Uri;
            return result;
        }

        public override FileServerResult StoreFile(System.IO.Stream stmFile, IFileQuery qryFileDestination)
        {
            if (CDNOnly)
            {
                var result = new FileServerResult(true);
                var blob = GetBlobForStorage(qryFileDestination);
                blob.UploadFromStream(stmFile);
                result.Uri = blob.Uri;
                return result;
            }
            else
            {
                var result = base.WriteFileLocal(stmFile, qryFileDestination);
                if (result.Success)
                {
                    var blob = GetBlobForStorage(qryFileDestination);
                    blob.UploadFromFile(GetLocalDiskPath(qryFileDestination));
                }
                return result;
            }
        }

        public override FileServerResult StoreFileFromString(string strFileBody, IFileQuery qryFileDestination, Encoding encoding = null)
        {
            var result = new FileServerResult(true);
            var blob = GetBlobForStorage(qryFileDestination);
            blob.UploadText(strFileBody, encoding);
            if (!CDNOnly)
                result = base.WriteFileFromStringLocal(strFileBody, qryFileDestination, encoding);
            result.Uri = blob.Uri;
            return result;
        }

        public override FileServerResult StoreImage(System.Drawing.Image objImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            if (CDNOnly)
            {
                var result = new FileServerResult(true);
                var blob = GetBlobForStorage(qryFileDestination);
                using (System.IO.MemoryStream stmImage = new System.IO.MemoryStream())
                {
                    if(enuFormat != null)
                        objImage.Save(stmImage, enuFormat);
                    else
                        objImage.Save(stmImage, objImage.RawFormat);

                    stmImage.Seek(0, SeekOrigin.Begin);
                    blob.UploadFromStream(stmImage);
                }
                result.Uri = blob.Uri;
                return result;
            }
            else
            {
                var result = base.WriteImageLocal(objImage, qryFileDestination, enuFormat);
                if (result.Success)
                {
                    var blob = GetBlobForStorage(qryFileDestination);
                    blob.UploadFromFile(GetLocalDiskPath(qryFileDestination));
                    result.Uri = blob.Uri;
                }
                return result;
            }
        }

        public override FileServerResult StoreImage(System.IO.Stream stmImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            if (CDNOnly)
            {
                var result = new FileServerResult(true);
                var blob = GetBlobForStorage(qryFileDestination);

                if (enuFormat != null)
                {
                    using(System.IO.MemoryStream stmImageFormatted = new System.IO.MemoryStream())
                    {
                        using (Image img = System.Drawing.Image.FromStream(stmImage))
                        {
                            img.Save(stmImageFormatted, enuFormat);
                            stmImageFormatted.Seek(0, SeekOrigin.Begin);
                        }
                        blob.UploadFromStream(stmImageFormatted);
                    }
                }
                else
                    blob.UploadFromStream(stmImage);
                result.Uri = blob.Uri;
                return result;
            }
            else
            {
                var result = base.WriteImageLocal(stmImage, qryFileDestination, enuFormat);
                if (result.Success)
                {
                    var blob = GetBlobForStorage(qryFileDestination);
                    blob.UploadFromFile(GetLocalDiskPath(qryFileDestination));
                }
                return result;
            }
        }

        public override FileServerResult StoreImage(string strSourceImagePath, IFileQuery qryFileDestination)
        {
            var result = new FileServerResult(false);
            var blob = GetBlobForStorage(qryFileDestination);
            blob.UploadFromFile(strSourceImagePath);
            if(!CDNOnly)
                result = base.WriteImageLocal(strSourceImagePath, qryFileDestination);
            result.Uri = blob.Uri;
            return result;
        }
        #endregion

        #region Delete Overrides
        public override FileServerResult Delete(IFileQuery qryFile)
        {
            FileServerResult result;
            if (CDNOnly)
                result = new FileServerResult(true);
            else
                result = base.DeleteFileLocal(qryFile);
            var blob = GetBlob(qryFile);
            var blnResult = blob.DeleteIfExists();
            if (!blnResult)
            { 
                result.Success = false;
                result.Message = "Blob delete failed at Azure";
            }
            return result;
        }
        #endregion

    }
}
