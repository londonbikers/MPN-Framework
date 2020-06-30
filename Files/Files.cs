using System;
using System.Text.RegularExpressions;
using System.IO;
using MPN.Framework.Content;

namespace MPN.Framework.Files
{
    public class Files
    {
		/// <summary>
		/// Returns the file-size of a file.
		/// </summary>
		/// <param name="path">The full local or UNC path to the file.</param>
		public static long GetFileSize(string path)
		{
			return new FileInfo(path).Length;
		}

        /// <summary>
        /// This function checks to see if there's a file already in the supplied path
        /// with the filename, if one's found, it'll adapt it to make sure it's unique.
        /// </summary>
        public static string GetUniqueFilename(string path, string filename)
        {
            filename = filename.Trim();
            var isUnique = false;
            var filenamePart = Path.GetFileNameWithoutExtension(filename);
            var extension = Path.GetExtension(filename);

            if (!path.EndsWith("\\"))
                path += "\\";

            while (!isUnique)
            {
                if (File.Exists(path + filename))
                {
                    filenamePart += string.Format("-{0}", DateTime.Now.Second);
                    filename = filenamePart + extension;
                }
                else
                {
                    isUnique = true;
                }
            }

            return filename;
        }

        /// <summary>
        /// Transforms a filename so that it doesn't contain any illegal characters.
        /// </summary>
        /// <param name="filename">The filename to transform.</param>
        public static string GetSafeFilename(string filename)
        {
            var name = Path.GetFileNameWithoutExtension(filename);
            var extension = Path.GetExtension(filename);

            // hypens and spaces are acceptable, so let's just translate them for now.
            name = name.Replace("%20", " ");
            name = name.Replace(" ", "-");
            name = name.Replace("-", "ZZHYPZZ");
            name = name.Replace("_", "ZZHYPZZ");
            name = name.Replace(extension, String.Empty);
            name = RegularExpressions.RemoveNonAlphaNumericCharacters(name);
            name = name.Replace("ZZHYPZZ", "-");

            return name + extension;
        }

        /// <summary>
        /// Attempts to translate a mime-type into a file extension. Currently support os image types only.
        /// </summary>
        /// <param name="mimeType">The mime-type signature, i.e. 'image/jpeg'.</param>
        /// <returns>An extension including the period, or an empty string if translation not possible.</returns>
        public static string GetFileExtensionFromMimeType(string mimeType)
        {
            if (mimeType == String.Empty)
                return String.Empty;

            var extension = String.Empty;
            mimeType = mimeType.ToLower().Trim();
            switch (mimeType)
            {
                case "image/pjpeg":
                case "image/jpeg":
                    extension = ".jpg";
                    break;
                case "image/gif":
                    extension = ".gif";
                    break;
                case "image/png":
                    extension = ".png";
                    break;
                case "image/bmp":
                    extension = ".bmp";
                    break;
            }

            return extension;
        }

        /// <summary>
        /// Determines if a filename has a traditional image signature, i.e. ends with an image file extension.
        /// </summary>
        /// <param name="filename">The filename or path and filename to inspect.</param>
        public static bool IsFilenameAnImage(string filename)
        {
            filename = filename.ToLower().Trim();
            return filename.EndsWith(".hdp") || filename.EndsWith(".jpg") || filename.EndsWith(".jpeg") || filename.EndsWith(".gif") || filename.EndsWith(".png") || filename.EndsWith(".bmp");
        }

        /// <summary>
        /// Attempts to delete a file and keeps on attempting until a file lock is released or the timeout value is reached.
        /// </summary>
        /// <param name="msTimeout">The time in miliseconds to wait before giving up.</param>
        /// <param name="filePath">The file-system path to the file to delete.</param>
        public static bool DeleteFile(int msTimeout, string filePath)
		{
			var startTime = DateTime.Now;
			while (DateTime.Now.Subtract(startTime).Milliseconds < msTimeout)
			{
				try
				{
					File.Delete(filePath);
					return true;
				}
				catch (FileNotFoundException)
				{
					// bad file-path, or the file has already been deleted.
					Logger.LogWarning(string.Format("DeleteFile() - File not found: '{0}'.", filePath));
					return false;
				}
				catch
				{
				    // file must be locked, continue.
				}
			}

			return false;
		}

		/// <summary>
		/// Translates a byte filesize into a human-friendly filesize, i.e. 24kb, 31mb or 43.01gb
		/// </summary>
		public static string GetFriendlyFilesize(long filesize)
		{
			if (filesize < 1)
				return "-";

			if (filesize < 1048576)
			{
				// under one mb.
				return (filesize / 1024L).ToString("N0") + "kb";
			}
			else if (filesize < 1073741824)
			{
				// under one gb.
				return (filesize / 1048576L).ToString("N0") + "mb";
			}
			else
			{
				// over one gb. could fail but I can't get it to do a proper divide.
                try
                {
                    var gb = (decimal)filesize / (decimal)1073741824;
                    return gb.ToString("N2") + "gb";
                }
                catch
                {
                    return "??";
                }
			}
		}

		/// <summary>
		/// Attempts to extract a human-friendly name from a filenma, 
		/// i.e "Max Biaggi 001" from "max-biaggi-001.jpg"
		/// </summary>
		/// <param name="filename">The filename part of a path.</param>
		public static string GetFriendlyNameFromFilename(string filename)
		{
			// re-use the MediaPanther mode url simplifier code.
			filename = filename.Replace(" ", "-");
			var urlRegex = new Regex(@"[^\w-_]");
			filename = urlRegex.Replace(filename, string.Empty);
			filename = Regex.Replace(filename, "-{2,}", "-");
            filename = Regex.Replace(filename, "^-|-$", String.Empty);

			return Text.CapitaliseEachWord(filename.ToLower());
		}
    }
}