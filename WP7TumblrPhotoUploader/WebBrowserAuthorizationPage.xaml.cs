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
    public partial class WebBrowserAuthorizationPage : PhoneApplicationPage
    {
        private const string TUMBLR_AUTH_URL = "http://tumblr.com/oauth/authorize";
        private string oauthToken;

        public WebBrowserAuthorizationPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            browser.Navigating += new EventHandler<NavigatingEventArgs>(browser_Navigating);


            this.oauthToken = this.NavigationContext.QueryString["oauth_token"];

            browser.Navigate(new Uri(TUMBLR_AUTH_URL + "?oauth_token=" + this.oauthToken));
        }

        void browser_Navigating(object sender, NavigatingEventArgs e)
        {
            // Tumblr is redirecting us to the callback site
            if (e.Uri.AbsoluteUri.Contains("tomrudick.com"))
            {
                // Stop the navigation!
                e.Cancel = true;

                // Get the verifier from the redirect URL
                string[] uriParts = e.Uri.AbsoluteUri.Split('?');

                if (uriParts.Length == 2)
                {
                    // Verify that the auth token we have matches the auth token back from the browser
                    if (this.oauthToken.Equals(Common.GetValueFromQueryString(uriParts[1], "oauth_token")))
                    {
                        string verifier = Common.GetValueFromQueryString(uriParts[1], "oauth_verifier");

                        // Store the verifier so that we can get it on the other page
                        // TODO: I don't like this... I want to just pass it back through a URI or something
                        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                        settings.Add("oauth_verifier", verifier);

                        // Set this field so we know where we are coming from...
                        settings.Add("from_browser", true);

                        // Send the user back to the previous page
                        Dispatcher.BeginInvoke(() => NavigationService.GoBack());
                    }
                }   
            }
        }
    }
}