using System.Drawing;

namespace MPN.Framework.Imaging
{
	public class ImageHelper
	{
		/// <summary>
		/// Returns the width and height of an image by inspecting the image file itself.
		/// </summary>
		/// <param name="path">The full file-system path (local or UNC) to the image file.</param>
		public static Size GetImageDimensions(string path)
		{
			Size size;
			using (var i = Image.FromFile(path))
				size = i.Size;
			
			return size;
		}
	}
}