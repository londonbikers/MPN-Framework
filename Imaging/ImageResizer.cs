using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace MPN.Framework.Imaging
{
	/// <summary>
	/// Resizes images in a number of ways.
	/// </summary>
	public class ImageResizer
	{
		#region enums
		/// <summary>
		/// Defines an axis of an object.
		/// </summary>
		public enum Axis
		{
			Vertical,
			Horizontal,
			Undefined
		}
		#endregion

		#region public methods
        /// <summary>
        /// Saves a resized image to a destination path.
        /// </summary>
        public void SaveResizeImage(string sourceImagePath, string destinationImagePath, int primaryAxisLength, bool constrainToSquare, Axis axisToResize, Size? specificSize)
        {
            if (string.IsNullOrEmpty(sourceImagePath) || !File.Exists(sourceImagePath))
                throw new ArgumentNullException();

            var sourceImage = Image.FromFile(sourceImagePath);
            if (sourceImage == null)
                throw new ArgumentException("sourceImagePath is not a valid image file! cannot open.");

            Image destinationImage = null;
			if (specificSize.HasValue)
				destinationImage = ResizeImage(sourceImage, specificSize.Value);
            else if (axisToResize != Axis.Undefined)
                destinationImage = ResizeImage(sourceImage, primaryAxisLength, axisToResize);
            else
                destinationImage = ResizeImage(sourceImage, primaryAxisLength, constrainToSquare);
            
            // ensure destination folder exists.
            if (!Directory.Exists(Path.GetDirectoryName(destinationImagePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(destinationImagePath));
            
            try
            {
                // output jpeg settings.
                var info = ImageCodecInfo.GetImageEncoders();
                var parameters = new EncoderParameters(1);
                parameters.Param[0] = new EncoderParameter(Encoder.Quality, (long)95);
                
                // save image file.
                destinationImage.Save(destinationImagePath, info[1], parameters);
            }
            finally
            {
                // all done, release image resources.
                sourceImage.Dispose();

                if (destinationImage != null)
                    destinationImage.Dispose();
            }
        }

		/// <summary>
		/// Resizes an image against its primary axis, i.e. if it's portrait it will be resized upon the height, otherwise it's width for landscape.
		/// </summary>
		/// <param name="sourceImage">The source image to resize.</param>
		/// <param name="primaryAxisLength">The length in pixels to resize the primary axis to.</param>
		public Image ResizeImage(Image sourceImage, int primaryAxisLength)
		{
			return PerformResize(sourceImage, primaryAxisLength, false, Axis.Undefined, null);
		}

		/// <summary>
		/// Resizes an image using a specified axis, i.e. horizontal allows you to specify a width and it will scale the height automatically.
		/// </summary>
		/// <param name="sourceImage">The source image to resize.</param>
		/// <param name="primaryAxisLength">The length in pixels to resize the primary axis to.</param>
		/// <param name="axisToResize">Which axis will be used to specify the new dimenion.</param>
		public Image ResizeImage(Image sourceImage, int primaryAxisLength, Axis axisToResize)
		{
			return PerformResize(sourceImage, primaryAxisLength, false, axisToResize, null);
		}

		/// <summary>
		/// Resizes an image against its primary axis, i.e. if it's portrait it will be resized upon the height, otherwise it's width for landscape.
		/// </summary>
		/// <param name="sourceImage">The source image to resize.</param>
		/// <param name="primaryAxisLength">The length in pixels to resize the primary axis to.</param>
		/// <param name="squareImage">Gives the option of constraining the image canvas to a squad, i.e. chopping excess off.</param>
		public Image ResizeImage(Image sourceImage, int primaryAxisLength, bool squareImage)
		{
			return PerformResize(sourceImage, primaryAxisLength, squareImage, Axis.Undefined, null);
		}

        /// <summary>
        /// Resizes an image to a specific set of dimensions, i.e. crops it.
        /// </summary>
        /// <param name="sourceImage">The source image to resize.</param>
        /// <param name="dimensions">The dimensions to crop the image to.</param>
        public Image ResizeImage(Image sourceImage, Size dimensions)
        {
            return PerformResize(sourceImage, 0, false, Axis.Undefined, dimensions);
        }
		#endregion

		#region private methods
		/// <summary>
		/// Performs the actual resizing of the image. Flexible in terms of how the image is to be resized.
		/// </summary>
		/// <param name="sourceImage">The source image to resize.</param>
		/// <param name="desiredPrimaryAxisLength">The desired width of the image.</param>
		/// <param name="constrainToSquare">Determines whether or not the image should be contrained to a squad.</param>
		/// <param name="primaryAxis">Which axis will be used to specify the new dimenion. An Undefined axis means the natural primary will be used (excluding square instructions).</param>
        /// <param name="specificSize">If supplied, this will constrain the image to a specific pair of dimensions, i.e. crop the image.</param>
		private Image PerformResize(Image sourceImage, int desiredPrimaryAxisLength, bool constrainToSquare, Axis primaryAxis, Size? specificSize)
		{
			if (sourceImage == null)
				return null;

			// portrait, landscape or square?
			int originalPrimarySize;
			switch (primaryAxis)
			{
			    case Axis.Horizontal:
			        originalPrimarySize = sourceImage.Width;
			        break;
			    case Axis.Vertical:
			        originalPrimarySize = sourceImage.Height;
			        break;
			    default:
			        originalPrimarySize = (sourceImage.Width > sourceImage.Height) ? sourceImage.Width : sourceImage.Height;
			        break;
			}

			// we only resize down, not up.
			if (desiredPrimaryAxisLength >= originalPrimarySize)
				return sourceImage;

			// determine the new scaled image dimensions.
			var scaledHeight = sourceImage.Height;
			var scaledWidth = sourceImage.Width;
			var xPlacement = 0;
			var yPlacement = 0;
			Image resizedImage = null;

            if (specificSize.HasValue)
            {
                primaryAxis = Axis.Horizontal;
                desiredPrimaryAxisLength = specificSize.Value.Width;
            }			
            
            if (constrainToSquare)
			{
				#region square image
				// determine floating image size.
				if (sourceImage.Width >= sourceImage.Height)
				{
					// landscape
					if (sourceImage.Height < desiredPrimaryAxisLength)
					{
						// do not enlarge the picture.
						desiredPrimaryAxisLength = sourceImage.Height;
					}
					else
					{
						// get new size based on new height.
						scaledWidth = Convert.ToInt32(desiredPrimaryAxisLength * sourceImage.Width / sourceImage.Height);
						scaledHeight = desiredPrimaryAxisLength;
					}

					xPlacement = 0 - ((scaledWidth - desiredPrimaryAxisLength) / 2);
				}
				else
				{
					// portrait
					if (sourceImage.Width < desiredPrimaryAxisLength)
					{
						// do not enlarge the picture.
						desiredPrimaryAxisLength = sourceImage.Width;
					}
					else
					{
						// get new size based on new width.
						scaledWidth = desiredPrimaryAxisLength;
						scaledHeight = Convert.ToInt32(scaledWidth * sourceImage.Height / sourceImage.Width);
					}

					yPlacement = 0 - ((scaledHeight - desiredPrimaryAxisLength) / 2);
				}

				// canvas is now square according to the primary axis.
				resizedImage = new Bitmap(desiredPrimaryAxisLength, desiredPrimaryAxisLength);
				#endregion
			}
			else switch (primaryAxis)
			{
			    case Axis.Horizontal:
			        scaledWidth = desiredPrimaryAxisLength;
			        scaledHeight = Convert.ToInt32(scaledWidth * sourceImage.Height / sourceImage.Width);
			        resizedImage = new Bitmap(scaledWidth, scaledHeight);
			        break;
			    case Axis.Vertical:
			        scaledWidth = Convert.ToInt32(desiredPrimaryAxisLength * sourceImage.Width / sourceImage.Height);
			        scaledHeight = desiredPrimaryAxisLength;
			        resizedImage = new Bitmap(scaledWidth, scaledHeight);
			        break;
			    default:
			        if (sourceImage.Width >= sourceImage.Height)
			        {
			            // landscape
			            scaledWidth = desiredPrimaryAxisLength;
			            scaledHeight = Convert.ToInt32(scaledWidth * sourceImage.Height / sourceImage.Width);
			        }
			        else
			        {
			            // portrait
			            scaledWidth = Convert.ToInt32(desiredPrimaryAxisLength * sourceImage.Width / sourceImage.Height);
			            scaledHeight = desiredPrimaryAxisLength;
			        }
			        resizedImage = new Bitmap(scaledWidth, scaledHeight);
			        break;
			}

            if (specificSize.HasValue)
            {
                // we've just re-used a resizer above, now to crop. 
                resizedImage.Dispose();
                resizedImage = new Bitmap(specificSize.Value.Width, specificSize.Value.Height);
                yPlacement = 0 - ((scaledHeight - specificSize.Value.Height) / 2);
            }

			var graphic = Graphics.FromImage(resizedImage);
			graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphic.SmoothingMode = SmoothingMode.HighQuality;
			graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
			graphic.CompositingQuality = CompositingQuality.HighQuality;
			graphic.DrawImage(sourceImage, xPlacement, yPlacement, scaledWidth, scaledHeight);

			// free up any resources.
			sourceImage.Dispose();
			graphic.Dispose();

			return resizedImage;
		}
		#endregion
	}
}