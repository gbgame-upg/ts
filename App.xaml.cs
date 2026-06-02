using System.Windows;
using QLHangTonKho.Views;

namespace QLHangTonKho
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Khởi động từ màn hình đăng nhập
            var login = new LoginWindow();
            login.Show();
        }
    }
}