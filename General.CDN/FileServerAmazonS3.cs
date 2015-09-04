using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace General.CDN
{
    public class FileServerAmazonS3 : FileServerLocal, IFileServer
    {
        #region Properties
        private TransferUtility S3TransferUtil;
        private AmazonS3Client _objS3Client;
        public AmazonS3Client S3Client
        {
            get { return _objS3Client; }
            set
            {
                _objS3Client = value;
                InitAWS();
            }
        }

        private S3StorageClass _objStorageClass = S3StorageClass.Standard;
        public S3StorageClass StorageClass { get { return _objStorageClass; } set { _objStorageClass = value; } }

        private S3CannedACL _objACL = S3CannedACL.PublicRead;
        public S3CannedACL CurrentACL { get { return _objACL; } set { _objACL = value; } }
        
        #endregion

        #region Constructors
        public FileServerAmazonS3(FileServerSettings objSettings)
            : base(objSettings)
        {

        }

        public FileServerAmazonS3(FileServerSettings objSettings, AmazonS3Client objCDNClient)
            : base(objSettings)
        {
            S3Client = objCDNClient;
        }

        public FileServerAmazonS3(string strLocalStoragePath, string strLocalHostedURL, AmazonS3Client objCDNClient, string strCDNBucket)
            : base(strLocalStoragePath, strLocalHostedURL)
        {
            S3Client = objCDNClient;
            this.Settings.CDNStorageBucket = strCDNBucket;
        }
        #endregion

        #region Misc
        protected void InitAWS()
        {
            //Amazon Configuration (depends on AWSProfileName key on Web.config)
            if (S3Client != null)
            { 
                S3TransferUtil = new TransferUtility(S3Client);
            }
            else
            {
                S3TransferUtil = null;
            }
        }
        #endregion

        #region File Versioning Methods
        public override bool FileExpiredLocal(IFileQuery qryFile)
        {
            var propsLocal = GetFilePropertiesLocal(qryFile);
            if (propsLocal == null)
                return true;
            var propsCDN = GetFilePropertiesFromCDN(qryFile);
            if (propsCDN == null)
                return false;
            return !propsLocal.IsCurrentVersionOf(propsCDN);
        }

        public override bool FileExistsInCDN(IFileQuery qryFile)
        {
            return GetFilePropertiesFromCDN(qryFile) != null;
        }

        public override FileProperties GetFilePropertiesFromCDN(IFileQuery qryFile)
        {
            try
            {
                var objRef = S3TransferUtil.S3Client.GetObjectMetadata(GetCDNFolderPath(qryFile, true), GetFileName(qryFile));
                FileProperties props = new FileProperties();
                props.Source = objRef;
                props.Length = objRef.ContentLength;
                props.ETag = objRef.ETag;
                props.VersionNumber = objRef.VersionId;
                props.ContentMD5 = objRef.Headers.ContentMD5;
                props.ContentType = objRef.Headers.ContentType;
                props.LastModified = new DateTimeOffset(objRef.LastModified);
                props.Created = props.LastModified;
                props.URL = GetCDNURL(qryFile);
                return props;
            }
            catch (Amazon.S3.AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }
        #endregion

        #region GetCDNUrl
        public override string GetCDNURL(IFileQuery qryFile)
        {
            return "https://s3.amazonaws.com/" + GetCDNPath(qryFile, true);
        }
        #endregion

        #region SaveFromCDN
        private void SaveFromCDN(IFileQuery qryFile)
        {
            S3TransferUtil.Download(GetAndCreateLocalDiskPath(qryFile), GetCDNFolderPath(qryFile, true), GetFileName(qryFile));
        }

        private void PushToCDN(IFileQuery qryFile)
        {
            StoreFile(GetLocalDiskPath(qryFile), qryFile);
        }
        #endregion

        #region ImportIfNeeded
        public bool ImportIfNeeded(IFileQuery qryFile)
        {
            if (FileExistsLocal(qryFile) && !FileExistsInCDN(qryFile))
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
            if (FileExpiredLocal(qryFile))
                SaveFromCDN(qryFile);
            return base.LoadFileStream(qryFile);
        }

        public override byte[] LoadFileBytes(IFileQuery qryFile)
        {
            if (FileExpiredLocal(qryFile))
                SaveFromCDN(qryFile);
            return base.LoadFileBytes(qryFile);
        }

        public override string LoadFileText(IFileQuery qryFile)
        {
            if (FileExpiredLocal(qryFile))
                SaveFromCDN(qryFile);
            return base.LoadFileText(qryFile);
        }

        public override string LoadFileText(IFileQuery qryFile, System.Text.Encoding encoding)
        {
            if (FileExpiredLocal(qryFile))
                SaveFromCDN(qryFile);
            return base.LoadFileText(qryFile, encoding);
        }

        public override System.Drawing.Image LoadImage(IFileQuery qryFile)
        {
            if (FileExpiredLocal(qryFile))
                SaveFromCDN(qryFile);
            return base.LoadImage(qryFile);
        }

        #endregion

        #region Store Overrides
        private TransferUtilityUploadRequest GetUploadRequest(IFileQuery qryFileDestination)
        {
            TransferUtilityUploadRequest fileTransferUtilityRequest = new TransferUtilityUploadRequest
            {
                BucketName = GetCDNFolderPath(qryFileDestination, true),
                StorageClass = StorageClass,
                Key = GetFileName(qryFileDestination),
                CannedACL = CurrentACL
            };

            #region Meta Data
            fileTransferUtilityRequest.Metadata.Add("FileName", qryFileDestination.FileName);
            if (!String.IsNullOrWhiteSpace(qryFileDestination.Folder))
                fileTransferUtilityRequest.Metadata.Add("Folder", qryFileDestination.Folder);
            if (!String.IsNullOrWhiteSpace(qryFileDestination.SubFolder))
                fileTransferUtilityRequest.Metadata.Add("SubFolder", qryFileDestination.SubFolder);

            foreach (var item in qryFileDestination.MetaData)
                fileTransferUtilityRequest.Metadata.Add(item.Key, item.Value);
            #endregion

            return fileTransferUtilityRequest;
        }

        public override FileServerResult StoreFile(string strSourceFilePath, IFileQuery qryFileDestination)
        {
            var result = new FileServerResult(false);
            var fileTransferUtilityRequest = GetUploadRequest(qryFileDestination);
            fileTransferUtilityRequest.FilePath = strSourceFilePath;
            S3TransferUtil.Upload(fileTransferUtilityRequest);
            result = base.WriteFileLocal(strSourceFilePath, qryFileDestination);
            return result;
        }

        public override FileServerResult StoreFile(System.IO.Stream stmFile, IFileQuery qryFileDestination)
        {
            var result = base.WriteFileLocal(stmFile, qryFileDestination);
            if (result.Success)
            {
                var fileTransferUtilityRequest = GetUploadRequest(qryFileDestination);
                fileTransferUtilityRequest.FilePath = GetLocalDiskPath(qryFileDestination);
                S3TransferUtil.Upload(fileTransferUtilityRequest);
            }
            return result;
        }

        public override FileServerResult StoreFileFromString(string strFileBody, IFileQuery qryFileDestination, Encoding encoding = null)
        {
            var result = base.WriteFileFromStringLocal(strFileBody, qryFileDestination, encoding);
            if (result.Success)
            {
                var fileTransferUtilityRequest = GetUploadRequest(qryFileDestination);
                fileTransferUtilityRequest.FilePath = GetLocalDiskPath(qryFileDestination);
                S3TransferUtil.Upload(fileTransferUtilityRequest);
            }
            return result;
        }

        public override FileServerResult StoreImage(System.Drawing.Image objImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            var result = base.WriteImageLocal(objImage, qryFileDestination, enuFormat);
            if (result.Success)
            {
                var fileTransferUtilityRequest = GetUploadRequest(qryFileDestination);
                fileTransferUtilityRequest.FilePath = GetLocalDiskPath(qryFileDestination);
                S3TransferUtil.Upload(fileTransferUtilityRequest);
            }
            return result;
        }

        public override FileServerResult StoreImage(System.IO.Stream stmImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            var result = base.WriteImageLocal(stmImage, qryFileDestination, enuFormat);
            if (result.Success)
            {
                var fileTransferUtilityRequest = GetUploadRequest(qryFileDestination);
                fileTransferUtilityRequest.FilePath = GetLocalDiskPath(qryFileDestination);
                S3TransferUtil.Upload(fileTransferUtilityRequest);
            }
            return result;
        }

        public override FileServerResult StoreImage(string strSourceImagePath, IFileQuery qryFileDestination)
        {
            var result = new FileServerResult(false);
            var fileTransferUtilityRequest = GetUploadRequest(qryFileDestination);
            fileTransferUtilityRequest.FilePath = strSourceImagePath;
            S3TransferUtil.Upload(fileTransferUtilityRequest);
            result = base.WriteImageLocal(strSourceImagePath, qryFileDestination);
            return result;
        }
        
        #endregion

        #region Delete Overrides
        public override FileServerResult Delete(IFileQuery qryFile)
        {
            var result = base.DeleteFileLocal(qryFile);
            var response = S3Client.DeleteObject(GetCDNFolderPath(qryFile, true), GetFileName(qryFile));
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            { 
                result.Success = false;
                result.Message = response.HttpStatusCode.ToString();
            }
            return result;
        }
        #endregion

    }
}
