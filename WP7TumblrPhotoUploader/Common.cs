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
    public class Common
    {
        public const string OAUTH_CONSUMER_KEY = "ddRwNYFhclqTMDM8VGCUNwlJEEPWQjLWWpYMhrockMaQBKlUiG";
        public const string OAUTH_CONSUMER_SECRET = "b5V0p8jP8qaiviUttv2aym41S2YiiOkYbzShsqVrUAtkIHTjyH";


        /**
 * Given a querystring and a field name, return the value of the field
 **/
        public static string GetValueFromQueryString(string queryString, string field)
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
