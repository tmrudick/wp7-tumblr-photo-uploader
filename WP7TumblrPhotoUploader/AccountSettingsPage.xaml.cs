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
    public partial class AccountSettingsPage : PhoneApplicationPage
    {
        public AccountSettingsPage()
        {
            InitializeComponent();
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

            // Then go back
            NavigationService.GoBack();
        }
    }
}