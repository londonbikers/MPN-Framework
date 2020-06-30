using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using MPN.Framework.Content;

namespace MPN.Framework.Web
{
    public class Web
    {
        #region enums
        /// <summary>
        /// Identifies an encoded string style, i.e. one that looks nicer or one that more accurately encodes characters so they
        /// </summary>
        public enum EncodedStringMode
        {
            /// <summary>
            /// Drops more non alpha-numeric characters to create a nicer looking string.
            /// </summary>
            Aggressive,
            /// <summary>
            /// Encodes more non alpha-numeric characters to preserve the content. Not as nice looking.
            /// </summary>
            Compliant
        }

		/// <summary>
		/// Denotes what type of url-encoding is necessary for a target website.
		/// </summary>
		public enum UrlEncodingType
		{
			Normal = 1,
			MediaPanther = 2,
			LB = 3
		}
        #endregion

		/// <summary>
		/// Attempts to identify whether or not the current user is a search-engine or other known bot.
		/// Won't raise false-positives but is not guaranteed to identify all bots.
		/// </summary>
		public static bool IsUserABot()
		{
		    if (HttpContext.Current.Request.UserAgent != null)
		    {
		        var userAgent = HttpContext.Current.Request.UserAgent.ToLower();
		        var botKeywords = new string[10] { "bot", "spider", "google", "yahoo", "search", "crawl", "slurp", "msn", "teoma", "ask.com" };
		        var n = botKeywords.Count(userAgent.Contains);
		        return (n > 0);
		    }

		    return false;
		}

        /// <summary>
        /// Returns the page segment of a URL.
        /// </summary>
        public static string PageNameFromUrl(string path)
        {
            return path.Substring((path.LastIndexOf("/") + 1));
        }

		/// <summary>
		/// Encodes a string in a chosen format so that it is URL-safe.
		/// </summary>
		public static string EncodeString(UrlEncodingType urlEncodingType, string stringToEncode)
		{
			return EncodeString(urlEncodingType, EncodedStringMode.Aggressive, stringToEncode);
		}

        /// <summary>
        /// Encodes a string in a chosen format so that it is URL-safe.
        /// </summary>
        public static string EncodeString(UrlEncodingType urlEncodingType, EncodedStringMode mode, string stringToEncode)
        {
            var output = String.Empty;
            switch (urlEncodingType)
            {
                case UrlEncodingType.Normal:
                    output = HttpUtility.UrlEncode(stringToEncode);
                    break;
                case UrlEncodingType.LB:
                    output = stringToEncode.Replace(" ", "_");
                    break;
                case UrlEncodingType.MediaPanther:
                    {
                        const string hyphen = "oo00HYP00oo";
                        if (mode == EncodedStringMode.Compliant)
                            stringToEncode = stringToEncode.Replace("-", hyphen);

                        stringToEncode = stringToEncode.Replace(" ", "-");
                        var urlRegex = new System.Text.RegularExpressions.Regex(@"[^\w-_]");
                        stringToEncode = urlRegex.Replace(stringToEncode, string.Empty);
                        stringToEncode = System.Text.RegularExpressions.Regex.Replace(stringToEncode, "-{2,}", "-");
                        stringToEncode = System.Text.RegularExpressions.Regex.Replace(stringToEncode, "^-|-$", String.Empty);

                        if (mode == EncodedStringMode.Compliant)
                            stringToEncode = stringToEncode.Replace(hyphen, "--");

                        output = stringToEncode.ToLower();
                    }
                    break;
            }

            return output;
        }

		/// <summary>
		/// Decodes a string from a chosen URL encoding format.
		/// </summary>
		public static string DecodeString(UrlEncodingType urlEncodingType, string stringToDecode)
		{
			var output = String.Empty;
			switch (urlEncodingType)
			{
			    case UrlEncodingType.Normal:
			        output = HttpUtility.UrlDecode(stringToDecode);
			        break;
			    case UrlEncodingType.LB:
			        output = stringToDecode.Replace("_", " ");
			        break;
			    case UrlEncodingType.MediaPanther:
			        stringToDecode = stringToDecode.Replace("--", "!!=!!");
			        stringToDecode = stringToDecode.Replace("-", " ");
			        stringToDecode = stringToDecode.Replace("!!=!!", "-");
			        stringToDecode = HttpUtility.UrlDecode(stringToDecode);
			        output = stringToDecode;
			        break;
			}

			return output;
		}

		/// <summary>
		/// Determines the next available numeric ID for a session container.
		/// </summary>
		/// <param name="containerType">The type of the session container.</param>
		public static int GetNextSessionContainerID(Type containerType)
		{
			var c = HttpContext.Current;
			var typeName = containerType.ToString();
			var id = 0;

		    if (c.Session == null)
                return id;

		    var id1 = id;
		    foreach (var tempId in from string key in c.Session.Keys
			                       where key.StartsWith(typeName + ":")
			                       select key.Split(char.Parse(":"))
			                       into parts select Convert.ToInt32(parts[1])
			                       into tempId where tempId > id1 select tempId)
			{
			    id = tempId;
			}

			return id + 1;
		}

        /// <summary>
        /// Ensures that a string will not break a html element if placed into a parameter. Useful for ensuring tooltips are safe.
        /// </summary>
        public static string ToSafeHtmlParameter(string textToMakeSafe)
        {
            return textToMakeSafe.Replace("\"", "'");
        }

		/// <summary>
		/// Fills the contents of a DropDownList with the names and values from an Enum.
		/// </summary>
		public static void PopulateDropDownFromEnum(DropDownList list, object enumToUse, bool useEnumValueAsItemValue)
		{
			if (!enumToUse.GetType().IsEnum)
				return;

			ListItem item;
			var values = Enum.GetValues(enumToUse.GetType());

			for (var i = 0; i < values.Length; i++)
			{
				item = new ListItem
				           {
				               Text = Text.SplitCamelCaseWords(Enum.GetName(enumToUse.GetType(), values.GetValue(i)))
				           };
			    item.Value = (useEnumValueAsItemValue) ? ((int)Enum.Parse(enumToUse.GetType(), values.GetValue(i).ToString())).ToString() : item.Text;
				list.Items.Add(item);
			}
		}
    }
}