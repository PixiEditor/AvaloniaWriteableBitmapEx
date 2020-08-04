using Avalonia;
using Avalonia.Media.Imaging;

namespace AvaloniaWriteableBitmapEx
{
    public static partial class WriteableBitmapEx
    {
        public enum Interpolation
        {
            /// <summary>
            /// The nearest neighbor algorithm simply selects the color of the nearest pixel.
            /// </summary>
            NearestNeighbor = 0,

            /// <summary>
            /// Linear interpolation in 2D using the average of 3 neighboring pixels.
            /// </summary>
            Bilinear,
        }

        /// <summary>
        /// Creates a new cropped WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="width">The width of the rectangle that defines the crop region.</param>
        /// <param name="height">The height of the rectangle that defines the crop region.</param>
        /// <returns>A new WriteableBitmap that is a cropped version of the input.</returns>
        public static WriteableBitmap Crop(this WriteableBitmap bmp, int x, int y, int width, int height)
        {
            using (var srcContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                var srcWidth = srcContext.Width;
                var srcHeight = srcContext.Height;

                // If the rectangle is completely out of the bitmap
                if (x > srcWidth || y > srcHeight)
                {
                    return BitmapFactory.New(0, 0);
                }

                // Clamp to boundaries
                if (x < 0) x = 0;
                if (x + width > srcWidth) width = srcWidth - x;
                if (y < 0) y = 0;
                if (y + height > srcHeight) height = srcHeight - y;

                // Copy the pixels line by line using fast BlockCopy
                var result = BitmapFactory.New(width, height);
                using (var destContext = result.GetBitmapContext())
                {
                    for (var line = 0; line < height; line++)
                    {
                        var srcOff = ((y + line) * srcWidth + x) * SizeOfArgb;
                        var dstOff = line * width * SizeOfArgb;
                        BitmapContext.BlockCopy(srcContext, srcOff, destContext, dstOff, width * SizeOfArgb);
                    }

                    return result;
                }
            }

        }

        /// <summary>
        /// Creates a new cropped WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="region">The rectangle that defines the crop region.</param>
        /// <returns>A new WriteableBitmap that is a cropped version of the input.</returns>
        public static WriteableBitmap Crop(this WriteableBitmap bmp, Rect region)
        {
            return bmp.Crop((int)region.X, (int)region.Y, (int)region.Width, (int)region.Height);
        }

        /// <summary>
        /// Creates a new resized WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="width">The new desired width.</param>
        /// <param name="height">The new desired height.</param>
        /// <param name="interpolation">The interpolation method that should be used.</param>
        /// <returns>A new WriteableBitmap that is a resized version of the input.</returns>
        public static WriteableBitmap Resize(this WriteableBitmap bmp, int width, int height, Interpolation interpolation)
        {
            using (var srcContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                var pd = Resize(srcContext, srcContext.Width, srcContext.Height, width, height, interpolation);

                var result = BitmapFactory.New(width, height);
                using (var dstContext = result.GetBitmapContext())
                {
                    BitmapContext.BlockCopy(pd, 0, dstContext, 0, SizeOfArgb * pd.Length);
                }
                return result;
            }
        }

        /// <summary>
        /// Creates a new resized bitmap.
        /// </summary>
        /// <param name="srcContext">The source context.</param>
        /// <param name="widthSource">The width of the source pixels.</param>
        /// <param name="heightSource">The height of the source pixels.</param>
        /// <param name="width">The new desired width.</param>
        /// <param name="height">The new desired height.</param>
        /// <param name="interpolation">The interpolation method that should be used.</param>
        /// <returns>A new bitmap that is a resized version of the input.</returns>
        public static int[] Resize(BitmapContext srcContext, int widthSource, int heightSource, int width, int height, Interpolation interpolation)
        {
            return Resize(srcContext.Pixels, widthSource, heightSource, width, height, interpolation);
        }

