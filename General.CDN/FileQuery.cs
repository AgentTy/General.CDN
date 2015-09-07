using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace General.CDN
{
    public interface IFileQuery
    {
        string FileName { get; set; }
        string Folder { get; set; }
        string SubFolder { get; set; }
        Dictionary<string, string> MetaData { get; set; }
    }

    [DataContract]
    public class FileQuery : Model.JsonObject, IFileQuery
    {
        public FileQuery()
        {

        }

        public FileQuery(string FileName, string Folder)
        {
            this.FileName = FileName;
            this.Folder = Folder;
        }

        public FileQuery(string FileName, string Folder, string SubFolder)
        {
            this.FileName = FileName;
            this.Folder = Folder;
            this.SubFolder = SubFolder;
        }

        [DataMember(Name = "FileName")]
        public string FileName { get; set; }
        [DataMember(Name = "Folder")]
        public string Folder { get; set; }
        [DataMember(Name = "SubFolder")]
        public string SubFolder { get; set; }

        private Dictionary<string, string> _objMetaData = new Dictionary<string, string>();
        [IgnoreDataMember]
        public Dictionary<string, string> MetaData
        {
            get { return _objMetaData; }
            set { _objMetaData = value; }
        }

    }

}
