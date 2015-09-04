using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.Runtime.Serialization;
using General;

namespace General.Model
{
	/// <summary>
	/// URL Address Class
	/// </summary>
	[Serializable, DataContract]
	public class URL
	{
		#region Private Variables
		private bool _blnValid;
		private string _strSource;
		private string _strVariable;
		#endregion

		#region Constructors
		/// <summary>
		/// URL
		/// </summary>
		public URL()
		{
			
		}

		/// <summary>
		/// URL
		/// </summary>
		public URL(string URL)
		{
			SetURL(URL);
		}

		/// <summary>
		/// URL
		/// </summary>
		public URL(string URL, string Variable)
		{
			_strVariable = Variable;
			SetURL(URL);
		}

		/// <summary>
		/// URL
		/// </summary>
		public URL(object DataCell)
		{
			if(DataCell == null)
			{
				_blnValid = false;
				_strSource = "";
			}
			else if(Convert.IsDBNull(DataCell))
			{
				_blnValid = false;
				_strSource = "";
			}
			else
			{
				SetURL(DataCell.ToString());
			}
		}
		#endregion

		#region Private Functions
		/// <summary>
		/// Overrides the URL address and parses the new string
		/// </summary>
		private void SetURL(string strURL) {
			_strSource = strURL;
            if (!StringFunctions.IsNullOrWhiteSpace(_strSource))
                _strSource = _strSource.Trim(); //Remove whitespace characters
            _blnValid = IsValid(_strSource);
		}
		#endregion

		#region Public Methods

        #region IsValid
        /// <summary>
        /// Checks if the URL address is valid
        /// </summary>
        /// <param name="strURL">string - A potential URL address</param>
        /// <returns>bool</returns>
        public static bool IsValid(string strURL)
        {

            if (StringFunctions.IsNullOrWhiteSpace(strURL))
                return false;

            string strURLRegEx = "^(https?://)?"
                //+ "(([0-9a-z_!~*'().&=+$%-]+: )?[0-9a-z_!~*'().&=+$%-]+@)?" //user@ 
                + @"(([0-9]{1,3}\.){3}[0-9]{1,3}" // IP- 199.194.52.184 
                + "|" // allows either IP or domain 
                + @"([0-9a-z_!~*'()-]+\.)*" // tertiary domain(s)- www. 
                + @"([0-9a-z][0-9a-z-]{0,61})?[0-9a-z]\." // second level domain 
                + "[a-z]{2,24})" // first level domain- .com or .museum or .photography
                + "(:[0-9]{1,4})?" // port number- :80 
                + "((/?)|" // a slash isn't required if there is no file name 
                + "(/[0-9a-z_!~*'().;?:@&=+$,%#-]+)+/?)$";

            Regex regxURL = new Regex(strURLRegEx, RegexOptions.IgnoreCase);

            if (strURL.StartsWith("javascript:"))
                return true;

            strURL = strURL.Replace("../", "");

            if (strURL.Contains("?"))
            {
                strURL = strURL.Replace(StringFunctions.AllAfter(strURL, "?"), "");
                strURL = StringFunctions.Shave(strURL, 1);
            }

            if (strURL.Contains("#"))
            {
                strURL = strURL.Replace(StringFunctions.AllAfter(strURL, "#"), "");
                strURL = StringFunctions.Shave(strURL, 1);
            }

            if (strURL.Contains("_"))
            {
                strURL = strURL.Replace("_", "");
            }

            if (strURL.StartsWith("/"))
                strURL = strURL.TrimStart('/');

            return regxURL.IsMatch(strURL);
        }
        #endregion

        #region ForceValidURLFileName (for FileName, not whole URL)
        public static string ForceValidURLFileName(string strURL)
        {
            strURL = strURL.Replace(":", "");
            strURL = strURL.Replace(",", "");
            strURL = strURL.Replace("\\", "");
            strURL = strURL.Replace("'", "");
            strURL = strURL.Replace("\"", "");
            strURL = strURL.Replace("/", "_");
            strURL = strURL.Replace(" ", "_");

            strURL = StringFunctions.ForceAlphaNumeric(strURL, true);

            return strURL;
        }
        #endregion

