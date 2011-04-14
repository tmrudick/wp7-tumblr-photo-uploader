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

namespace WP7TumblrPhotoUploader
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Holds if the caption text box holds the default "Enter Caption" test or not
        private bool hasDefaultText = true;

        // Byte array of the currently selected iamge or null if nothing has been selected
        private byte[] photo;

        private PhotoChooserTask photoChooser;
        
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
    }
}