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

namespace WP7TumblrPhotoUploader
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Holds if the caption text box holds the default "Enter Caption" test or not
        private bool hasDefaultText = true;

        // Byte array of the currently selected iamge or null if nothing has been selected
        private byte[] photo;

        private PhotoChooserTask photoChooser;

        private const string TUMBLR_AUTHORITY = "http://tumblr.com/api";
        private const string TUMBLR_POST_PATH = "/write";

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
            this.photoChooser.Show();
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

                // Now here comes the POST!

                // Create a RestClient
                RestClient client = new RestClient();
                client.Authority = MainPage.TUMBLR_AUTHORITY;

                // Create the request
                RestRequest request = new RestRequest();
                request.Path = MainPage.TUMBLR_POST_PATH;
                request.Method = Hammock.Web.WebMethod.Post;

                // Set the correct credentials on the client or request depending on auth method
                if (userCredentials.Type == TumblrCredentials.CredentialsType.OAuth)
                {
                    // TODO: OAuth stuff
                }
                else
                {
                    request.AddField("email", userCredentials.Username);
                    request.AddField("password", userCredentials.Password);
                }

                // Add metadata fields
                request.AddField("type", "photo");
                request.AddField("state", "draft"); // Debug line for testing
                request.AddField("send-to-twitter", "no"); // Debug line because I'm paranoid
                
                // Add caption but check for an empty field
                if (!this.hasDefaultText)
                {
                    request.AddField("caption", this.captionTextbox.Text);
                }

                // Add the photo but check for an empty photo
                if (this.photo != null)
                {
                    request.AddFile("data", "upload.jpg", new MemoryStream(photo));
                    // TODO: Some sort of error handling if this condition is not met.
                    // TODO: This check should probably be done first.
                }

                // Send the request of to la-la-land
                client.BeginRequest(request, new RestCallback(PostCompleted));

                // TODO: Add notification of progress
            }
        }

        /**
         * ASync callback for posting a new photo
         **/
        public void PostCompleted(RestRequest request, RestResponse response, object target)
        {
            // TODO: Add notification of completion
        }
    }
}