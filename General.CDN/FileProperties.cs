using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.CDN
{
    public class FileProperties
    {
        public FileProperties()
        {

        }

        public long Length { get; set; }
        public string ContentType { get; set; }
        public string ContentMD5 { get; set; }
        public string ETag { get; set; }
        public string VersionNumber { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public DateTimeOffset? Created { get; set; }
        public string URL { get; set; }

        public object Source { get; set; }

        public bool IsCurrentVersionOf(FileProperties fileMaster)
        {
            //First line of defense, different size means different versions
            if (this.Length != fileMaster.Length)
                return false;

            //Modify date is not reliable because the local file might retain the original modify date, while the remote CDN only stores a Create Date (the moment it was stored to the CDN)
            //if (this.LastModified.HasValue && fileMaster.LastModified.HasValue)
            //    if (fileMaster.LastModified > this.LastModified)
            //        return false;

            //Apparently this isn't reliable either
            //if (this.Created.HasValue && fileMaster.Created.HasValue)
            //    if (fileMaster.Created > this.Created)
            //        return false;

            //Now I'll do an MD5 check in some cases
            if (String.IsNullOrEmpty(this.ContentMD5) && !String.IsNullOrEmpty(fileMaster.ContentMD5) && this.Source is System.IO.FileInfo)
            {
                if (this.Length < 10000000) //10 MB
                {
                    //Load MD5 for local file
                    System.IO.FileInfo objInfo = (System.IO.FileInfo) this.Source;
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        using (var stream = System.IO.File.OpenRead(objInfo.FullName))
                        {
                            this.ContentMD5 = Convert.ToBase64String(md5.ComputeHash(stream));
                        }
                    }
               } 
            }
            if (!String.IsNullOrEmpty(this.ContentMD5) && !String.IsNullOrEmpty(fileMaster.ContentMD5))
                if (fileMaster.ContentMD5 != this.ContentMD5)
                    return false; //MD5 hash didn't match

            //We'll assume they are the same, this will be accurate most of the time
            return true;
        }
    }
}