        /// <summary>
        /// Creates a new resized bitmap.
        /// </summary>
        /// <param name="pixels">The source pixels.</param>
        /// <param name="widthSource">The width of the source pixels.</param>
        /// <param name="heightSource">The height of the source pixels.</param>
        /// <param name="width">The new desired width.</param>
        /// <param name="height">The new desired height.</param>
        /// <param name="interpolation">The interpolation method that should be used.</param>
        /// <returns>A new bitmap that is a resized version of the input.</returns>
        public static int[] Resize(int[] pixels, int widthSource, int heightSource, int width, int height, Interpolation interpolation)
        {
            var pd = new int[width * height];
            var xs = (float)widthSource / width;
            var ys = (float)heightSource / height;

            float fracx, fracy, ifracx, ifracy, sx, sy, l0, l1, rf, gf, bf;
            int c, x0, x1, y0, y1;
            byte c1a, c1r, c1g, c1b, c2a, c2r, c2g, c2b, c3a, c3r, c3g, c3b, c4a, c4r, c4g, c4b;
            byte a, r, g, b;

            // Nearest Neighbor
            if (interpolation == Interpolation.NearestNeighbor)
            {
                var srcIdx = 0;
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        sx = x * xs;
                        sy = y * ys;
                        x0 = (int)sx;
                        y0 = (int)sy;

                        pd[srcIdx++] = pixels[y0 * widthSource + x0];
                    }
                }
            }

            // Bilinear
            else if (interpolation == Interpolation.Bilinear)
            {
                var srcIdx = 0;
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        sx = x * xs;
                        sy = y * ys;
                        x0 = (int)sx;
                        y0 = (int)sy;

                        // Calculate coordinates of the 4 interpolation points
                        fracx = sx - x0;
                        fracy = sy - y0;
                        ifracx = 1f - fracx;
                        ifracy = 1f - fracy;
                        x1 = x0 + 1;
                        if (x1 >= widthSource)
                        {
                            x1 = x0;
                        }
                        y1 = y0 + 1;
                        if (y1 >= heightSource)
                        {
                            y1 = y0;
                        }


                        // Read source color
                        c = pixels[y0 * widthSource + x0];
                        c1a = (byte)(c >> 24);
                        c1r = (byte)(c >> 16);
                        c1g = (byte)(c >> 8);
                        c1b = (byte)(c);

                        c = pixels[y0 * widthSource + x1];
                        c2a = (byte)(c >> 24);
                        c2r = (byte)(c >> 16);
                        c2g = (byte)(c >> 8);
                        c2b = (byte)(c);

                        c = pixels[y1 * widthSource + x0];
                        c3a = (byte)(c >> 24);
                        c3r = (byte)(c >> 16);
                        c3g = (byte)(c >> 8);
                        c3b = (byte)(c);

                        c = pixels[y1 * widthSource + x1];
                        c4a = (byte)(c >> 24);
                        c4r = (byte)(c >> 16);
                        c4g = (byte)(c >> 8);
                        c4b = (byte)(c);


                        // Calculate colors
                        // Alpha
                        l0 = ifracx * c1a + fracx * c2a;
                        l1 = ifracx * c3a + fracx * c4a;
                        a = (byte)(ifracy * l0 + fracy * l1);

                        // Red
                        l0 = ifracx * c1r + fracx * c2r;
                        l1 = ifracx * c3r + fracx * c4r;
                        rf = ifracy * l0 + fracy * l1;

                        // Green
                        l0 = ifracx * c1g + fracx * c2g;
                        l1 = ifracx * c3g + fracx * c4g;
                        gf = ifracy * l0 + fracy * l1;

                        // Blue
                        l0 = ifracx * c1b + fracx * c2b;
                        l1 = ifracx * c3b + fracx * c4b;
                        bf = ifracy * l0 + fracy * l1;

                        // Cast to byte
                        r = (byte)rf;
                        g = (byte)gf;
                        b = (byte)bf;

                        // Write destination
                        pd[srcIdx++] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }
            }
            return pd;
        }
    }
}
