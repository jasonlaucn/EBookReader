using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows.Documents;
using EBookReader.Model;
using EBookReader.Utils;

namespace EBookReader
{
    public class ConfigSevice
    {
        private const string CONFIGKEY = "Config";
        private const string CURRENTKEY = "Current";
        private const string BOOKIDITEMNAME = "BookId";
        private const string FILEPATHITEMNAME = "FilePath";
        private const string BOOKNAMEITEMNAME = "BookName";
        private const string CHAPTERNAMEITEMNAME = "ChapterName";
        private const string CHAPTERINDEXITEMNAME = "ChapterIndex";
        private const string CHAPTEROFFSETITEMNAME = "ChapterOffset";

        public const string DEFAULTTXTREGEX1 = @"^(第.*?章\s*)|^序\s+|^序$|^前言\s+|^前言$|^后记\s|^后记$";
        public const string DEFAULTTXTREGEX2 = @"^(\d+\.\s*)|^序\s+|^序$|^前言\s+|^前言$|^后记\s|^后记$";
        public const string REGEXITMENAME = "Regex";
        public const string BOOKEXT = ".book";

        public static string ConfigFile = AppDomain.CurrentDomain.BaseDirectory + @"\Config.ini";
        public static string BookDir = AppDomain.CurrentDomain.BaseDirectory + @"\Books\";

        public static T GetAppSetting<T>(AppSettingKey key, object defaultValue = null)
        {
            var asr = new AppSettingsReader();
            var value = asr.GetValue(key.ToString(), typeof (T));
            if (value == null)
            {
                return (T)defaultValue;
            }
            return (T) value;
        }

        public static string GetConfig(ConfigKey key,string defaultValue = "")
        {
            if (!File.Exists(ConfigFile))
            {
                return defaultValue;
            }
            return IniUtils.Read(ConfigFile, CONFIGKEY, key.ToString(), defaultValue).Trim();
        }

        public static void SetConfig(ConfigKey key, string value)
        {
            if (!File.Exists(ConfigFile))
            {
                using (var fs = File.Create(ConfigFile))
                {
                    fs.Close();
                }
            }
            IniUtils.Write(ConfigFile, CONFIGKEY, key.ToString(), value);
        }

        public static List<BookInfo> GetAllBooks()
        {
            List<BookInfo> infos = new List<BookInfo>();
            var sections = IniUtils.ReadSections(ConfigFile);
            foreach (var section in sections)
            {
                if (section != CURRENTKEY && section != CONFIGKEY)
                {
                    BookInfo info;
                    if (GetBookInfo(section, out info))
                    {
                        infos.Add(info);
                    }
                }
            }
            return infos;
        }

        public static bool GetCurrentBookInfo(out BookInfo info)
        {
            info = null;
            if (!File.Exists(ConfigFile))
            {
                return false;
            }
            var id = IniUtils.Read(ConfigFile, CURRENTKEY, BOOKIDITEMNAME).Trim();
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            return GetBookInfo(id,out info);
        }

        private static bool GetBookInfo(string id, out BookInfo info)
        {
            info = null;
            var filePath = IniUtils.Read(ConfigFile, id, FILEPATHITEMNAME).Trim();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                IniUtils.Write(ConfigFile, id, "", "");
                return false;
            }
            int index;
            double offset;
            int.TryParse(IniUtils.Read(ConfigFile, id, CHAPTERINDEXITEMNAME, "0").Trim(), out index);
            double.TryParse(IniUtils.Read(ConfigFile, id, CHAPTEROFFSETITEMNAME, "0").Trim(), out offset);
            var name = IniUtils.Read(ConfigFile, id, BOOKNAMEITEMNAME).Trim();
            var itemName = IniUtils.Read(ConfigFile, id, CHAPTERNAMEITEMNAME).Trim();
            info = new BookInfo()
                {
                    BookId = id,
                    BookName = name,
                    ChapterName = itemName,
                    FilePath = filePath,
                    ChapterIndex = index,
                    ChapterOffset = offset
                };
            return true;
        }


        public static void DelBook(string id)
        {
            IniUtils.WriteSection(ConfigFile, id, "");
        }

        public static void SaveCurrentBook(BookInfo info)
        {
            if (!File.Exists(ConfigFile))
            {
                using (var fs = File.Create(ConfigFile))
                {
                    fs.Close();
                }
            }
            if (!string.IsNullOrEmpty(info.FilePath))
            {
                IniUtils.Write(ConfigFile, CURRENTKEY, BOOKIDITEMNAME, info.BookId);
                SaveBookData(info);
                return;
            }
            IniUtils.Write(ConfigFile, CURRENTKEY, BOOKIDITEMNAME, "");
        }
        public static void SaveBook(ref BookInfo info)
        {
            if (string.IsNullOrEmpty(info.FilePath))
            {
                return;
            }
            if (!File.Exists(ConfigFile))
            {
                using (var fs = File.Create(ConfigFile))
                {
                    fs.Close();
                }
            }
            var id = FileHelper.StringToMD5(info.FilePath);
            info.BookId = id;
            SaveBookData(info);
        }

        private static void SaveBookData(BookInfo info)
        {
            //文件地址
            IniUtils.Write(ConfigFile, info.BookId, FILEPATHITEMNAME, info.FilePath);
            IniUtils.Write(ConfigFile, info.BookId, CHAPTERINDEXITEMNAME, info.ChapterIndex.ToString());
            IniUtils.Write(ConfigFile, info.BookId, CHAPTEROFFSETITEMNAME, info.ChapterOffset.ToString());
            IniUtils.Write(ConfigFile, info.BookId, BOOKNAMEITEMNAME, info.BookName);
            IniUtils.Write(ConfigFile, info.BookId, CHAPTERNAMEITEMNAME, info.ChapterName);
        }
    }
}
