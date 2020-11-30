using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace Task_3
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is Blob rawUri && targetType == typeof(IBitmap))
            {
                //Console.WriteLine('M');
                //return 'N';
                return ClassificationVM.ByteArrayToImage(rawUri.Bytes);
                //return new Bitmap("/Users/alexandra/Desktop/imagen/n07753113_7675_fig.jpg");
            }
            throw new NotSupportedException();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
