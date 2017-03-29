using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EBookReader.Biz;
using EBookReader.Model;
using EBookReader.Utils;
using Microsoft.Win32;
using Path = System.Windows.Shapes.Path;

namespace EBookReader
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChaptersInfo _currentChapter;
        private BookInfo _bookInfo;
        private bool _autoLoad = true;
        private List<ChaptersInfo> _chapters = new List<ChaptersInfo>();
        private Size _oldSize;
        private string _file;
        private string _txtRegex;
        private bool _regexChanged;
        private Point _p;
        private bool _isResize;
        private FloatingWin _floatingWin;

        public MainWindow()
        {
            InitializeComponent();

        }

        private void BtnOpen(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
                {
                    RestoreDirectory = true,
                    CheckFileExists = true,
                    Multiselect = false,
                    Filter = "(" + ConfigSevice.ImportBookExt + ")|" + ConfigSevice.ImportBookExt
                };
            if (fileDialog.ShowDialog() != true)
            {
                return;
            }
            SaveCurrentBook();
            LsCatalog.ItemsSource = null;
            TbContent.Text = "";
            _bookInfo = null;
            _currentChapter = null;
            _regexChanged = false;
            _autoLoad = false;
            _file = fileDialog.FileName;
            //TbFile.Text = FileHelper.GetFileExtNonePoint(_file);
            new Thread(GetChapters).Start();
        }

        private void BtnBookShelf(object sender, RoutedEventArgs e)
        {
            BdBookShelf.Visibility =Visibility.Visible;
            LoadBookShelft();
        }
        

        private void BookShelfCloseOnClick(object sender, RoutedEventArgs e)
        {
            BdBookShelf.Visibility = Visibility.Collapsed;
        }
        private void BookOnClick(object sender, MouseButtonEventArgs e)
        {
            var book = (sender as Border).DataContext as BookInfo;
            if (book != null)
            {
                if (_bookInfo == null || _bookInfo.BookId != book.BookId)
                {
                    SaveCurrentBook();
                    _file = book.FilePath;
                    _currentChapter = null;
                    LsCatalog.ItemsSource = null;
                    _bookInfo = book;
                    _regexChanged = false;
                    _autoLoad = true;
                    GetChapters();
                }
            }
            BdBookShelf.Visibility = Visibility.Collapsed;
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ConfigSevice.IsShowFloatingWin)
            {
                _floatingWin = new FloatingWin { Owner = this };
                _floatingWin.Left = SystemParameters.VirtualScreenWidth - _floatingWin.Width;
                _floatingWin.Top = 0;
                _floatingWin.Show();
            }
            //var web = new WebBook("http://www.biquge.tw/59_59883/");
            //return;
            LoadConfig();
            _txtRegex = ConfigSevice.DefaultTxtRegex1;
            if (!ConfigSevice.GetCurrentBookInfo(out _bookInfo))
            {
                return;
            }
            _file = _bookInfo.FilePath;
            new Thread(GetChapters).Start();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            //_floatingWin.Close();
            SaveConfig();
            SaveCurrentBook();
        } 
        private void BookOnDel(object sender, MouseButtonEventArgs e)
        {
            var book = (sender as Border).DataContext as BookInfo;
            if (book == null || string.IsNullOrEmpty(book.BookId))
            {
                return;
            }
            ConfigSevice.DelBook(book.BookId);
            if (_bookInfo != null && book.BookId == _bookInfo.BookId)
            {
                _bookInfo = null;
                LsCatalog.ItemsSource = null;
                TbContent.Text = "";
                TbBookName.Text = "";
                TbChapter.Text = "";
                _file = "";
                _chapters.Clear();
                _currentChapter = null;
            }
            LoadBookShelft();
            e.Handled = true;
        }
        private void BtnDel(object sender, RoutedEventArgs e)
        {
            LsCatalog.ItemsSource = null;
            TbContent.Text = "";
            TbBookName.Text = "";
            TbChapter.Text = "";
            _file = "";
            _chapters.Clear();
            _currentChapter = null;
            if (_bookInfo != null && !string.IsNullOrEmpty(_bookInfo.BookId))
            {
                ConfigSevice.DelBook(_bookInfo.BookId);
            }
            _bookInfo = null;
        }

        private void BtnWebDownload(object sender, RoutedEventArgs e)
        {
            var win = new WebBookDownloader();
            win.SetOpacity(Opacity);
            win.Owner = this;
            win.ShowDialog();
        }


        private void SBrightness_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var value = (byte)Math.Round(sBrightness.Value, 0);
            var brush = new SolidColorBrush(Color.FromRgb(value, value, value));
            TbContent.Foreground = brush;
        }

        private void Btn2OnClick(object sender, RoutedEventArgs e)
        {
            ChapterUp();
        }
        private void Btn3OnClick(object sender, RoutedEventArgs e)
        {
            ChapterDown();
        }
        private void Btn5OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void WinStateOnClick(object sender, RoutedEventArgs e)
        {
            WindowState = (sender as CheckBox).IsChecked==true ? WindowState.Maximized : WindowState.Normal;
        }
        private void WinMinOnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CbTitleVisbleOnClick(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
            {
                SpTitle.Visibility = Visibility.Collapsed;
                SpSetting.Visibility = Visibility.Collapsed;
            }
            else
            {
                SpTitle.Visibility = Visibility.Visible;
                SpSetting.Visibility = Visibility.Visible;
            }
        }

        private void CbCatalogVisbleOnClick(object sender, RoutedEventArgs e)
        {
            LsCatalog.Visibility = (sender as CheckBox).IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            UpdateLayout();
            if ((sender as CheckBox).IsChecked == true && _currentChapter != null)
            {
                var offset = LsCatalog.ActualHeight * (LsCatalog.SelectedIndex + 1) / _chapters.Count - 30;
                if (offset > 0)
                {
                    svCatalog.ScrollToVerticalOffset(offset);
                }
                else
                {
                    svCatalog.ScrollToTop();
                }
            }
        }

        private void HandleOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _p = e.GetPosition(this);
            if (_p.X <= ActualWidth - 20 || _p.Y <= ActualHeight - 20)
            {
                DragMove();
                return;
            }
            _oldSize = new Size(ActualWidth, ActualHeight);
            Mouse.Capture(this);
            _isResize =true;
        }

        private void HandelOnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                var p = e.GetPosition(this);
                WindowResize(p);
            }
        }

        private void HandelOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(this);
            WindowResize(p);
        }
        private void WindowResize(Point p)
        {
            if (_isResize)
            {
                Width = _oldSize.Width + p.X - _p.X;
                Height = _oldSize.Height + p.Y - _p.Y;
                _isResize = false;
                ReleaseMouseCapture();
                TbContent_OnPreviewMouseWheel(null, null);
            }
        }
        private void LsCatalog_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = LsCatalog.SelectedItem as ChaptersInfo;
            if (item != null)
            {
                Chapterpbar.Value = 0;
                _currentChapter = item;
                var index = _chapters.IndexOf(item);
                try
                {
                    if (index < (_chapters.Count - 1))
                    {
                        var item2 = _chapters[index + 1];
                        TbContent.Text = GetChapterContent(item.LineNum, item2.LineNum - 1);
                    }
                    else
                    {
                        TbContent.Text = GetChapterContent(item.LineNum, -1);
                    }
                }
                catch
                {
                    ReloadBook();
                    return;
                }
                if (_autoLoad)
                {
                    UpdateLayout();
                    var c = _bookInfo.ChapterOffset + TbContent.ViewportHeight;
                    Chapterpbar.Value = c / TbContent.ExtentHeight * 100;
                    TbContent.ScrollToVerticalOffset(c);
                }
                else
                {
                    TbContent.ScrollToHome();
                }
                TbChapter.Text = item.Content;
            }
            TotalCpbar.PercentValue = (LsCatalog.SelectedIndex + 1) / (double)_chapters.Count * 100;
            _autoLoad = false; 
            if (LsCatalog.Visibility == Visibility.Visible)
            {
                LsCatalog.Visibility = Visibility.Collapsed;
                CbCatalogVisble.IsChecked = false;
            }
        }
        private void ReloadBook()
        {
            _autoLoad = true;
            _regexChanged = true;
            _txtRegex = _txtRegex == ConfigSevice.DefaultTxtRegex1
                ? ConfigSevice.DefaultRxtRegex2 : ConfigSevice.DefaultTxtRegex1;
            GetChapters();
        }

        private void GetChapters()
        {
            _chapters.Clear();
            if (!File.Exists(_file))
            {
                _autoLoad = false;
                return;
            }
            var name = FileHelper.GetFileNameNoneExt(_file);
            if (!_file.EndsWith(ConfigSevice.BookExt))
            {
                var tmpFile = ConfigSevice.BookDir + name + ConfigSevice.BookExt;
                FileHelper.FileToUTF8(_file, tmpFile);
                _file = tmpFile;
            }
            var rgx = new Regex(_txtRegex);
            using (StreamReader sr = File.OpenText(_file))
            {
                string s;
                int lineNum = 1;
                while ((s = sr.ReadLine()) != null)
                {
                    if (rgx.IsMatch(s))
                    {
                        var count = _chapters.Count; 
                        if (count <= 0 || (lineNum - _chapters[count - 1].LineNum) > 5)
                        {
                            if (count <= 0 && lineNum > 5)
                            {
                                _chapters.Add(new ChaptersInfo
                                {
                                    Content = "序",
                                    LineNum = 0
                                });
                            }
                            _chapters.Add(new ChaptersInfo
                                {
                                    Content = s,
                                    LineNum = lineNum
                                });
                        }
                    }
                    lineNum++;
                }
            }
            if ( _chapters.Count <= 0 && !_regexChanged)
            {
                ReloadBook();
                return;
            }
            Dispatcher.Invoke(() =>
            {
                LsCatalog.ItemsSource = _chapters;
                if (_chapters.Count > 0)
                {
                    if (_autoLoad)
                    {
                        LsCatalog.SelectedIndex = _bookInfo.ChapterIndex < 0 ? 0 : _bookInfo.ChapterIndex;
                    }
                    else
                    {
                        _bookInfo = new BookInfo
                            {
                                BookName = name,
                                FilePath = _file
                            };
                        ConfigSevice.SaveBook(ref _bookInfo);
                        LsCatalog.SelectedIndex = 0;
                    }
                    TbBookName.Text = string.Format("《{0}》", _bookInfo.BookName);
                }
                
            });
        }

        private string GetChapterContent(int lineStart, int lineEnd)
        {
            StringBuilder content = new StringBuilder();
            using (StreamReader sr = File.OpenText(_file))
            {
                string s;
                int lineNum = 1;
                while ((s = sr.ReadLine()) != null)
                {
                    if (lineNum >= lineStart)
                    {
                        if (lineEnd > -1 && lineNum > lineEnd)
                        {
                            break;
                        }
                        if (content.Length > 1024*1024)
                        {
                            if (_regexChanged)
                            {
                                MessageBox.Show("章节内容超过1MB,超出部分未加载");
                                break;
                            }
                            throw new Exception();
                        }
                        content.Append(s + "\r\n");
                    }
                    lineNum++;
                }
            }
            return content.ToString();
        }

        private void SaveCurrentBook()
        {
            if (_bookInfo == null)
            {
                return;
            }
            _bookInfo.ChapterIndex = _chapters.IndexOf(_currentChapter);
            _bookInfo.ChapterOffset = TbContent.VerticalOffset;
            if (_currentChapter != null)
            {
                var rgx = new Regex(_txtRegex);
                _bookInfo.ChapterName = rgx.Match(_currentChapter.Content).Value;
            }
            ConfigSevice.SaveCurrentBook(_bookInfo);
        }

        private void LoadBookShelft()
        {
            var list = ConfigSevice.GetAllBooks();
            if (list != null && list.Count>0 && _bookInfo != null && !string.IsNullOrEmpty(_bookInfo.BookId))
            {
                var book = list.First(o => o.BookId == _bookInfo.BookId);
                if (_currentChapter != null)
                {
                    var rgx = new Regex(_txtRegex);
                    book.ChapterName = rgx.Match(_currentChapter.Content).Value;
                }
                list.Remove(book);
                list.Insert(0,book);
            }
            ListCtrlBookShelf.ItemsSource = list;
        }

        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                case Key.D:
                    ChapterDown();
                    break;
                case Key.A:
                case Key.Left:
                    ChapterUp();
                    break;
                case Key.PageUp:
                case Key.W:
                    PageUp();
                    break;
                case Key.PageDown:
                case Key.S:
                    PageDown();
                    break;
                case Key.Down:
                case Key.E:
                    TbContent.LineDown();
                    TbContent_OnPreviewMouseWheel(null, null);
                    break;
                case Key.Up:
                case Key.Q:
                    TbContent.LineUp();
                    TbContent_OnPreviewMouseWheel(null, null);
                    break;
                case Key.Escape:
                    WindowState = WindowState.Minimized;
                    break;
            }
        }
        private void PageUp()
        {
            if (Chapterpbar.Value <= 0)
            {
                ChapterUp();
                return;
            }
            var o = TbContent.VerticalOffset - TbContent.ViewportHeight;
            Chapterpbar.Value = o / TbContent.ExtentHeight * 100;
            if (Chapterpbar.Value <= 0)
            {
                TbContent.ScrollToHome();
                Chapterpbar.Value = 0;
                return;
            }
            TbContent.ScrollToVerticalOffset(o);
        }

        private void PageDown()
        {
            if (Chapterpbar.Value >= 100)
            {
                ChapterDown();
                return;
            }
            var c = TbContent.VerticalOffset + TbContent.ViewportHeight;
            Chapterpbar.Value = c / TbContent.ExtentHeight * 100;
            if (Chapterpbar.Value >= 100)
            {
                TbContent.ScrollToEnd();
                Chapterpbar.Value = 100;
                return;
            }
            TbContent.ScrollToVerticalOffset(c);
        }

        private void ChapterUp()
        {
            var index = _chapters.IndexOf(_currentChapter);
            if (index > 0)
            {
                LsCatalog.SelectedIndex = index - 1;
            }
            else
            {
                if(_bookInfo != null)
                    MessageBox.Show("前面没有了");
            }
        }

        private void ChapterDown()
        {
            var index = _chapters.IndexOf(_currentChapter);
            if (index < (_chapters.Count - 1))
            {
                LsCatalog.SelectedIndex = index + 1;
            }
            else
            {
                if (_bookInfo != null)
                    MessageBox.Show("后面没有了");
            }
        }

        private void TbContent_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var tb = sender as TextBox;
            var p = e.GetPosition(tb);
            var h = tb.ActualHeight/3;
            if (p.Y <= h)
            {
                PageUp();
            }
            else if (p.Y >= 2*h)
            {
                PageDown(); 
            }
            else
            {

                if (CbTitle.IsChecked == true)
                {
                    if (CbCatalogVisble.IsChecked != true)
                    {
                        CbTitle.IsChecked = false;
                        SpTitle.Visibility = Visibility.Visible;
                        SpSetting.Visibility = Visibility.Visible;
                    }
                    return;
                }
            }
            if (CbTitle.IsChecked != true)
            {
                CbTitle.IsChecked = true;
                SpTitle.Visibility = Visibility.Collapsed;
                SpSetting.Visibility = Visibility.Collapsed;
            }
            if (CbCatalogVisble.IsChecked == true)
            {
                CbCatalogVisble.IsChecked = false;
                LsCatalog.Visibility = Visibility.Collapsed;
            }
        }

        private void TbContent_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (TbContent.VerticalOffset <= 0)
            {
                Chapterpbar.Value = 0;
                return;
            }
            var c = TbContent.VerticalOffset + TbContent.ViewportHeight;
            Chapterpbar.Value = c / TbContent.ExtentHeight * 100;
        }

        private void ColorsOnClick(object sender, RoutedEventArgs e)
        {
            var bg = (sender as RadioButton).Background;
            BdBackground.Background = bg;
        }

        private void LoadConfig()
        {
            var isMaximized = ConfigSevice.GetConfig(ConfigKey.WindowMaximized, "0") == "1";
            if (!isMaximized)
            {
                var size = ConfigSevice.GetConfig(ConfigKey.WindowSize);
                if (!string.IsNullOrEmpty(size))
                {
                    var sizeArr = size.Split(',');
                    if (sizeArr.Length == 2)
                    {
                        Width = double.Parse(sizeArr[0]);
                        Height = double.Parse(sizeArr[1]);
                    }
                }
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
            var opacity = ConfigSevice.GetConfig(ConfigKey.WindowOpacity, "0.8");
            sOpacity.Value = double.Parse(opacity);
            var background = ConfigSevice.GetConfig(ConfigKey.Background, "#AAA");
            BdBackground.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(background));
            foreach (RadioButton child in SpSetting.Children.Cast<object>().Where(child => child is RadioButton))
            {
                var str = (child.Background as SolidColorBrush).Color.ToString();
                if (str == background)
                {
                    child.IsChecked = true;
                }
            }
            var fontsize = ConfigSevice.GetConfig(ConfigKey.FontSize, "14");
            sFontSize.Value = double.Parse(fontsize);
            var brightness = ConfigSevice.GetConfig(ConfigKey.FontBrightness, "0");
            sBrightness.Value = double.Parse(brightness);
        }

        private void SaveConfig()
        {
            var isMaximized = WindowState == WindowState.Maximized;
            ConfigSevice.SetConfig(ConfigKey.WindowMaximized, isMaximized ? "1":"0");
            if (!isMaximized)
            {
                ConfigSevice.SetConfig(ConfigKey.WindowSize, string.Format("{0},{1}", ActualWidth.ToString("F"), ActualHeight.ToString("F")));
            }
            ConfigSevice.SetConfig(ConfigKey.WindowOpacity, Opacity.ToString("F"));
            ConfigSevice.SetConfig(ConfigKey.Background, (BdBackground.Background as SolidColorBrush).Color.ToString());
            ConfigSevice.SetConfig(ConfigKey.FontSize, sFontSize.Value.ToString("F"));
            ConfigSevice.SetConfig(ConfigKey.FontBrightness,sBrightness.Value.ToString("F"));
        }
    }
}
