using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WP7TumblrPhotoUploader
{
    public class TumblrCredentials
    {
        public enum CredentialsType {BasicAuth, OAuth};

        public CredentialsType Type
        {
            get
            {
                if (oauthToken.Equals(string.Empty))
                {
                    return CredentialsType.BasicAuth;
                }
                else
                {
                    return CredentialsType.OAuth;
                }
            }
        }

        // Old style username and password
        private string Username { get; set; }
        private string password { get; set; }

        // New style OAuth this and that
        private string oauthToken { get; set; }
        private string oauthTokenSecret { get; set; }
    }
}
