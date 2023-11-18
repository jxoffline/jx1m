using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace GameServer.KiemThe.Utilities.Libraries
{
    /// <summary>
    /// Generates a captcha image based on the captcha code string given.
    /// <br></br>
    /// <seealso cref="https://github.com/vishnuprasadv/CaptchaGen/blob/master/CaptchaGen/ImageFactory.cs"/>
    /// </summary>
    public static class CaptchaImageGenerator
    {
        /// <summary>
        /// Amount of distortion required.
        /// Default value = 18
        /// </summary>
        public static int Distortion { get; set; } = 18;
        const int HEIGHT = 96;
        const int WIDTH = 150;
        const string FONTFAMILY = "Tahoma";
        const int FONTSIZE = 25;

        /// <summary>
        /// Background color to be used.
        /// Default value = Color.Wheat
        /// </summary>
        public static Color BackgroundColor { get; set; } = Color.Transparent;


        /// <summary>
        /// Generates the image with default image properties(150px X 96px) and distortion
        /// </summary>
        /// <param name="captchaCode">Captcha code for which the image has to be generated</param>
        /// <returns>Generated jpeg image as a MemoryStream object</returns>
        public static MemoryStream GenerateImage(string captchaCode)
        {
            return CaptchaImageGenerator.BuildImage(captchaCode, HEIGHT, WIDTH, FONTSIZE, Distortion);
        }

        /// <summary>
        /// Generates the image with given image properties
        /// </summary>
        /// <param name="captchaCode">Captcha code for which the image has to be generated</param>
        /// <param name="imageHeight">Height of the image to be generated</param>
        /// <param name="imageWidth">Width of the image to be generated</param>
        /// <param name="fontSize">Font size to be used</param>
        /// <param name="distortion">Distortion required</param>
        /// <returns>Generated jpeg image as a MemoryStream object</returns>
        public static MemoryStream GenerateImage(string captchaCode, int imageHeight, int imageWidth, int fontSize, int distortion)
        {
            return CaptchaImageGenerator.BuildImage(captchaCode, imageHeight, imageWidth, fontSize, distortion);
        }


        /// <summary>
        /// Generates the image with given image properties
        /// </summary>
        /// <param name="captchaCode">Captcha code for which the image has to be generated</param>
        /// <param name="imageHeight">Height of the image to be generated</param>
        /// <param name="imageWidth">Width of the image to be generated</param>
        /// <param name="fontSize">Font size to be used</param>
        /// <returns>Generated jpeg image as a MemoryStream object</returns>
        public static MemoryStream GenerateImage(string captchaCode, int imageHeight, int imageWidth, int fontSize)
        {
            return CaptchaImageGenerator.BuildImage(captchaCode, imageHeight, imageWidth, fontSize, Distortion);
        }

        /// <summary>
        /// Actual image generator. Internally used.
        /// </summary>
        /// <param name="captchaCode">Captcha code for which the image has to be generated</param>
        /// <param name="imageHeight">Height of the image to be generated</param>
        /// <param name="imageWidth">Width of the image to be generated</param>
        /// <param name="fontSize">Font size to be used</param>
        /// <param name="distortion">Distortion required</param>
        /// <returns>Generated jpeg image as a MemoryStream object</returns>
        private static MemoryStream BuildImage(string captchaCode, int imageHeight, int imageWidth, int fontSize, int distortion)
        {
            int newX, newY;
            MemoryStream memoryStream = new MemoryStream();
            Bitmap captchaImage = new Bitmap(imageWidth, imageHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Bitmap cache = new Bitmap(imageWidth, imageHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            /// Load graphic
            Graphics graphicsTextHolder = Graphics.FromImage(captchaImage);
            Rectangle rect = new Rectangle(0, 0, imageWidth, imageHeight);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            /// Add spaces
            captchaCode = string.Join(" ", captchaCode.ToCharArray());
            graphicsTextHolder.DrawString(captchaCode, new Font("Arial", fontSize, FontStyle.Italic), Brushes.White, rect, stringFormat);
            HatchBrush hatchBrush = new HatchBrush(HatchStyle.DottedDiamond, Color.LightGray, Color.Transparent);
            graphicsTextHolder.FillRectangle(hatchBrush, rect);

            /// Distort the image with a wave function
            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    newX = (int) (x + (distortion * Math.Sin(Math.PI * y / 35)));
                    newY = (int) (y + (distortion * Math.Cos(Math.PI * x / 35)));
                    if (newX < 0 || newX >= imageWidth) newX = 0;
                    if (newY < 0 || newY >= imageHeight) newY = 0;
                    cache.SetPixel(x, y, captchaImage.GetPixel(newX, newY));
                }
            }

            ///// Make noise using Gaussian filter
            //ImageProcessor.GaussianNoise(cache);

            cache.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;

            /// Release memory
            graphicsTextHolder.Dispose();
            captchaImage.Dispose();
            cache.Dispose();
            hatchBrush.Dispose();

            /// Return result
            return memoryStream;
        }
    }
}