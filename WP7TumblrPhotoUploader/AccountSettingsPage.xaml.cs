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

            // If OAuth tokens are present, display the OAuth screen
            if (this.userCredentials.Type == TumblrCredentials.CredentialsType.OAuth)
            {
                // HACK: This is dumb but I don't want to use automation to perform a button click. Consider refactoring.
                this.OAuthOption_Click(null, null);
                this.oAuthClearButton.Visibility = System.Windows.Visibility.Visible;
            }
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
            oAuthCred.ConsumerKey = Common.OAUTH_CONSUMER_KEY;
            oAuthCred.ConsumerSecret = Common.OAUTH_CONSUMER_SECRET;

            client.Credentials = oAuthCred;

            // Perform the async request for the request token
            client.BeginRequest(new RestCallback(RequestTokenCallback));
        }

        /**
         * Callback for receiving the RequestToken from Tumblr
         **/
        private void RequestTokenCallback(RestRequest request, RestResponse response, object target)
        {
            // Parse out the request tokens
            string queryString = response.Content;
            this.oAuthRequestToken = Common.GetValueFromQueryString(queryString, "oauth_token");
            this.oAuthRequestSecret = Common.GetValueFromQueryString(queryString, "oauth_token_secret");
        
            // Start the web browser with the request token
            Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/WebBrowserAuthorizationPage.xaml?oauth_token=" + this.oAuthRequestToken, UriKind.Relative)));
        }

        /**
         * Navigation callback. The web browser will hit this method when it is done authorizing
         **/
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (storage.Contains("from_browser"))
            {
                storage.Remove("from_browser");

                string verifier = storage["oauth_verifier"].ToString();
                storage.Remove("oauth_verifier");

                // Request the Access Token
                RestClient client = new RestClient();
                client.Authority = AccountSettingsPage.OAUTH_AUTHORITY;
                client.Path = AccountSettingsPage.OAUTH_ACCESS_TOKEN_PATH;
                client.Method = Hammock.Web.WebMethod.Post;
                client.HasElevatedPermissions = true;

                // Set up the first set of credentials
                OAuthCredentials oAuthCred = new OAuthCredentials();
                oAuthCred.Type = OAuthType.AccessToken;
                oAuthCred.ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader;
                oAuthCred.SignatureMethod = OAuthSignatureMethod.HmacSha1;
                oAuthCred.ConsumerKey = Common.OAUTH_CONSUMER_KEY;
                oAuthCred.ConsumerSecret = Common.OAUTH_CONSUMER_SECRET;
                oAuthCred.Verifier = verifier;
                oAuthCred.Token = this.oAuthRequestToken;
                oAuthCred.TokenSecret = this.oAuthRequestSecret;

                client.Credentials = oAuthCred;

                // Perform the async request for the request token
                client.BeginRequest(new RestCallback(AccessTokenCallback));

            }

            base.OnNavigatedTo(e);
        }

        /**
         * Called when we finally need the access token!
         **/
        public void AccessTokenCallback(RestRequest request, RestResponse response, object target)
        {
            string queryString = response.Content;

            // Get the FINAL tokens
            userCredentials.OAuthToken = Common.GetValueFromQueryString(queryString, "oauth_token");
            userCredentials.OAuthTokenSecret = Common.GetValueFromQueryString(queryString, "oauth_token_secret");

            Dispatcher.BeginInvoke(() => this.oAuthClearButton.Visibility = System.Windows.Visibility.Visible);
        }

        /**
         * Clear any existing OAuth tokens
         **/
        private void ClearOAuthTokens_Click(object sender, RoutedEventArgs e)
        {
            this.userCredentials.OAuthToken = null;
            this.userCredentials.OAuthTokenSecret = null;

            this.oAuthClearButton.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}