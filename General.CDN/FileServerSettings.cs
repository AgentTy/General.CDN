using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General.CDN
{
    public class FileServerSettings
    {
        private bool _blnOverwriteExistingFiles = true;
        public bool OverwriteExistingFiles { get { return _blnOverwriteExistingFiles; } set { _blnOverwriteExistingFiles = value; } }

        public string LocalHostedURL { get; set; }
        public string LocalStoragePath { get; set; }
        public bool LocalReadOnlyMode { get; set; }
        public string CDNStorageBucket { get; set; }

        public string GlobalFileNamePrepend { get; set; }
        public string GlobalFilePathPrepend_Local { get; set; }
        public string GlobalFilePathAppend_Local { get; set; }
        public string GlobalFilePathPrepend_CDN { get; set; }
        public string GlobalFilePathAppend_CDN { get; set; }
    }
}
