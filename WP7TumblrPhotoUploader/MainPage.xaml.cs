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
using Microsoft.Phone.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using Hammock;
using Hammock.Authentication.OAuth;
using Microsoft.Phone.Shell;

namespace WP7TumblrPhotoUploader
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Holds if the caption text box holds the default "Enter Caption" test or not
        private bool hasDefaultText = true;

        // Determines if a post operation is currently ongoing
        private bool isPosting = false;

        // Byte array of the currently selected iamge or null if nothing has been selected
        private byte[] photo;

        private PhotoChooserTask photoChooser;

        private const string TUMBLR_AUTHORITY = "http://www.tumblr.com";
        private const string TUMBLR_POST_PATH = "/api/write";

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Initialize the PhotoChooser, allow the camera, and create a callback
            photoChooser = new PhotoChooserTask();
            photoChooser.ShowCamera = true;
            photoChooser.Completed += new EventHandler<PhotoResult>(photoChooser_Completed);
        }

        /**
         * Remove the default text when the user enters the texbox
         **/
        private void captionTextbox_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (this.hasDefaultText)
            {
                this.hasDefaultText = false;
                this.captionTextbox.Text = string.Empty;
                this.captionTextbox.TextAlignment = TextAlignment.Left;
                this.captionTextbox.Foreground = this.Resources["DefaultTextBrush"] as Brush;
            }
        }

        /**
         * Adds the default text if a user leaves the caption textbox empty.
         * 
         * Unsure why I have to use LostFocus instead of ManipulatedCompleted...
         **/
        private void captionTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.captionTextbox.Text.Equals(string.Empty))
            {
                this.hasDefaultText = true;
                this.captionTextbox.Text = "Enter a Caption...";
                this.captionTextbox.TextAlignment = TextAlignment.Center;
                this.captionTextbox.Foreground = this.Resources["InactiveTextBrush"] as Brush;
            }
        }

        /**
         * Display the photo choosers to the user when a user clicks on the image preview
         **/
        private void photoPreview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.isPosting) // Don't allow a new photo to be selected if we are posting
            {
                this.photoChooser.Show();
            }
        }

        /**
         * Get the selected image from the photochooser, populate the photo byte array, and display the photo
         **/
        private void photoChooser_Completed(object sender, PhotoResult e)
        {
            // if they cancelled the chooser ChosenPhoto will be null
            if (e.ChosenPhoto != null)
            {
                // Read the photo into the photo byte array
                MemoryStream memStream = new MemoryStream();
                byte[] buffer = new byte[1024];

                int readByte = 0;

                while ((readByte = e.ChosenPhoto.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memStream.Write(buffer, 0, buffer.Length);
                }

                photo = memStream.ToArray();

                // Update the photo preview image to show the selected image
                BitmapImage previewBitmap = new BitmapImage();
                previewBitmap.SetSource(memStream);

                this.photoPreview.Source = previewBitmap;

                // Update the preview image margin and stretch
                this.photoPreview.Stretch = Stretch.Fill;
                this.photoPreview.SetValue(Canvas.MarginProperty, new Thickness(0, 0, 0, 0));
            }
        }

        /**
         * Display the account settings page
         **/
        private void accountSettings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AccountSettingsPage.xaml", UriKind.Relative));
        }

        /**
         * Actually post the image... finally.
         **/
        private void Post_Click(object sender, EventArgs e)
        {
            // Check if we have credentials
            IsolatedStorageSettings storage = IsolatedStorageSettings.ApplicationSettings;

            // If we have no credentials, force the user to enter their info in account settings.
            if (!storage.Contains("userCredentials"))
            {
                NavigationService.Navigate(new Uri("/AccountSettingsPage.xaml", UriKind.Relative));
            }
            else
            {
                // Get the credentials
                TumblrCredentials userCredentials = storage["userCredentials"] as TumblrCredentials;

                // We have credentials, do we have a photo?
                if (photo != null)
                {
                    // Now here comes the POST!

                    // Create a RestClient
                    RestClient client = new RestClient();
                    client.Authority = MainPage.TUMBLR_AUTHORITY;
                    client.HasElevatedPermissions = true;

                    client.Path = MainPage.TUMBLR_POST_PATH;
                    client.Method = Hammock.Web.WebMethod.Post;

                    // Set the correct credentials on the client or request depending on auth method
                    if (userCredentials.Type == TumblrCredentials.CredentialsType.OAuth)
                    {
                        OAuthCredentials oAuthCred = new OAuthCredentials();
                        oAuthCred.ConsumerKey = Common.OAUTH_CONSUMER_KEY;
                        oAuthCred.ConsumerSecret = Common.OAUTH_CONSUMER_SECRET;
                        oAuthCred.Token = userCredentials.OAuthToken;
                        oAuthCred.TokenSecret = userCredentials.OAuthTokenSecret;
                        oAuthCred.ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader;
                        oAuthCred.SignatureMethod = OAuthSignatureMethod.HmacSha1;
                        oAuthCred.Type = OAuthType.ProtectedResource;

                        client.Credentials = oAuthCred;
                    }
                    else
                    {
                        client.AddField("email", userCredentials.Username);
                        client.AddField("password", userCredentials.Password);
                    }

                    // Add metadata fields
                    client.AddField("type", "photo");
                    //client.AddField("state", "draft"); // Debug line for testing
                    client.AddField("send-to-twitter", "auto"); // Debug line because I'm paranoid

                    // Add caption but check for an empty field
                    if (!this.hasDefaultText)
                    {
                        client.AddField("caption", this.captionTextbox.Text);
                    }

                    client.AddFile("data", "upload.jpg", new MemoryStream(photo));

                    // Send the request of to la-la-land
                    client.BeginRequest(new RestCallback(PostCompleted));

                    this.isPosting = true;
                    // HACK: Well this is hacky...
                    Dispatcher.BeginInvoke(() => ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false);

                    Dispatcher.BeginInvoke(() =>
                    {
                        this.postProgress.Visibility = System.Windows.Visibility.Visible;
                        this.captionTextbox.IsEnabled = false;
                        this.postProgress.Focus();
                    });
                }
                else
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Please Select a Photo."));
                }
            }
        }

        /**
         * ASync callback for posting a new photo
         **/
        public void PostCompleted(RestRequest request, RestResponse response, object target)
        {
            Dispatcher.BeginInvoke(() => this.postProgress.Visibility = System.Windows.Visibility.Collapsed);
            this.isPosting = false;
            // HACK: This is kind of hacky...
            Dispatcher.BeginInvoke(() => ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                Dispatcher.BeginInvoke(() => {
                    MessageBox.Show("Photo Posted Successfully!");
                    this.captionTextbox.IsEnabled = true;
                    
                    // Reset photo and caption
                    this.hasDefaultText = true;
                    this.captionTextbox.Text = "Enter a Caption...";
                    this.captionTextbox.TextAlignment = TextAlignment.Center;
                    this.captionTextbox.Foreground = this.Resources["InactiveTextBrush"] as Brush;

                    this.photo = null;
                    this.photoPreview.Source = new BitmapImage(new Uri("/Images/photo_icon.png", UriKind.Relative));
                    this.photoPreview.Stretch = Stretch.None;
                    this.photoPreview.SetValue(Canvas.MarginProperty, new Thickness(9, 69, 6, 0));
                });
            }
            else
            {
                Dispatcher.BeginInvoke(() => {
                    MessageBox.Show("Error Posting Photo: " + response.Content);
                    this.captionTextbox.IsEnabled = true; // Re-enable the text box
                });
            }
        }
    }
}