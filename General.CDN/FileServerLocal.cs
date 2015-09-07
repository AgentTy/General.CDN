using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General.CDN
{
   
    public class FileServerResult
    {
        public static FileServerResult Successful = new FileServerResult(true);
        public static FileServerResult Failure = new FileServerResult(false);

        public FileServerResult(bool blnSuccess)
        {
            Success = blnSuccess;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public interface IFileServer
    {
        FileServerSettings Settings { get; set; }

        /// <summary>
        /// Gets the specified file name with any modifications required by the file server.
        /// </summary>
        /// <param name="qryFile">Any object that impliments IFileQuery</param>
        /// <returns>File Name</returns>
        string GetFileName(IFileQuery qryFile);

        /// <summary>
        /// Get the global folder path for a file. This portion applies to local and remote storage environments.
        /// </summary>
        /// <param name="qryFile">Any object that impliments IFileQuery</param>
        /// <returns>Relative URI</returns>
        string GetFolderPath(IFileQuery qryFile);

        /// <summary>
        /// Adds CDN specific storage path to GetFolderPath, with or without the top level CDN bucket.
        /// </summary>
        /// <param name="qryFile">Any object that impliments IFileQuery</param>
        /// <returns>Relative URI</returns>
        string GetCDNFolderPath(IFileQuery qryFile, bool blnIncludeCDNStorageBucket);

        /// <summary>
        /// Adds Local path information to GetFolderPath.
        /// </summary>
        /// <param name="qryFile">Any object that impliments IFileQuery</param>
        /// <returns>Relative URI</returns>
        string GetLocalFolderPath(IFileQuery qryFile);

        /// <summary>
        /// Global folder path + file name. 
        /// </summary>
        /// <param name="qryFile">Any object that impliments IFileQuery</param>
        /// <returns>Relative URI</returns>
        string GetBasePath(IFileQuery qryFile);

        /// <summary>
        /// CDN folder path + file name. 
        /// </summary>
        /// <param name="qryFile">Any object that impliments IFileQuery</param>
        /// <returns>Relative URI</returns>
        string GetCDNPath(IFileQuery qryFile, bool blnIncludeCDNStorageBucket);

        /// <summary>
        /// Local folder path + file name. 
        /// </summary>
        /// <param name="qryFile">Any object that impliments IFileQuery</param>
        /// <returns>Relative URI</returns>
        string GetLocalRelativePath(IFileQuery qryFile);

        /// <summary>
        /// An absolute path to this file on the local disk.
        /// </summary>
        /// <param name="qryFile">Any object that impliments IFileQuery</param>
        /// <returns>Absolute URI</returns>
        string GetLocalDiskPath(IFileQuery qryFile);

        /// <summary>
        /// Local server URL + Local folder path + file name. 
        /// </summary>
        /// <param name="qryFile">Any object that impliments IFileQuery</param>
        /// <returns>Url</returns>
        string GetLocalURL(IFileQuery qryFile);

        /// <summary>
        /// Gets the primary Url for serving this file from the remote server. 
        /// </summary>
        /// <param name="qryFile">Any object that impliments IFileQuery</param>
        /// <returns>Url</returns>
        string GetCDNURL(IFileQuery qryFile);

        bool FileExistsLocal(IFileQuery qryFile);
        bool FileExistsInCDN(IFileQuery qryFile);
        bool FileExpiredLocal(IFileQuery qryFile);
        FileProperties GetFilePropertiesFromCDN(IFileQuery qryFile);
        FileProperties GetFilePropertiesLocal(IFileQuery qryFile);
        Model.URL.URLCheckExistsResult FileExistsLocal_HTTPCheck(IFileQuery qryFile);

        System.IO.Stream LoadFileStream(IFileQuery qryFile);
        byte[] LoadFileBytes(IFileQuery qryFile);
        string LoadFileText(IFileQuery qryFile);
        string LoadFileText(IFileQuery qryFile, System.Text.Encoding encoding);
        Image LoadImage(IFileQuery qryFile);

        FileServerResult DeleteFileLocal(IFileQuery qryFile);
        FileServerResult Delete(IFileQuery qryFile);
        Task<FileServerResult> DeleteAsync(IFileQuery qryFile);

        FileServerResult StoreFile(System.IO.Stream stmFile, IFileQuery qryFileDestination);
        Task<FileServerResult> StoreFileAsync(System.IO.Stream stmFile, IFileQuery qryFileDestination);

        FileServerResult StoreFile(string strSourceFilePath, IFileQuery qryFileDestination);
        Task<FileServerResult> StoreFileAsync(string strSourceFilePath, IFileQuery qryFileDestination);

        FileServerResult StoreFileFromString(string strFileBody, IFileQuery qryFileDestination, System.Text.Encoding encoding = null);
        Task<FileServerResult> StoreFileFromStringAsync(string strFileBody, IFileQuery qryFileDestination, System.Text.Encoding encoding = null);

        FileServerResult StoreImage(Image objImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null);
        Task<FileServerResult> StoreImageAsync(Image objImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null);

        FileServerResult StoreImage(System.IO.Stream stmImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null);
        Task<FileServerResult> StoreImageAsync(System.IO.Stream stmImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null);

        FileServerResult StoreImage(string strSourceImagePath, IFileQuery qryFileDestination);
        Task<FileServerResult> StoreImageAsync(string strSourceImagePath, IFileQuery qryFileDestination);
    }

    public class FileServerLocal : IFileServer
    {
        private FileServerSettings _objSettings;
        public FileServerSettings Settings
        {
            get
            {
                return _objSettings;
            }
            set
            {
                _objSettings = value;
            }
        }

        #region Constructors
        public FileServerLocal(FileServerSettings objSettings)
        {
            Settings = objSettings;
            CleanupPaths();
        }

        public FileServerLocal(string strLocalStoragePath, string strLocalHostedURL)
        {
            Settings = new FileServerSettings();
            Settings.LocalStoragePath = strLocalStoragePath;
            Settings.LocalHostedURL = strLocalHostedURL;
            CleanupPaths();
        }
        #endregion

        #region Path Methods
        public string GetFileName(IFileQuery qryFile)
        {
            if (!String.IsNullOrWhiteSpace(Settings.GlobalFileNamePrepend))
                return Settings.GlobalFileNamePrepend + qryFile.FileName;
            return qryFile.FileName;
        }

        public string GetFolderPath(IFileQuery qryFile)
        {
            if (!String.IsNullOrWhiteSpace(qryFile.SubFolder))
            {
                qryFile.SubFolder = CleanPathSegment(qryFile.SubFolder);
                if (!String.IsNullOrWhiteSpace(qryFile.SubFolder) && !qryFile.SubFolder.EndsWith("/"))
                    qryFile.SubFolder = qryFile.SubFolder + "/";
            }
 
            if(!String.IsNullOrWhiteSpace(qryFile.Folder) && !String.IsNullOrWhiteSpace(qryFile.SubFolder))
                return qryFile.Folder + "/" + qryFile.SubFolder;
            else if (!String.IsNullOrWhiteSpace(qryFile.Folder))
                return qryFile.Folder;
            else if(!String.IsNullOrWhiteSpace(qryFile.SubFolder))
                return qryFile.SubFolder;
            else
                return "";
        }

        public string GetCDNFolderPath(IFileQuery qryFile, bool blnIncludeCDNStorageBucket)
        {
            string strPath = "";

            if (blnIncludeCDNStorageBucket && !String.IsNullOrWhiteSpace(Settings.CDNStorageBucket))
                strPath += Settings.CDNStorageBucket + "/";

            if (!String.IsNullOrWhiteSpace(Settings.GlobalFilePathPrepend_CDN))
                strPath += Settings.GlobalFilePathPrepend_CDN + "/";

            strPath += GetFolderPath(qryFile);

            if (!String.IsNullOrWhiteSpace(Settings.GlobalFilePathAppend_CDN))
                strPath += Settings.GlobalFilePathAppend_CDN + "/";

            return strPath.TrimEnd('/');
        }

        public string GetLocalFolderPath(IFileQuery qryFile)
        {
            string strPath = "";

            if (!String.IsNullOrWhiteSpace(Settings.LocalStoragePath))
                strPath += Settings.LocalStoragePath + "/";

            if (!String.IsNullOrWhiteSpace(Settings.GlobalFilePathPrepend_Local))
                strPath += Settings.GlobalFilePathPrepend_Local + "/";

            strPath += GetFolderPath(qryFile) + "/";

            if (!String.IsNullOrWhiteSpace(Settings.GlobalFilePathAppend_Local))
                strPath += Settings.GlobalFilePathAppend_Local + "/";

            return strPath.TrimEnd('/');
        }

        public string GetBasePath(IFileQuery qryFile)
        {
            return GetFolderPath(qryFile) + "/" + GetFileName(qryFile);
        }

        public string GetCDNPath(IFileQuery qryFile, bool blnIncludePrimaryBucket)
        {
            return GetCDNFolderPath(qryFile, blnIncludePrimaryBucket) + "/" + GetFileName(qryFile);
        }

        public string GetLocalRelativePath(IFileQuery qryFile)
        {
            return GetLocalFolderPath(qryFile) + "/" + GetFileName(qryFile);
        }

        public string GetLocalDiskPath(IFileQuery qryFile)
        {
            return MapPathLocal(GetLocalRelativePath(qryFile));
        }

        public string GetLocalURL(IFileQuery qryFile)
        {
            return Settings.LocalHostedURL + "/" + GetLocalRelativePath(qryFile);
        }

        public string GetAndCreateLocalDiskPath(IFileQuery qryFile)
        {
            string strFilePath = GetLocalDiskPath(qryFile);
            string strDirectoryPath = System.IO.Path.GetDirectoryName(strFilePath);

            try
            {
                if (!System.IO.Directory.Exists(strDirectoryPath))
                    System.IO.Directory.CreateDirectory(strDirectoryPath);
            }
            catch(Exception ex) { }

            return strFilePath;
        }
        #endregion

        #region Local File Load Methods

        public System.IO.FileStream LoadFileStreamLocal(IFileQuery qryFile)
        {
            if (!FileExistsLocal(qryFile))
                return null;
            return System.IO.File.OpenRead(GetLocalDiskPath(qryFile));
        }

        public byte[] LoadFileBytesLocal(IFileQuery qryFile)
        {
            if (!FileExistsLocal(qryFile))
                return null;
            return System.IO.File.ReadAllBytes(GetLocalDiskPath(qryFile));
        }

        public string LoadFileTextLocal(IFileQuery qryFile)
        {
            if (!FileExistsLocal(qryFile))
                return null;
            return System.IO.File.ReadAllText(GetLocalDiskPath(qryFile));
        }

        public string LoadFileTextLocal(IFileQuery qryFile, System.Text.Encoding encoding)
        {
            if (!FileExistsLocal(qryFile))
                return null;
            return System.IO.File.ReadAllText(GetLocalDiskPath(qryFile), encoding);
        }

        public Image LoadImageLocal(IFileQuery qryFile)
        {
            if (!FileExistsLocal(qryFile))
                return null;
            return new Bitmap(GetLocalDiskPath(qryFile));
        }

        #endregion

        #region Local File Methods
        public bool FileExistsLocal(IFileQuery qryFile)
        {
            return System.IO.File.Exists(GetLocalDiskPath(qryFile));
        }

        public FileProperties GetFilePropertiesLocal(IFileQuery qryFile)
        {
            try
            {
                var objFileInfo = new System.IO.FileInfo(GetLocalDiskPath(qryFile));
                FileProperties props = new FileProperties();
                props.Source = objFileInfo;
                props.Length = objFileInfo.Length;
                props.LastModified = new DateTimeOffset(objFileInfo.LastWriteTimeUtc);
                props.Created = new DateTimeOffset(objFileInfo.CreationTimeUtc);
                props.URL = GetLocalURL(qryFile);
                return props;
            }
            catch (System.IO.FileNotFoundException) { return null; }
            catch (System.IO.DirectoryNotFoundException) { return null; }
        }

        public bool DirectoryExistsLocal(IFileQuery qryFile)
        {
            return System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(GetLocalDiskPath(qryFile)));
        }

        public Model.URL.URLCheckExistsResult FileExistsLocal_HTTPCheck(IFileQuery qryFile)
        {
            return new Model.URL(GetLocalURL(qryFile)).CheckExists();
        }

        public FileServerResult DeleteFileLocal(IFileQuery qryFile)
        {
            if (FileExistsLocal(qryFile))
                System.IO.File.Delete(GetLocalDiskPath(qryFile));
            return FileServerResult.Successful;
        }
        public Task<FileServerResult> DeleteFileLocalAsync(IFileQuery qryFile)
        {
            return Task.Factory.StartNew(() =>
            {
                return DeleteFileLocal(qryFile);
            });
        }

        public FileServerResult WriteFileLocal(System.IO.Stream stmFile, IFileQuery qryFile)
        {
            if (Settings.LocalReadOnlyMode)
                return FileServerResult.Failure;
            if (Settings.OverwriteExistingFiles)
                DeleteFileLocal(qryFile);
            using (var writer = new System.IO.StreamWriter(GetAndCreateLocalDiskPath(qryFile)))
            {
                stmFile.Seek(0, System.IO.SeekOrigin.Begin);
                stmFile.CopyTo(writer.BaseStream);
                writer.Close();
            }
            return FileServerResult.Successful;
        }
        public Task<FileServerResult> WriteFileLocalAsync(System.IO.Stream stmFile, IFileQuery qryFile)
        {
            return Task.Factory.StartNew(() =>
            {
                return WriteFileLocal(stmFile, qryFile);
            });
        }

        public FileServerResult WriteFileLocal(string strSourceFilePath, IFileQuery qryFile)
        {
            if (Settings.LocalReadOnlyMode)
                return FileServerResult.Failure;
            System.IO.File.Copy(strSourceFilePath, GetAndCreateLocalDiskPath(qryFile), Settings.OverwriteExistingFiles);
            return FileServerResult.Successful;
        }
        public Task<FileServerResult> WriteFileLocalAsync(string strSourceFilePath, IFileQuery qryFile)
        {
            return Task.Factory.StartNew(() =>
            {
                return WriteFileLocal(strSourceFilePath, qryFile);
            });
        }

        public FileServerResult WriteFileFromStringLocal(string strFileBody, IFileQuery qryFile, System.Text.Encoding encoding = null)
        {
            if (Settings.LocalReadOnlyMode)
                return FileServerResult.Failure;
            if (encoding != null)
                System.IO.File.WriteAllText(GetAndCreateLocalDiskPath(qryFile), strFileBody, encoding);
            else
                System.IO.File.WriteAllText(GetAndCreateLocalDiskPath(qryFile), strFileBody);
            return FileServerResult.Successful;
        }
        public Task<FileServerResult> WriteFileFromStringLocalAsync(string strFileBody, IFileQuery qryFile)
        {
            return Task.Factory.StartNew(() =>
            {
                return WriteFileFromStringLocal(strFileBody, qryFile);
            });
        }

        public FileServerResult WriteImageLocal(System.IO.Stream stmImage, IFileQuery qryFile, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            if (Settings.LocalReadOnlyMode)
                return FileServerResult.Failure;
            if (Settings.OverwriteExistingFiles)
                DeleteFileLocal(qryFile);
            using (Image img = System.Drawing.Image.FromStream(stmImage))
            {
                if (enuFormat != null)
                    img.Save(GetAndCreateLocalDiskPath(qryFile), enuFormat);
                else
                    img.Save(GetAndCreateLocalDiskPath(qryFile));
            }
            return FileServerResult.Successful;
        }
        public Task<FileServerResult> WriteImageLocalAsync(System.IO.Stream stmImage, IFileQuery qryFile, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return WriteImageLocal(stmImage, qryFile, enuFormat);
            });
        }

        public FileServerResult WriteImageLocal(Image objImage, IFileQuery qryFile, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            if (Settings.LocalReadOnlyMode)
                return FileServerResult.Failure;
            if (Settings.OverwriteExistingFiles)
                DeleteFileLocal(qryFile);
            if (enuFormat != null)
                objImage.Save(GetAndCreateLocalDiskPath(qryFile), enuFormat);
            else
                objImage.Save(GetAndCreateLocalDiskPath(qryFile));
            return FileServerResult.Successful;
        }
        public Task<FileServerResult> WriteImageLocalAsync(Image objImage, IFileQuery qryFile, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return WriteImageLocal(objImage, qryFile, enuFormat);
            });
        }

        public FileServerResult WriteImageLocal(string strSourceImagePath, IFileQuery qryFile)
        {
            if (Settings.LocalReadOnlyMode)
                return FileServerResult.Failure;
            System.IO.File.Copy(strSourceImagePath, GetAndCreateLocalDiskPath(qryFile), Settings.OverwriteExistingFiles);
            return FileServerResult.Successful;
        }
        public Task<FileServerResult> WriteImageLocalAsync(string strSourceImagePath, IFileQuery qryFile)
        {
            return Task.Factory.StartNew(() =>
            {
                return WriteImageLocal(strSourceImagePath, qryFile);
            });
        }

        #endregion

        #region CDN Methods (must be overridden)
        public virtual string GetCDNURL(IFileQuery qryFile)
        {
            throw new NotImplementedException("No remote server has been specified for file retrieval");
        }

        public virtual bool FileExpiredLocal(IFileQuery qryFile)
        {
            throw new NotImplementedException("No remote server has been specified to check expiration");
        }

        public virtual bool FileExistsInCDN(IFileQuery qryFile)
        {
            throw new NotImplementedException("No remote server has been specified to check existance");
        }

        public virtual FileProperties GetFilePropertiesFromCDN(IFileQuery qryFile)
        {
            throw new NotImplementedException("No remote server has been specified to check file dates");
        }
        #endregion

        #region Virtual Load Methods
        public virtual System.IO.Stream LoadFileStream(IFileQuery qryFile)
        {
            return LoadFileStreamLocal(qryFile);
        }

        public virtual byte[] LoadFileBytes(IFileQuery qryFile)
        {
            return LoadFileBytesLocal(qryFile);
        }

        public virtual string LoadFileText(IFileQuery qryFile)
        {
            return LoadFileTextLocal(qryFile);
        }

        public virtual string LoadFileText(IFileQuery qryFile, System.Text.Encoding encoding)
        {
            return LoadFileTextLocal(qryFile, encoding);
        }

        public virtual Image LoadImage(IFileQuery qryFile)
        {
            return LoadImageLocal(qryFile);
        }
        #endregion

        #region Virtual Methods

        public virtual FileServerResult StoreFile(System.IO.Stream stmFile, IFileQuery qryFileDestination)
        {
            return WriteFileLocal(stmFile, qryFileDestination);
        }
        public virtual Task<FileServerResult> StoreFileAsync(System.IO.Stream stmFile, IFileQuery qryFileDestination)
        {
            return Task.Factory.StartNew(() =>
            {
                return StoreFile(stmFile, qryFileDestination);
            });
        }

        public virtual FileServerResult StoreFile(string strSourceFilePath, IFileQuery qryFileDestination)
        {
            return WriteFileLocal(strSourceFilePath, qryFileDestination);
        }
        public virtual Task<FileServerResult> StoreFileAsync(string strSourceFilePath, IFileQuery qryFileDestination)
        {
            return Task.Factory.StartNew(() =>
            {
                return StoreFile(strSourceFilePath, qryFileDestination);
            });
        }

        public virtual FileServerResult StoreFileFromString(string strFileBody, IFileQuery qryFileDestination, System.Text.Encoding encoding = null)
        {
            return WriteFileFromStringLocal(strFileBody, qryFileDestination, encoding);
        }
        public virtual Task<FileServerResult> StoreFileFromStringAsync(string strSourceFilePath, IFileQuery qryFileDestination, System.Text.Encoding encoding = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return StoreFileFromString(strSourceFilePath, qryFileDestination, encoding);
            });
        }

        public virtual FileServerResult StoreImage(Image objImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            return WriteImageLocal(objImage, qryFileDestination, enuFormat);
        }
        public virtual Task<FileServerResult> StoreImageAsync(Image objImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return StoreImage(objImage, qryFileDestination, enuFormat);
            });
        }

        public virtual FileServerResult StoreImage(System.IO.Stream stmImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            return WriteImageLocal(stmImage, qryFileDestination, enuFormat);
        }
        public virtual Task<FileServerResult> StoreImageAsync(System.IO.Stream stmImage, IFileQuery qryFileDestination, System.Drawing.Imaging.ImageFormat enuFormat = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return StoreImage(stmImage, qryFileDestination, enuFormat);
            });
        }

        public virtual FileServerResult StoreImage(string strSourceImagePath, IFileQuery qryFileDestination)
        {
            return WriteImageLocal(strSourceImagePath, qryFileDestination);
        }
        public virtual Task<FileServerResult> StoreImageAsync(string strSourceImagePath, IFileQuery qryFileDestination)
        {
            return Task.Factory.StartNew(() =>
            {
                return StoreImage(strSourceImagePath, qryFileDestination);
            });
        }

        public virtual FileServerResult Delete(IFileQuery qryFile)
        {
            return DeleteFileLocal(qryFile);
        }
        public virtual Task<FileServerResult> DeleteAsync(IFileQuery qryFile)
        {
            return Task.Factory.StartNew(() =>
            {
                return Delete(qryFile);
            });
        }

        #endregion

        #region Misc
        protected void CleanupPaths()
        {
            if (!String.IsNullOrEmpty(Settings.LocalHostedURL))
                Settings.LocalHostedURL = CleanPathSegment(Settings.LocalHostedURL);
            if (!String.IsNullOrEmpty(Settings.LocalStoragePath))
                Settings.LocalStoragePath = CleanPathSegment(Settings.LocalStoragePath);
            if (!String.IsNullOrEmpty(Settings.CDNStorageBucket))
                Settings.CDNStorageBucket = CleanPathSegment(Settings.CDNStorageBucket);
        }

        protected string CleanPathSegment(string strPath)
        {
            if (!String.IsNullOrEmpty(strPath))
            {
                strPath = strPath.Trim('/');
                if (!System.IO.Path.IsPathRooted(strPath))
                {
                    strPath = strPath.Replace('\\', '/');
                    strPath = strPath.Replace("://", ":::TEMP:::");
                    strPath = strPath.Replace("//", "/");
                    strPath = strPath.Replace(":::TEMP:::", "://");
                }
            }
            return strPath;
        }

        public static string MapPathLocal(string strPath)
        {
            if (System.IO.Path.IsPathRooted(strPath))
                return System.IO.Path.GetFullPath(strPath);

            if (System.Web.HttpContext.Current != null)
                return System.Web.HttpContext.Current.Server.MapPath("/" + strPath);
            return System.IO.Path.Combine(GetAppDirectory(), strPath.Replace("~", string.Empty).Replace('/', '\\'));
        }

        #region GetAppDirectory
        /// <summary>
        /// Returns the root directory of the current application
        /// </summary>
        public static string GetAppDirectory()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory.Replace("/", "\\");
        }
        #endregion

        #endregion

    }
}
