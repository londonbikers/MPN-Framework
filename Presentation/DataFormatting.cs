using System;

namespace MPN.Framework.Presentation
{
    public class DataFormatting
    {
        #region motorsport
        /// <summary>
        /// Converts a time-span into a presentable lap-time string. e.g. 1'27.603
        /// </summary>
        public static string TimeSpanToLapTime(TimeSpan time, bool markAsEmpty)
        {
            if (time == new TimeSpan())
                return (markAsEmpty) ? "-" : String.Empty;

            var ms = (time.Milliseconds < 100) ? "0" + time.Milliseconds : time.Milliseconds.ToString();
            return string.Format("{0}'{1}.{2}", time.Minutes, time.Seconds, ms);
        }

        /// <summary>
        /// Formats a speed into a full speed statement, i.e. "307.048" into "307.05 MPH".
        /// </summary>
        /// <param name="speed">The speed as a double.</param>
        /// <param name="suffix">Any suffix, i.e. speed metric to append to the end if there's a valid speed.</param>
        /// <returns>"304 MPH" or "-" if no value.</returns>
        public static string FormatSpeed(double speed, string suffix)
        {
            string result;
            if (speed > 0)
            {
                result = speed.ToString("###.##");
                if (suffix != String.Empty)
                    result += " " + suffix;
            }
            else
            {
                result = "-";
            }

            return result;
        }

        public static string FormatInt(int number, bool markAsEmpty)
        {
            if (number > 0)
                return number.ToString();
            return (markAsEmpty) ? "-" : String.Empty;
        }
        #endregion
    }
}