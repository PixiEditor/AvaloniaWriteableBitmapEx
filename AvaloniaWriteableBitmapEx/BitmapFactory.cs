using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvaloniaWriteableBitmapEx
{
    public static class BitmapFactory
    {

        /// <summary>
        /// Creates a new WriteableBitmap of the specified width and height
        /// </summary>
        /// <remarks>Default DPI is 96x96 and PixelFormat is Bgra8888</remarks>
        /// <param name="pixelWidth"></param>
        /// <param name="pixelHeight"></param>
        /// <returns></returns>
        public static WriteableBitmap New(int pixelWidth, int pixelHeight)
        {
            if (pixelHeight < 1) pixelHeight = 1;
            if (pixelWidth < 1) pixelWidth = 1;

            return new WriteableBitmap(new Avalonia.PixelSize(pixelWidth, pixelHeight), new Avalonia.Vector(96.0, 96.0), 
            Avalonia.Platform.PixelFormat.Bgra8888);
        }
    }
}
