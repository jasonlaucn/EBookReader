using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EBookReader.Biz;
using EBookReader.Model;
using EBookReader.Utils;
using Microsoft.Win32;

namespace EBookReader
{
    /// <summary>
    /// WebBookDownloader.xaml 的交互逻辑
    /// </summary>
    public partial class WebBookDownloader : INotifyPropertyChanged
    {
        private WebBook _book;

        private List<BookInfo> _bookFiles = new List<BookInfo>();

        public List<BookInfo> BookFiles
        {
            get { return _bookFiles; }
            set
            {
                _bookFiles = value;
                OnPropertyChanged("BookFiles");
            }
        }

        private BookInfo _bookInfo;

        public WebBookDownloader()
        {
            InitializeComponent();
        }

        public void SetOpacity(double o)
        {
            this.Opacity = o;
        }

        private void BtnCloseOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnDownloadOnClick(object sender, RoutedEventArgs e)
        {
            var url = TbUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                TbState.Text = "Url Error";
                return;
            }
            if (!url.StartsWith("http"))
            {
                url = "http://" + url;
            }
            _book = new WebBook(url);
            if (_book.CatalogInfos == null || _book.CatalogInfos.Count == 0)
            {
                TbState.Text = "Failed";
                return;
            }
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "(*.txt)|*.txt",
                FileName = string.IsNullOrEmpty(_book.Title) ? Guid.NewGuid().GetHashCode().ToString() : _book.Title
            };
            if (saveFileDialog.ShowDialog(this) != true)
            {
                return;
            }
            var name = FileHelper.GetFileNameNoneExt(saveFileDialog.FileName);
            _bookInfo = new BookInfo
            {
                BookName = name,
                FilePath = saveFileDialog.FileName
            };
            BtnDownload.IsEnabled = false;
            new Thread(DownloadBook).Start();
        }

        private void DownloadBook()
        {
            if (File.Exists(_bookInfo.FilePath))
            {
                File.Delete(_bookInfo.FilePath);
            }
            FileHelper.CreateFile(_bookInfo.FilePath);
            using (var fs = new StreamWriter(_bookInfo.FilePath, false, Encoding.UTF8))
            {
                double count = _book.CatalogInfos.Count;
                var i = 1;
                foreach (var info in _book.CatalogInfos)
                {
                    var chapter = new WebBook(info.Url);
                    fs.WriteLine(info.Name);
                    fs.WriteLine(chapter.Content);
                    Dispatcher.Invoke(() =>
                        {
                            Downloadbar.Value = i/count*100;
                        });
                    i++;
                }
                fs.Close();
            }
            Dispatcher.Invoke(() =>
                {
                    if (BookFiles.All(o => !o.FilePath.Equals(_bookInfo.FilePath)))
                    {
                        BookFiles.Add(_bookInfo);
                    }
                    BtnDownload.IsEnabled = true;
                    TbState.Text = "Complated";
                });
        }
        
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private void BtnImportOnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var bookInfo = btn.DataContext as BookInfo;
            ConfigSevice.SaveBook(ref bookInfo);
            btn.IsEnabled = false;
        }
    }
}
