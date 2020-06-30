using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace MPN.Framework.Content
{
    public class RegularExpressions
    {
        /// <summary>
        /// Finds all instances of image url's in a piece of text. ie. http://domain.com/image.jpg. Finds jpg's, png's, gif's and 'bmps.
        /// </summary>
        public static ArrayList FindAllImageUrls(string source)
        {
            // now, because I can't make a pattern for both cases that match properly we're doing it in two runs.
            var list = new ArrayList();
            
            // free-form url's.
            var pattern = @"(?:(?:(?:http|ftp|gopher|telnet|news)://)(?:w{3}\.)?(?:[a-zA-Z0-9/;\?&=:\-_\$\+!\*'\(\|\\~\[\]#%\.])+)(\.jpg|\.jpeg|\.png|\.gif|\.bmp)";
            foreach (Match match in System.Text.RegularExpressions.Regex.Matches(source, pattern, RegexOptions.IgnoreCase))
                list.Add(match.Value);

            // img src="" matches.
            pattern = "< *[img][^\\>]*[src] *= *[\"\']{0,1}([^\"\'\\ >]*)";
            var matches = System.Text.RegularExpressions.Regex.Matches(source, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                foreach (Group group in match.Groups)
                {
                    if (Uri.IsWellFormedUriString(group.Value, UriKind.Absolute) && !list.Contains(group.Value))
                        list.Add(group.Value);
                }
            }

            return list;
        }

        /// <summary>
        /// Removes any non alpha-numeric characters from a string.
        /// </summary>
        public static string RemoveNonAlphaNumericCharacters(string source)
        {
            return Regex.Replace(source, "[^a-zA-Z0-9]", String.Empty);
        }
    }
}