        #region FormatURL
        public static string FormatURL(string strURL)
        {
            if (!StringFunctions.IsNullOrWhiteSpace(strURL))
            {

                string strAnchorAppend = String.Empty;
                if (strURL.Contains("#"))
                {
                    strAnchorAppend = StringFunctions.AllAfter(strURL, "#");
                    strURL = strURL.Replace(strAnchorAppend, "");
                    strURL = StringFunctions.Shave(strURL, 1);
                }

                #region Relative URL Test
                string strRelativeURLTest = strURL;
                strRelativeURLTest = StringFunctions.AllAfterReverse(strRelativeURLTest, "../");
                if (strRelativeURLTest.Contains("?"))
                    strRelativeURLTest = StringFunctions.AllBetween(strRelativeURLTest, ".", "?");
                else
                    strRelativeURLTest = StringFunctions.AllAfterReverse(strRelativeURLTest, ".");

                switch (strRelativeURLTest)
                {
                    case "aspx":
                    case "html":
                    case "htm":
                    case "pdf":
                    case "jpg":
                    case "gif":
                    case "png":
                    case "exe":
                    case "zip":
                        return strURL;
                }
                #endregion

                #region Local URL Test
                string strLocalURLTest = strURL;
                string[] aryParts;
                aryParts = strLocalURLTest.Split('.');
                if (aryParts.Length == 2)
                {
                    if (strLocalURLTest.Contains("?"))
                        strLocalURLTest = StringFunctions.AllBetween(strLocalURLTest, ".", "?");
                    else
                        strLocalURLTest = StringFunctions.AllAfterReverse(strLocalURLTest, ".");

                    switch (strLocalURLTest)
                    {
                        case "aspx":
                        case "html":
                        case "htm":
                        case "pdf":
                        case "jpg":
                        case "gif":
                        case "png":
                        case "exe":
                        case "zip":
                            return strURL;
                    }
                }
                #endregion

                #region Validate External URL
                /*
                 * I don't think this is a good idea anymore. Not all sites start with www.
                if (!strURL.Contains("www."))
                    if (StringFunctions.Count(strURL, ".") == 1 && !strURL.StartsWith("http://"))
                        strURL = "www." + strURL;
                */
                if (!strURL.Contains("://") && !strURL.Contains("javascript:"))
                    strURL = "http://" + strURL;
                #endregion

                #region Append Anchor
                if (!StringFunctions.IsNullOrWhiteSpace(strAnchorAppend))
                    strURL = strURL + "#" + strAnchorAppend;
                #endregion

            }

            return strURL;
        }
        #endregion

        #region CheckExists
        public enum URLCheckExistsResult : int
		{
			Exists = 1,
			DoesNotExist = 2,
			Unknown = 3
		}

		public URLCheckExistsResult CheckExists()
		{
            if (Valid)
            {
                if (!_strSource.Contains("://"))
                {
                    return _exists = URLCheckExistsResult.Unknown;
                }

                HttpWebRequest objRequest;
                objRequest = (HttpWebRequest)HttpWebRequest.Create(FormatURL(_strSource));
                objRequest.Timeout = 3000; //Don't spend more than 3 seconds waiting for a response
                objRequest.Proxy = null;
                objRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                HttpWebResponse objResponse = null;
                try
                {
                    objResponse = (HttpWebResponse)objRequest.GetResponse();
                    if (objResponse.StatusCode == HttpStatusCode.OK)
                    {
                        objResponse.Close();
                        _exists = URLCheckExistsResult.Exists;
                    }
                    else
                    {
                        objResponse.Close();
                        _exists = URLCheckExistsResult.DoesNotExist;
                    }
                }
                catch (WebException ex)
                {
                    if(ex.Message == "Too many automatic redirections were attempted.")
                        _exists = URLCheckExistsResult.Exists;
                    if (ex.Message.Contains("500")) //Error
                        _exists = URLCheckExistsResult.Unknown;
                    if (ex.Message.Contains("403")) //Forbidden
                        _exists = URLCheckExistsResult.Unknown;
                    _exists = URLCheckExistsResult.DoesNotExist;
                }
            }
            else
                _exists = URLCheckExistsResult.DoesNotExist;

            return _exists;
        }
        #endregion

        #region CheckRedirect
        public enum URLCheckRedirectResult : int
        {
            NoRedirect = 1,
            TemporaryRedirect = 2,
            PermanentRedirect = 3,
            Unknown = 4
        }

        public URLCheckRedirectResult CheckRedirect()
        {
            string strNewURL;
            return CheckRedirect(out strNewURL);
        }

