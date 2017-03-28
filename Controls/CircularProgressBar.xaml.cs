using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EBookReader.Controls
{
    /// <summary>
    /// CircularProgressBar.xaml 的交互逻辑
    /// </summary>
    public partial class CircularProgressBar : INotifyPropertyChanged
    {
        public static readonly DependencyProperty PercentValueProperty =
            DependencyProperty.Register("PercentValue", typeof (double), typeof (CircularProgressBar), new PropertyMetadata(default(double),PercentValueChanged));
        
        public double PercentValue
        {
            get { return (double) GetValue(PercentValueProperty); }
            set { SetValue(PercentValueProperty, value); }
        }

        private double _angleValue;

        public double AngleValue
        {
            get { return _angleValue; }
            set
            {
                _angleValue = value;
                OnPropertyChanged("AngleValue");
            }
        }

        private string _percentText="0%";

        public string PercentText
        {
            get { return _percentText; }
            set
            {
                _percentText = value;
                OnPropertyChanged("PercentText");
            }
        }


        public CircularProgressBar()
        {
            InitializeComponent();
        }


        private static void PercentValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as CircularProgressBar;
            ctrl.AngleValue = 360*(double)e.NewValue/100;
            ctrl.PercentText = string.Format("{0}%", Math.Ceiling((double)e.NewValue));
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
