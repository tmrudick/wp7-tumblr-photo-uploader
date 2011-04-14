using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;

namespace WP7TumblrPhotoUploader
{
    public partial class AccountSettingsPage : PhoneApplicationPage
    {
        private IsolatedStorageSettings storage = IsolatedStorageSettings.ApplicationSettings;
        private TumblrCredentials userCredentials;

        public AccountSettingsPage()
        {
            InitializeComponent();

            // Load user stuff from isolated storage
            if (this.storage.Contains("userCredentials"))
            {
                this.userCredentials = storage["userCredentials"] as TumblrCredentials;
            }
            else
            {
                this.userCredentials = new TumblrCredentials();
            }

            // Update username and password stuff
            this.usernameTextBox.Text = this.userCredentials.Username;
            this.passwordTextBox.Password = this.userCredentials.Password;
        }

        /**
         * Just go back to the previous page
         **/
        private void Cancel_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        /**
         * Save and THEN go back to the previous page
         **/
        private void Save_Click(object sender, EventArgs e)
        {
            // TODO: Save here
            this.userCredentials.Username = this.usernameTextBox.Text;
            this.userCredentials.Password = this.passwordTextBox.Password;

            storage.Remove("userCredentials");
            storage.Add("userCredentials", this.userCredentials);

            // Then go back
            NavigationService.GoBack();
        }

        private void Basic_Click(object sender, MouseButtonEventArgs e)
        {
            this.oauthCanvas.Visibility = System.Windows.Visibility.Collapsed;
            this.oauthTextBlock.Foreground = this.Resources["UnselectedOptionBrush"] as Brush;

            this.basicCanvas.Visibility = System.Windows.Visibility.Visible;
            this.basicTextBlock.Foreground = this.Resources["SelectedOptionBrush"] as Brush;
            
        }

        private void OAuth_Click(object sender, MouseButtonEventArgs e)
        {
            this.basicCanvas.Visibility = System.Windows.Visibility.Collapsed;
            this.basicTextBlock.Foreground = this.Resources["UnselectedOptionBrush"] as Brush;

            this.oauthCanvas.Visibility = System.Windows.Visibility.Visible;
            this.oauthTextBlock.Foreground = this.Resources["SelectedOptionBrush"] as Brush;
        }
    }
}