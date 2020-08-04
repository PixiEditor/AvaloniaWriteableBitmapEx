using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvaloniaWriteableBitmapEx
{
    public static partial class WriteableBitmapEx
    {
        public const int SizeOfArgb = 4;
        /// <summary>
        /// Copies the Pixels from the WriteableBitmap into a ARGB byte array starting at a specific Pixels index.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="offset">The starting Pixels index.</param>
        /// <param name="count">The number of Pixels to copy, -1 for all</param>
        /// <returns>The color buffer as byte ARGB values.</returns>
        public static byte[] ToByteArray(this WriteableBitmap bmp, int offset, int count)
        {
            using (var context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                if (count == -1)
                {
                    // Copy all to byte array
                    count = context.Length;
                }

                var len = count * SizeOfArgb;
                var result = new byte[len]; // ARGB
                BitmapContext.BlockCopy(context, offset, result, 0, len);
                return result;
            }
        }
    }
}
