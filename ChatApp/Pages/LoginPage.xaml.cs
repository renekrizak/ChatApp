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
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void goBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }

        private void loginUser(object sender, RoutedEventArgs e)
        {
            AsynchronousClient.StartUserClient(getLoginFlag(), getLogCredentials());
            string id = AsynchronousClient.returnUserID();
            Client user = new Client(id);
            user.Show();
        }

        private string getLogCredentials()
        {
            string creds = usernameLoginBox.Text + ":" + passwordLoginBox.Text + "<EOF>";
            return creds;
        }
        
        private string getLoginFlag()
        {
            return "LOG";
        }
    }
}
