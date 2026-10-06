using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartFillMonitor.UserControls
{
    /// <summary>
    /// StatusIndicator.xaml 的交互逻辑
    /// </summary>
    public partial class StatusIndicator : UserControl
    {
        public StatusIndicator()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsOnlineProperty =
        DependencyProperty.Register("IsOnline", typeof(bool), typeof(StatusIndicator), new PropertyMetadata(false));
        public bool IsOnline
        {
            get { return (bool)GetValue(IsOnlineProperty); }
            set { SetValue(IsOnlineProperty, value); }
        }

        public static readonly DependencyProperty IsWaringProperty =
        DependencyProperty.Register("IsWaring", typeof(bool), typeof(StatusIndicator), new PropertyMetadata(false));
        public bool IsWaring
        {
            get { return (bool)GetValue(IsWaringProperty); }
            set { SetValue(IsWaringProperty, value); }
        }


        public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register("StatusText", typeof(string), typeof(StatusIndicator), new PropertyMetadata(false));
        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

    }
}
