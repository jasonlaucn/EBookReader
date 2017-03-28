using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EBookReader.Utils;

namespace EBookReader.Biz
{
    /// <summary>  
    /// 公共方法类  (For WebSite:http://www.37zw.com)
    /// </summary>  
    public class WebBook
    {
        private const string CONTENTSTART = "<div id=\"content\">";
        private const string DIVEND = "</div>";
        private const string INFOSTART = "<div id=\"info\">";
        private const string TITLESTART = "<h1>";
        private const string TITLEEND = "</h1>";
        private const string CATALOGSTART = "<div id=\"list\">";
        private const string CATALOGITMESTART = "<dd>";
        private const string CATALOGITMEEND = "</dd>";
        private const string CATALOGCLEARREGEX = "<dt.*?>(.*?)</dt>|<dd><a.*href=\"|&nbsp;|\\r\\n|\\t|#|@";

        private string _url;
        private string _htmlStr;
        private string _content;
        private string _title;
        private List<WebCatalogInfo> _catalogInfos;

        public string Title
        {
            get
            {
                if (!string.IsNullOrEmpty(_htmlStr) && _title == null)
                {
                    _title = GetTitle(_htmlStr);
                }
                return _title;
            }
        }

        public string Content { 
            get{
                if (!string.IsNullOrEmpty(_htmlStr) && _content == null)
                {
                    _content = GetContent(_htmlStr);
                }
                return _content;
            } 
        }

        public List<WebCatalogInfo> CatalogInfos
        {
            get
            {
                if (!string.IsNullOrEmpty(_htmlStr) && _catalogInfos == null)
                {
                    _catalogInfos = GetCatalogs(_htmlStr);
                }
                return _catalogInfos;
            }
        }

        public WebBook(string url)
        {
            _url = url;
            _htmlStr = WebHelper.GetHtmlStr(url);
            //if (string.IsNullOrEmpty(_htmlStr))
            //{
            //    return;
            //}
            //_catalogInfos = GetCatalogs(_htmlStr);
            //_title=GetTitle(_htmlStr);
            //_content = GetContent(_htmlStr);
        }

        private string GetContent(string htmlStr)
        {
            var s = htmlStr.IndexOf(CONTENTSTART);
            if (s == -1)
            {
                return string.Empty;
            }
            var e = htmlStr.IndexOf(DIVEND, s);
            if (e == -1)
            {
                return string.Empty;
            }
            return htmlStr.Substring(s + CONTENTSTART.Length, e - DIVEND.Length - s).Replace("&nbsp;", " ").Replace("<br />", "\r\n");
        }

        private List<WebCatalogInfo> GetCatalogs(string htmlStr)
        {
            var catalogs = new List<WebCatalogInfo>();
            var s = htmlStr.IndexOf(CATALOGSTART);
            if (s == -1)
            {
                return catalogs;
            }
            var e = htmlStr.IndexOf(DIVEND, s);
            if (e == -1)
            {
                return catalogs;
            }
            htmlStr = htmlStr.Substring(s, e - s);
            var ls = htmlStr.IndexOf(CATALOGITMESTART);
            if (ls == -1)
            {
                return catalogs;
            }
            var le = htmlStr.LastIndexOf(CATALOGITMEEND);
            if (le == -1)
            {
                return catalogs;
            }
            htmlStr = htmlStr.Substring(ls, le + CATALOGITMEEND.Length - ls);
            htmlStr = Regex.Replace(htmlStr, CATALOGCLEARREGEX, "", RegexOptions.Compiled);
            htmlStr = htmlStr.Replace("</a></dd>", "@").Replace("\">", "#").TrimEnd('@');
            var list = htmlStr.Split('@');
            if (list.Length > 1)
            {
                return list.Select(s1 => s1.Split('#')).Select(itme => new WebCatalogInfo
                    {
                        Url = _url+itme[0],
                        Name = itme[1]
                    }).ToList();
            }
            return catalogs;
        }

        private string GetTitle(string htmlStr)
        {
            var s = htmlStr.IndexOf(INFOSTART);
            if (s == -1)
            {
                return string.Empty;
            }
            var e = htmlStr.IndexOf(DIVEND, s);
            if (e == -1)
            {
                return string.Empty;
            }
            htmlStr = htmlStr.Substring(s, e - s);
            var ls = htmlStr.IndexOf(TITLESTART);
            if (ls == -1)
            {
                return string.Empty;
            }
            var le = htmlStr.LastIndexOf(TITLEEND);
            if (le == -1)
            {
                return string.Empty;
            }
            return htmlStr.Substring(ls, le + CATALOGITMEEND.Length - ls)
                             .Replace(TITLESTART, "")
                             .Replace(TITLEEND, "");
        }
    } 

    public class WebCatalogInfo
    {
        public string Url { get; set; }
        public string Name { get; set; }
    }
}
