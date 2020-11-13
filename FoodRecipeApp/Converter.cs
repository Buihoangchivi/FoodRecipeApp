using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.IO;
using System.Windows.Media.Imaging;

namespace FoodRecipeApp
{
    class Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string relative = (string)value;
            string absolutePath;
            if (relative != null && !relative.Contains(":\\"))
            {
                string folder = AppDomain.CurrentDomain.BaseDirectory;
                absolutePath = $"{folder}{relative}";
            }
            else
            {
                string folder = "";
                absolutePath = $"{folder}{relative}";
            }
            if (relative != null)
            {
                var image = ConvertToImage(absolutePath);
                return image;
            }
            else
            {
                return absolutePath;
            }
        }

        public static BitmapImage ConvertToImage(string path)
        {
           
                BitmapImage bitmapImage = null;

                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new FileStream(path, FileMode.Open, FileAccess.Read);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.StreamSource.Dispose();
           
                return bitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
