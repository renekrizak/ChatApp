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
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ChatApp.Pages
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        private void goBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }

        private void registerNewUser(object sender, RoutedEventArgs e)
        {
            AsynchronousClient.StartLogRegClient(getRegFlag(), getRegCredantials());
            string id = AsynchronousClient.returnUserID();
            Client user = new Client(id);
            user.Show();
        }

        private string getRegFlag()
        {
            return "REG";
        }

        private string getRegCredantials()
        {
            string result = usernameRegisterBox.Text + ":" + passwordRegisterBox.Text + ":" + emailRegisterBox.Text + "<EOF>";
            return result;
        }

    }
}
