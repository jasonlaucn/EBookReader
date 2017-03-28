using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// SystemTimer.xaml 的交互逻辑
    /// </summary>
    public partial class SystemTimer : UserControl
    {
        private Timer _timer;

        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register("FormatString", typeof (string), typeof (SystemTimer), new PropertyMetadata("t"));
        /// <summary>
        /// 时间格式化字符串
        //y 	DateTime.Now.ToString() 	2016/5/9 13:09:55 	            短日期 长时间
        //d 	DateTime.Now.ToString("d") 	2016/5/9 	                    短日期
        //D 	DateTime.Now.ToString("D") 	2016年5月9日 	                长日期
        //f 	DateTime.Now.ToString("f") 	2016年5月9日 13:09 	            长日期 短时间
        //F 	DateTime.Now.ToString("F") 	2016年5月9日 13:09:55 	        长日期 长时间
        //g 	DateTime.Now.ToString("g") 	2016/5/9 13:09 	                短日期 短时间
        //G 	DateTime.Now.ToString("G")  	2016/5/9 13:09:55 	        短日期 长时间
        //t 	DateTime.Now.ToString("t") 	13:09 	                        短时间
        //T 	DateTime.Now.ToString("T") 	13:09:55 	                    长时间
        //u 	DateTime.Now.ToString("u") 	2016-05-09 13:09:55Z 	 
        //U 	DateTime.Now.ToString("U") 	2016年5月9日 5:09:55 	        本初子午线的长日期和长时间
        //m 	DateTime.Now.ToString("m") 	5月9日 	 
        //M 	DateTime.Now.ToString("M") 	5月9日 	 
        //r 	DateTime.Now.ToString("r") 	Mon, 09 May 2016 13:09:55 GMT 	 
        //R 	DateTime.Now.ToString("R") 	Mon, 09 May 2016 13:09:55 GMT 	 
        //y 	DateTime.Now.ToString("y") 	2016年5月 	 
        //Y 	DateTime.Now.ToString("Y") 	2016年5月 	 
        //o 	DateTime.Now.ToString("o") 	2016-05-09T13:09:55.2350000 	 
        //O 	DateTime.Now.ToString("O") 	2016-05-09T13:09:55.2350000 	 
        //s 	DateTime.Now.ToString("s") 	2016-05-09T13:09:55 
        /// </summary>
        public string FormatString
        {
            get { return (string) GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        public SystemTimer()
        {
            InitializeComponent();
        }

        private void SystemTimer_OnLoaded(object sender, RoutedEventArgs e)
        {
            TbTime.Text = DateTime.Now.ToString(FormatString);
            _timer = new Timer
                {
                    AutoReset = true,
                    Enabled = true,
                    Interval = 10000
                };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Dispatcher.Invoke(() => TbTime.Text = DateTime.Now.ToString(FormatString));
        }

        private void SystemTimer_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _timer = null;
        }
    }
}
