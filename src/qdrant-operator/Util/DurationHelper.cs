using System;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.VisualBasic;

namespace QdrantOperator.Util
{
    /// <summary>
    /// Duration helper methods.
    /// </summary>
    public static class DurationHelper
    {
        /// <summary>
        /// Parse a string duration.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TimeSpan ParseDuration(string duration)
        {
            var matches = Regex.Matches(duration, @"([0-9]*)((?:ms|s|m|h|d|w|y))");

            var result = TimeSpan.Zero;

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                var durationValue = int.Parse(match.Groups[1].Value);
                var unit          = (string)match.Groups[2].Value;

                switch (unit)
                {
                    case "ms":

                        result = result.Add(TimeSpan.FromMilliseconds(durationValue));

                        break;

                    case "s":

                        result = result.Add(TimeSpan.FromSeconds(durationValue));

                        break;

                    case "m":

                        result = result.Add(TimeSpan.FromMinutes(durationValue));

                        break;

                    case "h":

                        result = result.Add(TimeSpan.FromHours(durationValue));

                        break;

                    case "d":

                        result = result.Add(TimeSpan.FromDays(durationValue));

                        break;

                    case "w":

                        result = result.Add(TimeSpan.FromDays(durationValue * 7));

                        break;

                    case "y":

                        result = result.Add(TimeSpan.FromDays(durationValue * 365));

                        break;

                    default:

                        throw new ArgumentException("invalid argument");

                }
            }

            return result;
        }

        /// <summary>
        /// Convert a timespan to duration string.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static string ToDurationString(TimeSpan duration)
        {
            var result = new StringBuilder();

            var years = (int)(duration.TotalDays / 365);
            duration = duration.Subtract(TimeSpan.FromDays(365 * years));

            var weeks = (int)(duration.TotalDays / 7);
            duration = duration.Subtract(TimeSpan.FromDays(7 * weeks));

            if (years > 0)
            {
                result.Append(years);
                result.Append("y");
            }

            if (weeks > 0)
            {
                result.Append(weeks);
                result.Append("w");
            }

            if (duration.Days > 0)
            {
                result.Append(duration.Days);
                result.Append("d");
            }

            if (duration.Hours > 0)
            {
                result.Append(duration.Hours);
                result.Append("h");
            }

            if (duration.Minutes > 0)
            {
                result.Append(duration.Minutes);
                result.Append("m");
            }

            if (duration.Seconds > 0)
            {
                result.Append(duration.Seconds);
                result.Append("s");
            }

            if (duration.Milliseconds > 0)
            {
                result.Append(duration.Milliseconds);
                result.Append("ms");
            }

            return result.ToString();
        }
    }
}
