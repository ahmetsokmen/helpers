using Toolbox.Exceptions;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;

namespace Toolbox.Helpers
{
    public static class StaticHelpers
    {

        public static string RemoveSpecialCharacters(this string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9]+", ".", RegexOptions.Compiled
                                                            | RegexOptions.CultureInvariant
                                                            | RegexOptions.ExplicitCapture
                                                            | RegexOptions.IgnoreCase
                                                            | RegexOptions.IgnorePatternWhitespace);
        }

        public static string CreateLanguageKey(this string str)
        {
            return "i18n-" + str.Trim().RemoveSpecialCharacters().ToLower();
        }
        public static string SetUrlParameter(this string url, string paramName, string value)
        {
            return new Uri(url).SetParameter(paramName, value).ToString();
        }

        public static Uri SetParameter(this Uri url, string paramName, string value)
        {
            if (!url.ToString().Contains(paramName)) throw new CustomBaseException(CustomException.URL_PARAMETER_ERROR);
            var queryParts = HttpUtility.ParseQueryString(url.Query);   
            queryParts[paramName] = value;
            return new Uri(url.AbsoluteUriExcludingQuery() + '?' + queryParts.ToString());
        }

        public static string AbsoluteUriExcludingQuery(this Uri url)
        {
            return url.AbsoluteUri.Split('?').FirstOrDefault() ?? String.Empty;
        }

        public static string GetDirection(Type type)
        {
            var path = type.GetType().Name;
            var method = GetActualAsyncMethodName();
            return path + "." + method;
        }

        public static int NthIndexOf(this string target, string value, int n)
        {
            Match m = Regex.Match(target, "((" + Regex.Escape(value) + ").*?){" + n + "}");

            if (m.Success)
                return m.Groups[2].Captures[n].Index;
            else
                return -1;
        }

        public static int IndexOfOccurence(this string s, string match, int occurence)
        {
            int i = 1;
            int index = 0;

            while (i <= occurence && (index = s.IndexOf(match, index)) != -1)
            {
                if (i == occurence)
                    return index;

                i++;
            }

            return -1;
        }


        public static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;
        public static string GetEnumDisplay(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()
                            .GetName();
        }

        public static string JsonStringEncode(this string text)
        {
            return text.Replace("\r", "").Replace("\n", "");
        }

        public static TimeSpan StripMiliSeconds(this TimeSpan time)
        {
            return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        }

        public static bool IsBetweenRange(int value, int minValue, int maxValue)
        {
            if (value >= minValue && value <= maxValue)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public static string FormatName(this string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            var trimmed = str.Trim();

            var withSpace = char.ToUpper(trimmed[0]) + trimmed.Substring(1).ToLower();

            if (withSpace.Contains(" "))
            {
                if (withSpace.Count(x => x == ' ') > 1)
                {

                    throw new InvalidOperationException("Maksimum iki isim girebilirsiniz.");
                }

                var spaceIndex = withSpace.IndexOf(" ");

                withSpace = withSpace.Substring(0, spaceIndex + 1) + withSpace.Substring(spaceIndex + 1, 1).ToUpper() + withSpace.Substring(spaceIndex + 2).ToLower();
            }




            return withSpace;
        }

        public static string ToFirstLetterUpper(this string text)
        {
            var withSpace = char.ToUpper(text[0]) + text.Substring(1).ToLower();

            if (withSpace.Contains(" "))
            {
                var spaceIndex = withSpace.IndexOf(" ");
                withSpace = withSpace.Substring(0, spaceIndex + 1) + withSpace.Substring(spaceIndex + 1, 1).ToUpper() + withSpace.Substring(spaceIndex + 2).ToLower();
            }

            return withSpace;
        }

        public static string ReplaceTurkishChars(this string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            return text.Replace("ş", "s").Replace("ö", "o").Replace("ı", "i").Replace("ğ", "g").Replace("ç", "c").Replace("ü", "u");

        }

        public static string Substring(this string text, int maxValue)
        {
            if (!string.IsNullOrEmpty(text))
            {
                return text.Length > maxValue ? text.Substring(0, maxValue) + "..." : text;
            }

            else
            {
                return string.Empty;
            }

        }

        public static List<string> GetChangedProperties<T>(object A, object B)
        {
            if (A != null && B != null)
            {
                var type = typeof(T);
                var allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var allSimpleProperties = allProperties.Where(pi => pi.PropertyType.IsSimpleType());
                var unequalProperties =
                       from pi in allSimpleProperties
                       let AValue = type.GetProperty(pi.Name).GetValue(A, null)
                       let BValue = type.GetProperty(pi.Name).GetValue(B, null)
                       where AValue != BValue && (AValue == null || !AValue.Equals(BValue))
                       select pi.Name;
                return unequalProperties.ToList();
            }
            else
            {
                throw new ArgumentNullException("You need to provide 2 non-null objects");
            }
        }

        public static bool IsSimpleType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return type.GetGenericArguments()[0].IsSimpleType();
            }
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }
        public static bool IsMember<T>(this T item, List<T> list)
        {
            if (list.Contains(item))
            {
                return true;
            }

            else
            {
                return false;
            }

        }

        public static int Replace<T>(this IList<T> source, T oldValue, T newValue)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var index = source.IndexOf(oldValue);
            if (index != -1)
                source[index] = newValue;
            return index;
        }

        public static void ReplaceAll<T>(this List<T> source, T oldValue, T newValue)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int index = -1;
            do
            {
                index = source.IndexOf(oldValue);
                if (index != -1)
                    source[index] = newValue;
            } while (index != -1);
        }

        public static bool HasChanges(List<KeyValuePair<object, object>> all)
        {
            bool hasChanges = false;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Key.ToString() != all[i].Value.ToString())
                {
                    hasChanges = true;
                    break;
                }
            }

            return hasChanges;
        }

        public static bool HasNull(params object[] all)
        {
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null)
                {
                    return true;
                }

                else
                {
                    continue;
                }
            }

            return false;

        }
        public static bool RfidRegexControl(string _trayRfid)
        {
            Regex _regex = new Regex("[A-Z0-9]{" + _trayRfid.Length + "}");
            Match _match = _regex.Match(_trayRfid);
            if (_match.Success)
            {
                return true;
            }
            return false;
        }
    }
}
