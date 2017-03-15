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
        private Point _pt;
        private Size _oldSize;
        private string _file;
        private string _txtRegex;

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
                    Filter = "(*.txt;)|*.txt;"
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

        private void BdBookShelf_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
                    _autoLoad = true;
                    _file = book.FilePath;
                    _bookInfo = book;
                    new Thread(GetChapters).Start();
                }
            }
            BdBookShelf.Visibility = Visibility.Collapsed;
        }

        private void CbTitleVisbleOnClick(object sender, RoutedEventArgs e)
        {
            SpTitle.Visibility = (sender as CheckBox).IsChecked == true ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CbCatalogVisbleOnClick(object sender, RoutedEventArgs e)
        {
            LsCatalog.Visibility = (sender as CheckBox).IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            if ((sender as CheckBox).IsChecked == true && _currentChapter != null)
            {
                LsCatalog.ScrollIntoView(_currentChapter);
            }
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!ConfigSevice.GetCurrentBookInfo(out _bookInfo))
            {
                return;
            }
            _file = _bookInfo.FilePath;
            new Thread(GetChapters).Start();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            SaveCurrentBook();
        }

        private void BtnDel(object sender, RoutedEventArgs e)
        {
            LsCatalog.ItemsSource = null;
            TbContent.Text = "";
            _file = "";
            _currentChapter = null;
            if (_bookInfo != null && !string.IsNullOrEmpty(_bookInfo.BookId))
            {
                ConfigSevice.DelBook(_bookInfo.BookId);
            }
        }

        private void Btn1OnClick(object sender, RoutedEventArgs e)
        {
            PageUp();
        }
        private void Btn2OnClick(object sender, RoutedEventArgs e)
        {
            ChapterUp();
        }
        private void Btn3OnClick(object sender, RoutedEventArgs e)
        {
            ChapterDown();
        }
        private void Btn4OnClick(object sender, RoutedEventArgs e)
        {
            PageDown();
        }
        private void Btn5OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HandleOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Win32Support.POINT point;
            Win32Support.GetCursorPos(out point);
            _pt = new Point(point.X, point.Y);
            _oldSize = new Size(ActualWidth, ActualHeight);
            Mouse.Capture(sender as Path);
        }

        private void HandelOnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Win32Support.POINT point;
                Win32Support.GetCursorPos(out point);
                var p = new Point(point.X, point.Y);
                Width = _oldSize.Width + p.X - _pt.X;
                Height = _oldSize.Height + p.Y - _pt.Y;
            }
            else
            {
                (sender as Path).ReleaseMouseCapture();
            }
        }

        private void HandelOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (sender as Path).ReleaseMouseCapture();
        }
        private void LsCatalog_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = LsCatalog.SelectedItem as ChaptersInfo;
            if (item != null)
            {
                _currentChapter = item;
                var index = _chapters.IndexOf(item);
                if (index < (_chapters.Count - 1))
                {
                    var item2 = _chapters[index + 1];
                    TbContent.Text = GetChapterContent(item.LineNum, item2.LineNum - 1);
                }
                else
                {
                    TbContent.Text = GetChapterContent(item.LineNum, -1);
                }
                if (_autoLoad)
                {
                    TbContent.ScrollToVerticalOffset(_bookInfo.ChapterOffset);
                }
                else
                {
                    TbContent.ScrollToHome();
                }
                TbFile.Text = item.Content;
            }

            _autoLoad = false; 
            if (LsCatalog.Visibility == Visibility.Visible)
            {
                LsCatalog.Visibility = Visibility.Collapsed;
                CbCatalogVisble.IsChecked = false;
            }
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
            if (!_file.EndsWith(ConfigSevice.BOOKEXT))
            {
                var tmpFile = ConfigSevice.BookDir + name + ConfigSevice.BOOKEXT;
                FileHelper.FileToUTF8(_file, tmpFile);
                _file = tmpFile;
            }
            _txtRegex = ConfigSevice.GetConfig(ConfigSevice.REGEXITMENAME);
            if (string.IsNullOrEmpty(_txtRegex))
            {
                _txtRegex = ConfigSevice.DEFAULTTXTREGEX;
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
            Dispatcher.Invoke(() =>
            {
                LsCatalog.ItemsSource = _chapters;
                if (_chapters.Count > 0)
                {
                    if (_autoLoad)
                    {
                        _autoLoad = false;
                        LsCatalog.SelectedIndex = _bookInfo.ChapterIndex;
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
                            MessageBox.Show("章节内容超过1MB,超出部分未加载");
                            break;
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

        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
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
                    break;
                case Key.Up:
                case Key.Q:
                    TbContent.LineUp();
                    break;
                case Key.Escape:
                    WindowState = WindowState.Minimized;
                    break;
            }
        }
        private void PageUp()
        {
            var o = TbContent.VerticalOffset - TbContent.ActualHeight;
            if (o <= 0)
            {
                TbContent.ScrollToHome();
                return;
            }
            TbContent.ScrollToVerticalOffset(o);
        }

        private void PageDown()
        {
            var c = TbContent.VerticalOffset + TbContent.ActualHeight;
            if (c > TbContent.ExtentHeight)
            {
                TbContent.ScrollToEnd();
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
                MessageBox.Show("后面没有了");
            }
        }

    }
}
