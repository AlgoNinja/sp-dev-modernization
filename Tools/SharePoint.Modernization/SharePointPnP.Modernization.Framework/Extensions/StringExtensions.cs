﻿using System;
using System.Linq;
using System.Text;

namespace SharePointPnP.Modernization.Framework.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Determines if a string exists in another string regardless of casing
        /// </summary>
        /// <param name="value">original string</param>
        /// <param name="comparedWith">string to compare with</param>
        /// <param name="stringComparison">optional comparison mode</param>
        /// <returns></returns>
        public static bool ContainsIgnoringCasing(this string value, string comparedWith, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return value.IndexOf(comparedWith, stringComparison) >= 0;
        }

        /// <summary>
        /// Prepends string to another including null checking
        /// </summary>
        /// <param name="value"></param>
        /// <param name="prependString"></param>
        /// <returns></returns>
        public static string PrependIfNotNull(this string value, string prependString)
        {
            if (!string.IsNullOrEmpty(value) && !value.ContainsIgnoringCasing(prependString))
            {
                value = prependString + value;
            }

            return value; // Fall back
        }


        /// <summary>
        /// Removes a relative section of by string where context not available
        /// </summary>
        /// <param name="value"></param>
        /// <param name="seperator"></param>
        /// <param name="instanceFrom"></param>
        /// <returns></returns>
        public static string StripRelativeUrlSectionString(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var sitesColl = "/sites/";
                var teamsColl = "/teams/";
                var containsSites = value.IndexOf(sitesColl, StringComparison.InvariantCultureIgnoreCase);
                var containsTeams = value.IndexOf(teamsColl, StringComparison.InvariantCultureIgnoreCase);
                if (containsSites > -1 || containsTeams > -1)
                {
                    if (containsSites > -1)
                    {
                        var result = value.TrimStart(sitesColl.ToCharArray());
                        if (result.IndexOf('/') > -1)
                        {
                            return result.Substring(result.IndexOf('/'));
                        }
                    }
                    else if (containsTeams > -1)
                    {
                        var result = value.TrimStart(teamsColl.ToCharArray());
                        if (result.IndexOf('/') > -1)
                        {
                            return result.Substring(result.IndexOf('/'));
                        }
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Gets base url from string
        /// </summary>
        /// <param name="sourceSite"></param>
        /// <returns></returns>
        public static string GetBaseUrl(this string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(url) && (url.ContainsIgnoringCasing("https://") || url.ContainsIgnoringCasing("http://")))
                {
                    Uri siteUri = new Uri(url);
                    string host = $"{siteUri.Scheme}://{siteUri.DnsSafeHost}";
                    return host;
                }
            }
            catch (Exception)
            {
                //Swallow
            }

            return string.Empty;
        }

        /// <summary>
        /// Get type in short form
        /// </summary>
        /// <param name="typeValue"></param>
        /// <returns></returns>
        public static string GetTypeShort(this string typeValue)
        {
            string name = typeValue;
            var typeSplit = typeValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (typeSplit.Length > 0)
            {
                name = typeSplit[0];
            }

            return $"{name}";
        }

        /// <summary>
        /// Gets classname from type
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static string InferClassNameFromNameSpace(this string typeName)
        {
            string shortType = typeName;
            string className = string.Empty;
            if (typeName.Contains(","))
            {
                shortType = typeName.GetTypeShort();
            }

            var typeShortSplit = shortType.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (typeShortSplit.Length > 0)
            {
                className = typeShortSplit.Last();
            }

            return className;
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another 
        /// specified string according the type of search to use for the specified string.
        /// Copied from https://stackoverflow.com/questions/6275980/string-replace-ignoring-case
        /// </summary>
        /// <param name="str">The string performing the replace method.</param>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string replace all occurrences of <paramref name="oldValue"/>. 
        /// If value is equal to <c>null</c>, than all occurrences of <paramref name="oldValue"/> will be removed from the <paramref name="str"/>.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue"/> are replaced with <paramref name="newValue"/>. 
        /// If <paramref name="oldValue"/> is not found in the current instance, the method returns the current instance unchanged.</returns>
        public static string Replace(this string str, string oldValue, string @newValue, StringComparison comparisonType)
        {

            // Check inputs.
            if (str == null)
            {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentNullException(nameof(str));
            }
            if (str.Length == 0)
            {
                // Same as original .NET C# string.Replace behavior.
                return str;
            }
            if (oldValue == null)
            {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentNullException(nameof(oldValue));
            }
            if (oldValue.Length == 0)
            {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentException("String cannot be of zero length.");
            }

            // Prepare string builder for storing the processed string.
            // Note: StringBuilder has a better performance than String by 30-40%.
            StringBuilder resultStringBuilder = new StringBuilder(str.Length);

            // Analyze the replacement: replace or remove.
            bool isReplacementNullOrEmpty = string.IsNullOrEmpty(@newValue);

            // Replace all values.
            const int valueNotFound = -1;
            int foundAt;
            int startSearchFromIndex = 0;
            while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound)
            {

                // Append all characters until the found replacement.
                int @charsUntilReplacment = foundAt - startSearchFromIndex;
                bool isNothingToAppend = @charsUntilReplacment == 0;
                if (!isNothingToAppend)
                {
                    resultStringBuilder.Append(str, startSearchFromIndex, @charsUntilReplacment);
                }

                // Process the replacement.
                if (!isReplacementNullOrEmpty)
                {
                    resultStringBuilder.Append(@newValue);
                }

                // Prepare start index for the next search.
                // This needed to prevent infinite loop, otherwise method always start search 
                // from the start of the string. For example: if an oldValue == "EXAMPLE", newValue == "example"
                // and comparisonType == "any ignore case" will conquer to replacing:
                // "EXAMPLE" to "example" to "example" to "example" … infinite loop.
                startSearchFromIndex = foundAt + oldValue.Length;
                if (startSearchFromIndex == str.Length)
                {
                    // It is end of the input string: no more space for the next search.
                    // The input string ends with a value that has already been replaced. 
                    // Therefore, the string builder with the result is complete and no further action is required.
                    return resultStringBuilder.ToString();
                }
            }

            // Append the last part to the result.
            int @charsUntilStringEnd = str.Length - startSearchFromIndex;
            resultStringBuilder.Append(str, startSearchFromIndex, @charsUntilStringEnd);

            return resultStringBuilder.ToString();
        }
    }
}
