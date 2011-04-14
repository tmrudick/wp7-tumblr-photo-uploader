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

namespace WP7TumblrPhotoUploader
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Holds if the caption text box holds the default "Enter Caption" test or not
        private bool hasDefaultText = true;
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void captionTextbox_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (this.hasDefaultText)
            {
                this.hasDefaultText = false;
                this.captionTextbox.Text = string.Empty;
                this.captionTextbox.TextAlignment = TextAlignment.Left;
            }
        }

        private void captionTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.captionTextbox.Text.Equals(string.Empty))
            {
                this.hasDefaultText = true;
                this.captionTextbox.Text = "Enter a Caption...";
                this.captionTextbox.TextAlignment = TextAlignment.Center;
            }

        }
    }
}