        public URLCheckRedirectResult CheckRedirect(out string strNewURL)
        {
            strNewURL = String.Empty;
            if (Valid)
            {
                HttpWebRequest objRequest;
                objRequest = (HttpWebRequest)HttpWebRequest.Create(FormatURL(_strSource));
                objRequest.AllowAutoRedirect = false;
                objRequest.Proxy = null;
                objRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                try
                {
                    HttpWebResponse objResponse = null;
                    objResponse = (HttpWebResponse)objRequest.GetResponse();
                    if (objResponse.StatusCode == HttpStatusCode.MovedPermanently)
                    {
                        strNewURL = objResponse.Headers["Location"];
                        objResponse.Close();
                        return URLCheckRedirectResult.PermanentRedirect;
                    }
                    else if (objResponse.StatusCode == HttpStatusCode.Moved)
                    {
                        strNewURL = objResponse.Headers["Location"];
                        objResponse.Close();
                        return URLCheckRedirectResult.PermanentRedirect;
                    }
                    else if (objResponse.StatusCode == HttpStatusCode.Redirect)
                    {
                        strNewURL = objResponse.Headers["Location"];
                        objResponse.Close();
                        return URLCheckRedirectResult.TemporaryRedirect;
                    }
                    else
                    {
                        objResponse.Close();
                        return URLCheckRedirectResult.NoRedirect;
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse objResponse = (HttpWebResponse) ex.Response;
                    if (objResponse == null)
                    {
                        //General.Debugging.Report.SendError("Error in CheckRedirect for: " + _strSource, ex);
                        throw;
                    }
                    if (objResponse.StatusCode == HttpStatusCode.MovedPermanently)
                    {
                        strNewURL = objResponse.Headers["Location"];
                        objResponse.Close();
                        return URLCheckRedirectResult.PermanentRedirect;
                    }
                    else if (objResponse.StatusCode == HttpStatusCode.Moved)
                    {
                        strNewURL = objResponse.Headers["Location"];
                        objResponse.Close();
                        return URLCheckRedirectResult.PermanentRedirect;
                    }
                    else if (objResponse.StatusCode == HttpStatusCode.Redirect)
                    {
                        strNewURL = objResponse.Headers["Location"];
                        objResponse.Close();
                        return URLCheckRedirectResult.TemporaryRedirect;
                    }
                    else
                    {
                        objResponse.Close();
                        return URLCheckRedirectResult.NoRedirect;
                    }
                }
            }
            else
                return URLCheckRedirectResult.Unknown;
        }
        #endregion

        #region CheckFor404
        public enum URLCheck404Result : int
        {
            NotFound404 = 1,
            Other = 2,
            Unknown = 3
        }

        public URLCheck404Result CheckFor404()
        {
            if (Valid)
            {
                HttpWebRequest objRequest;
                objRequest = (HttpWebRequest)HttpWebRequest.Create(FormatURL(_strSource));
                objRequest.Proxy = null;
                objRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                try
                {
                    HttpWebResponse objResponse = null;
                    objResponse = (HttpWebResponse)objRequest.GetResponse();
                    if (objResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        objResponse.Close();
                        return URLCheck404Result.NotFound404;
                    }
                    else
                    {
                        objResponse.Close();
                        return URLCheck404Result.Other;
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse objResponse = (HttpWebResponse)ex.Response;
                    if (objResponse == null)
                    {
                        //General.Debugging.Report.SendError("Error in CheckFor404 for: " + _strSource, ex);
                        throw;
                    }
                    if (objResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        objResponse.Close();
                        return URLCheck404Result.NotFound404;
                    }
                    else
                    {
                        objResponse.Close();
                        return URLCheck404Result.Other;
                    }
                }
            }
            else
                return URLCheck404Result.Unknown;
        }
        #endregion

        #region GetSiteRoot
        public URL GetSiteRoot()
        {
            return new URL(this.ToUri().Host);
        }

        public static URL GetSiteRoot(URL urlSrc)
        {
            return new URL(urlSrc.ToUri().Host);
        }
        #endregion

        #region IsPage / IsImage
        public static bool IsPage(string strURL)
        {
            string extension = "";
            try
            {
                extension = System.IO.Path.GetExtension(strURL);
            }
            catch { }

            if (extension.Contains("?"))
                extension = StringFunctions.AllBefore(extension, "?");

            switch (extension)
            {
                case "": //No extension is usually a page

                case ".html": //HTML
                case ".htm":
                case ".xhtml":
                case ".jhtml":

                case ".aspx": //ASP
                case ".asp":
                
                case ".rb": //Ruby
                case ".rhtml":

                case ".php": //PHP
                case ".phtml":
                case ".php4":
                case ".php3":
                case ".shtml":
                
                case ".cfm": //Coldfusion

                case ".jsp": //Java
                case ".jspx":
                case ".do":
                case ".action":
                case ".wss":

                case ".pl": //Perl
                case ".py": //Python
                case ".cgi": //Other
                case ".dll":
                    return true;
                default: return false;
            }

        }

        public static bool IsImage(string strURL)
        {
            string extension = "";
            try
            {
                extension = System.IO.Path.GetExtension(strURL);
            }
            catch { }

            if (extension.Contains("?"))
                extension = StringFunctions.AllBefore(extension, "?");

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".jif":
                case ".jfif":
                case ".png":
                case ".gif":
                case ".bmp":
                case ".tif":
                case ".tiff":
                    return true;
                default: return false;
            }
        }
        #endregion

        #endregion

        #region Output
        /// <summary>
        /// Returns the URL address as a Uri object
        /// </summary>
        public Uri ToUri()
        {
            Uri objUri = new Uri(ToString());
            return objUri;
        }

		/// <summary>
		/// Returns the URL address as a string
		/// </summary>
		public override string ToString()
		{
			return FormatURL(_strSource);
		}

		/// <summary>
		/// Returns the URL address as a string and replaces the predefined variable with a new string
		/// </summary>
		public string ToString(string strReplace)
		{
			return FormatURL(_strSource.Replace(_strVariable, strReplace));
		}

		/// <summary>
		/// Returns the URL address as a sql object
		/// </summary>
		public object ToSql()
		{
			if(_strSource == null)
				return DBNull.Value;
			else if(_strSource == String.Empty)
				return DBNull.Value;
			else
				return FormatURL(_strSource);
		}

		/// <summary>
		/// Returns the URL address as an html link
		/// </summary>
		public string ToLink()
		{
            return "<a href=\"" + FormatURL(_strSource) + "\">" + FormatURL(_strSource) + "</a>";
		}

        public string ToLink(string strLabel)
        {
            if (StringFunctions.IsNullOrWhiteSpace(strLabel))
                strLabel = FormatURL(_strSource);
            return "<a href=\"" + FormatURL(_strSource) + "\">" + strLabel + "</a>";
        }

        public string ToLink(string strLabel, string strTarget)
        {
            if (StringFunctions.IsNullOrWhiteSpace(strLabel))
                strLabel = FormatURL(_strSource);
            return "<a href=" + FormatURL(_strSource) + " target=\"" + strTarget + "\">" + strLabel + "</a>";
        }

        public string ToLink(string strLabel, string strTarget, string strCSSClass)
        {
            if (StringFunctions.IsNullOrWhiteSpace(strLabel))
                strLabel = FormatURL(_strSource);
            return "<a href=\"" + FormatURL(_strSource) + "\" target=\"" + strTarget + "\" class=\"" + strCSSClass + "\">" + strLabel + "</a>";
        }

		#endregion

		#region Public Properties
		/// <summary>
		/// Returns the validation status of the URL address
		/// </summary>
		[DataMember]
        public bool Valid
		{
			get	{return _blnValid;}
		}

		/// <summary>
		/// Returns the URL address as a string
		/// </summary>
		[DataMember]
        public string Value
		{
            get { return ToString(); }
		}

        private URLCheckExistsResult _exists = URLCheckExistsResult.Unknown;
        [DataMember]
        public URLCheckExistsResult Exists
        {
            get
            {
                return _exists;
                //return CheckExists();
                /*if (CheckExists() != URLCheckExistsResult.DoesNotExist)
                    return true;
                else
                    return false;*/
            }
        }

        [DataMember]
        public string ExistsDescription
        {
            get
            {
                return _exists.ToString();
            }
        }


        public string Domain
        {
            get
            {
                return ((Uri)this).Host;
            }
        }
		#endregion

		#region Operators
		/// <summary>
		/// Compares two URL objects
		/// </summary>
		public static bool operator ==(URL URL1, URL URL2)
		{
			if((object) URL1 == null && (object) URL2 != null)
				return false;
			if((object)URL2 == null && (object) URL1 != null)
				return false;
			if((object) URL1 == null && (object) URL2 == null)
				return true;
			return(URL1.ToString() == URL2.ToString());
		}

		/// <summary>
		/// Compares two URL objects
		/// </summary>
		public static bool operator !=(URL URL1, URL URL2)
		{
			if((object) URL1 == null && (object) URL2 != null)
				return true;
			if((object)URL2 == null && (object) URL1 != null)
				return true;
			if((object) URL1 == null && (object) URL2 == null)
				return false;
			return(URL1.ToString() != URL2.ToString());
		}	

		/// <summary>
		/// Casts an URL as a string
		/// </summary>
		public static implicit operator string(URL URL)
		{
            if (URL == null)
                return null;
			try
			{
				return URL.ToString();
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Casts a string as an URL
		/// </summary>
		public static implicit operator URL(string URL)
		{
			return new URL(URL);
		}

        /// <summary>
        /// Casts an URL as a Uri
        /// </summary>
        public static implicit operator Uri(URL URL)
        {
            try
            {
                return URL.ToUri();
            }
            catch
            {
                return null;
            }
        }

		/// <summary>
		/// Compares two URL objects
		/// </summary>
		public override bool Equals(object obj)
		{
			return(this==(URL) obj);
		}


		#endregion

        #region GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

	}
}
