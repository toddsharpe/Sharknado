using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SharkLibrary;
using SharkLibrary.Api;
using SharkLibrary.Models;
using SharkPhone.Models;

namespace SharkPhone.Controls
{
    
    public partial class LoginControl : BusyUserControl
    {
        public event EventHandler OnLogin;
        public event EventHandler OnLogout;

        public LoginControl()
        {
            InitializeComponent();
        }

        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            await LoginAsync(UsernameTextBox.Text, PasswordBox.Password);
        }
        private async void SignOutButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnBusyStarting("Logging Out...");

            try
            {
                await App.LogoutAsync();

                UsernameTextBox.Text = String.Empty;
                PasswordBox.Password = String.Empty;
                ShowLoginPanel();

                if (OnLogout != null)
                    OnLogout(this, new EventArgs());
            }
            catch (Exception)
            {
                ShowError("Unable to connect to Grooveshark");
            }

            OnBusyFinished();
        }

        public async Task LoginAsync(string username, string password)
        {
            OnBusyStarting("Logging In...");

            try
            {
                bool success = await App.LoginAsync(username, password);
                if (!success)
                {
                    ShowError("Invalid login credentials" + Environment.NewLine + "Check your username and password");
                }
                else
                {
                    if (OnLogin != null)
                        OnLogin(this, new EventArgs<bool>(true));

                    ShowLogoutPanel();
                }
            }
            catch (Exception)
            {
                ShowError("Unable to connect to Grooveshark");
            }

            OnBusyFinished();
        }

        private void ShowError(string error)
        {
            ErrorTextBlock.Visibility = Visibility.Visible;
            ErrorTextBlock.Text = error;
        }

        private void ShowLoginPanel()
        {
            SignInPanel.Visibility = Visibility.Visible;
            SignOutPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowLogoutPanel()
        {
            SignInPanel.Visibility = Visibility.Collapsed;
            SignOutPanel.Visibility = Visibility.Visible;

            NameTextBlock.Text = App.Api.CurrentUser.FName;
        }
    }
}
