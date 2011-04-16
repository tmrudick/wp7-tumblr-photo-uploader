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
using Hammock;
using Hammock.Authentication.OAuth;

namespace WP7TumblrPhotoUploader
{
    public partial class AccountSettingsPage : PhoneApplicationPage
    {
        // userCredentials come out of storage on page load or are created empty if they do not exist
        private IsolatedStorageSettings storage = IsolatedStorageSettings.ApplicationSettings;
        private TumblrCredentials userCredentials;

        private const string OAUTH_CONSUMER_KEY = "ddRwNYFhclqTMDM8VGCUNwlJEEPWQjLWWpYMhrockMaQBKlUiG";
        private const string OAUTH_CONSUMER_SECRET = "b5V0p8jP8qaiviUttv2aym41S2YiiOkYbzShsqVrUAtkIHTjyH";

        private const string OAUTH_AUTHORITY = "http://tumblr.com/oauth";
        private const string OAUTH_REQUEST_TOKEN_PATH = "/request_token";
        private const string OAUTH_AUTH_PATH = "/authorize";
        private const string OAUTH_ACCESS_TOKEN_PATH = "/access_token";

        private string oAuthRequestToken;
        private string oAuthRequestSecret;

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

        /**
         * Hide the OAuth stuff and just show an old-timey username and password box
         */
        private void BasicOption_Click(object sender, MouseButtonEventArgs e)
        {
            this.oauthCanvas.Visibility = System.Windows.Visibility.Collapsed;
            this.oauthTextBlock.Foreground = this.Resources["UnselectedOptionBrush"] as Brush;

            this.basicCanvas.Visibility = System.Windows.Visibility.Visible;
            this.basicTextBlock.Foreground = this.Resources["SelectedOptionBrush"] as Brush;
            
        }

        /**
         * Hide the Basic Auth stuff and just show new-timey OAuth settings
         */
        private void OAuthOption_Click(object sender, MouseButtonEventArgs e)
        {
            this.basicCanvas.Visibility = System.Windows.Visibility.Collapsed;
            this.basicTextBlock.Foreground = this.Resources["UnselectedOptionBrush"] as Brush;

            this.oauthCanvas.Visibility = System.Windows.Visibility.Visible;
            this.oauthTextBlock.Foreground = this.Resources["SelectedOptionBrush"] as Brush;
        }

        /**
         * Being the performance of the classic OAuth fancy authentication dance
         */
        private void GetOAuthTokens_Click(object sender, RoutedEventArgs e)
        {
            // Create the first client
            RestClient client = new RestClient();
            client.Authority = AccountSettingsPage.OAUTH_AUTHORITY;
            client.Path = AccountSettingsPage.OAUTH_REQUEST_TOKEN_PATH;
            client.Method = Hammock.Web.WebMethod.Post;
            client.HasElevatedPermissions = true;

            // Set up the first set of credentials
            OAuthCredentials oAuthCred = new OAuthCredentials();
            oAuthCred.Type = OAuthType.RequestToken;
            oAuthCred.ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader;
            oAuthCred.SignatureMethod = OAuthSignatureMethod.HmacSha1;
            oAuthCred.ConsumerKey = AccountSettingsPage.OAUTH_CONSUMER_KEY;
            oAuthCred.ConsumerSecret = AccountSettingsPage.OAUTH_CONSUMER_SECRET;

            client.Credentials = oAuthCred;

            // Perform the async request for the request token
            client.BeginRequest(new RestCallback(RequestTokenCallback));
        }

        /**
         * Callback for receiving the RequestToken from Tumblr
         **/
        private void RequestTokenCallback(RestRequest request, RestResponse response, object target)
        {
            string queryString = response.Content;
            this.oAuthRequestToken = GetValueFromQueryString(queryString, "oauth_token");
            this.oAuthRequestSecret = GetValueFromQueryString(queryString, "oauth_token_secret");
        }

        /**
         * Given a querystring and a field name, return the value of the field
         **/
        private string GetValueFromQueryString(string queryString, string field)
        {
            string[] pairs = queryString.Split('&');

            foreach (string pair in pairs)
            {
                if (pair.StartsWith(field))
                {
                    int equalsIndex = pair.IndexOf('=');
                    return pair.Substring(equalsIndex + 1);
                }
            }

            return null;
        }
    }
}