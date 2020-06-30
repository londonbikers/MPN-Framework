using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace MPN.Framework.Imaging
{
	public class ImageWatermarker
	{
		#region enums
		/// <summary>
		/// Denotes the relative position of the watermark over an image.
		/// </summary>
		public enum WatermarkLocation
		{
			BottomLeft,
			BottomMiddle,
			BottomRight,
			TopLeft,
			TopRight,
            Center
		}
		#endregion

		#region public methods
		/// <summary>
		/// Adds the site logo to the image as a watermark.
		/// </summary>
		public Bitmap WatermarkImage(Image sourceImage, Image imgWatermark, WatermarkLocation location)
		{
			var phWidth = sourceImage.Width;
			var phHeight = sourceImage.Height;

			// create an image object containing the watermark
			var wmWidth = imgWatermark.Width;
			var wmHeight = imgWatermark.Height;

			// create a Bitmap the Size of the original photograph
			var bmPhoto = new Bitmap(phWidth, phHeight, PixelFormat.Format32bppRgb);
			bmPhoto.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

			// load the Bitmap into a Graphics object 
			var grPhoto = Graphics.FromImage(bmPhoto);

			// set the rendering quality for this Graphics object
			grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
			grPhoto.SmoothingMode = SmoothingMode.HighQuality;
			grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
			grPhoto.CompositingQuality = CompositingQuality.HighQuality;

			// draws the photo Image object at original size to the graphics object.
			grPhoto.DrawImage(
				sourceImage,                               // Photo Image object
				new Rectangle(0, 0, phWidth, phHeight), // Rectangle structure
				0,                                      // x-coordinate of the portion of the source image to draw. 
				0,                                      // y-coordinate of the portion of the source image to draw. 
				phWidth,                                // Width of the portion of the source image to draw. 
				phHeight,                               // Height of the portion of the source image to draw. 
				GraphicsUnit.Pixel);                    // Units of measure 

			//------------------------------------------------------------
			//Step #2 - Insert Watermark image
			//------------------------------------------------------------

			// create a Bitmap based on the previously modified photograph Bitmap.
			var bmWatermark = new Bitmap(bmPhoto);
			bmWatermark.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

			// load this Bitmap into a new Graphic Object
			var grWatermark = Graphics.FromImage(bmWatermark);

			// to achieve a transulcent watermark we will apply (2) color 
			// manipulations by defineing a ImageAttributes object and 
			// seting (2) of its properties.
			var imageAttributes = new ImageAttributes();

			// the second color manipulation is used to change the opacity of the 
			// watermark.  This is done by applying a 5x5 matrix that contains the 
			// coordinates for the RGBA space.  By setting the 3rd row and 3rd column 
			// to 0.3f we achive a level of opacity
			float[][] colorMatrixElements = { 
												new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},       
												new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},        
												new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},        
												new float[] {0.0f,  0.0f,  0.0f,  1.0f, 0.0f},        
												new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}};

			var wmColorMatrix = new ColorMatrix(colorMatrixElements);
			imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			// watermark location.
			int xPosOfWm;
			int yPosOfWm;

			switch (location)
			{
			    case WatermarkLocation.BottomMiddle:
			        {
			            const int bottomOffset = 5;
			            xPosOfWm = ((phWidth / 2) - (wmWidth / 2));
			            yPosOfWm = ((phHeight - wmHeight) - bottomOffset);
			        }
			        break;
			    case WatermarkLocation.BottomRight:
			        {
			            const int offset = 0;
			            xPosOfWm = ((phWidth - wmWidth) - offset);
			            yPosOfWm = ((phHeight - wmHeight) - offset);
			        }
			        break;
			    case WatermarkLocation.TopLeft:
			        {
			            const int offset = 5;
			            xPosOfWm = offset;
			            yPosOfWm = offset;
			        }
			        break;
			    case WatermarkLocation.TopRight:
			        {
			            const int offset = 5;
			            xPosOfWm = ((phWidth - wmWidth) - offset);
			            yPosOfWm = offset;
			        }
			        break;
			    case WatermarkLocation.Center:
			        {
			            var bottomOffset = (phHeight / 2) - (wmHeight / 2);
			            xPosOfWm = ((phWidth / 2) - (wmWidth / 2));
			            yPosOfWm = ((phHeight - wmHeight) - bottomOffset);
			        }
			        break;
			    default:
			        {
			            // bottom-left position.
			            const int offset = 0;
			            xPosOfWm = offset;
			            yPosOfWm = ((phHeight - wmHeight) - offset);
			        }
			        break;
			}

			grWatermark.DrawImage(imgWatermark,
				new Rectangle(xPosOfWm, yPosOfWm, wmWidth, wmHeight),  //Set the detination Position
				0,                  // x-coordinate of the portion of the source image to draw. 
				0,                  // y-coordinate of the portion of the source image to draw. 
				wmWidth,            // Watermark Width
				wmHeight,		    // Watermark Height
				GraphicsUnit.Pixel, // Unit of measurment
				imageAttributes);   //ImageAttributes Object

			// replace the original photgraphs bitmap with the new Bitmap
			sourceImage.Dispose();
			bmPhoto.Dispose();
			imgWatermark.Dispose();
			grPhoto.Dispose();
			grWatermark.Dispose();

			return bmWatermark;
		}
		#endregion
	}
}