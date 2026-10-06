using HandyControl.Controls;
using Serilog.Events;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
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
using System.Windows.Shapes;

namespace SmartFillMonitor.Views
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : System.Windows.Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                await LoadUserAsync();
                if (PassWordBox != null)
                {
                    PassWordBox.Focus();
                }
            };

            KeyDown += LoginWidow_KeyDown;
        }

        private void LoginWidow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                Button_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            ;
        }

        private async Task LoadUserAsync()
        {
            try
            {
                List<User> users = await UserService.GetAllUserAsync();
                UserNameCombo.ItemsSource = users;
                if (users != null && users.Count > 0)
                {
                    UserNameCombo.SelectedIndex = 0;

                }

            }
            catch
            {

            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void Login_click(object sender, RoutedEventArgs e)
        {
            var username=(UserNameCombo.SelectedValue as string) ?? string.Empty;
            var password = PassWordBox.Password ?? string.Empty;
            if (string.IsNullOrEmpty(username))
            {
                System.Windows.MessageBox.Show("请输入用户名", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                UserNameCombo.Focus();
                return;
            }
            IsEnabled = false;

            try
            {
                var ok = await UserService.AutenticateAsync(username, password);
                if (ok)
                {
                    DialogResult = true;
                    Close(); 
                }
                else
                {
                    System.Windows.MessageBox.Show("用户名或者密码错误", "登录失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PassWordBox.Clear();
                    PassWordBox.Focus();
                }
            }
            catch 
            {
                System.Windows.MessageBox.Show("用户名或者密码错误", "登录失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                PassWordBox.Clear();
                PassWordBox.Focus();
            }
            finally { IsEnabled = true; }

        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
