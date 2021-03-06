﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EBookReader
{
    /// <summary>
    /// FloatingWin.xaml 的交互逻辑
    /// </summary>
    public partial class FloatingWin : INotifyPropertyChanged
    {
        public FloatingWin()
        {
            InitializeComponent();
        }

        private void BorderOnMouseDown(object sender, MouseEventArgs e)
        {
            if (Owner != null)
            {
                Owner.Activate();
            }
            DragMove();
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
    }
}
