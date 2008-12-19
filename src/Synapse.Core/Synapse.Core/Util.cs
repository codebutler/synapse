//
// Util.cs: Various utility methods.
//
// Copyright (C) 2008 Eric Butler
//
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;

namespace Synapse.Core
{
	public static class Util
	{
		public static string EscapeJavascript(string message)
		{
			StringBuilder builder = new StringBuilder();
			foreach (char c in message.ToCharArray()) {
				switch (c) {
				case '\\':
					builder.Append("\\\\");
					break;
				case '\"':
					builder.Append("\\\"");
					break;
				case '\n':
					builder.Append("");
					break;
				default:
					builder.Append(c);
					break;
				}
			}
			return builder.ToString();
		}

		public static string EscapeHtml (string text)
		{
			return text.Replace("<", "&lt;").Replace(">", "&gt;");
		}
		
		// I can't remember where this code came from, very sorry!
		public static string Strftime(string format, System.DateTime dt)
        {
            bool printFormat = false;
            System.Text.StringBuilder result = new System.Text.StringBuilder();

            foreach (char c in format)
            {
                if (!printFormat && c == '%')
                {
                    printFormat = true;
                    continue;
                }

                if (printFormat)
                {
                    switch (c)
                    {
                        case 'a':
                            result.Append(dt.ToString("ddd", CultureInfo.InvariantCulture));
                            break;
                        case 'A':
                            result.Append(dt.ToString("dddd", CultureInfo.InvariantCulture));
                            break;
                        case 'b':
                            result.Append(dt.ToString("MMM", CultureInfo.InvariantCulture));
                            break;
                        case 'B':
                            result.Append(dt.ToString("MMMM", CultureInfo.InvariantCulture));
                            break;
                        case 'c':
                            result.Append(dt.ToString(CultureInfo.InvariantCulture));
                            break;
                        case 'd':
                            result.Append(dt.ToString("dd", CultureInfo.InvariantCulture));
                            break;
                        case 'H':
                            result.Append(dt.ToString("HH", CultureInfo.InvariantCulture));
                            break;
                        case 'I':
                            result.Append(dt.ToString("hh", CultureInfo.InvariantCulture));
                            break;
                        case 'j':
                            string day = dt.DayOfYear.ToString(CultureInfo.InvariantCulture);
                            if (day.Length < 3)
                            {
                                if (day.Length == 1)
                                {
                                    day = "00" + day;
                                }
                                else
                                {
                                    day = "0" + day;
                                }
                            }
                            result.Append(day);
                            break;
                        case 'm':
                            result.Append(dt.ToString("MM", CultureInfo.InvariantCulture));
                            break;
                        case 'M':
                            result.Append(dt.ToString("mm", CultureInfo.InvariantCulture));
                            break;
                        case 'p':
                            result.Append(dt.ToString("tt", CultureInfo.InvariantCulture));
                            break;
                        case 'S':
                            result.Append(dt.ToString("ss", CultureInfo.InvariantCulture));
                            break;
                        case 'U':
                            {
                                System.Globalization.GregorianCalendar cal = new System.Globalization.GregorianCalendar();
                                int weekOfYear = cal.GetWeekOfYear(dt, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                                if (dt.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    weekOfYear--;
                                }
                                result.Append(weekOfYear.ToString("00", CultureInfo.InvariantCulture));
                            }
                            break;
                        case 'W':
                            {
                                System.Globalization.GregorianCalendar cal = new System.Globalization.GregorianCalendar();
                                int weekOfYear = cal.GetWeekOfYear(dt, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                                if (dt.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    weekOfYear--;
                                }
                                result.Append(weekOfYear.ToString("00", CultureInfo.InvariantCulture));
                            }
                            break;
                        case 'w':
                            result.Append(((int)dt.DayOfWeek).ToString(CultureInfo.InvariantCulture));
                            break;
                        case 'x':
                            result.Append(dt.ToString("d", CultureInfo.InvariantCulture));
                            break;
                        case 'X':
                            result.Append(dt.ToString("t", CultureInfo.InvariantCulture));
                            break;
                        case 'y':
                            result.Append(dt.ToString("yy", CultureInfo.InvariantCulture));
                            break;
                        case 'Y':
                            result.Append(dt.ToString("yyyy", CultureInfo.InvariantCulture));
                            break;
                        case 'Z':
                            {
                                TimeZone localZone = TimeZone.CurrentTimeZone;
                                result.Append(localZone.DaylightName);
                            }
                            break;
                        case '%':
                            result.Append('%');
                            break;
                        default:
                            //invalid format - do nothing
                            //C specification leaves this behaviour undefined
                            break;
                    }
                    printFormat = false;
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
		
		public static string JoinPath(params string[] pathParts)
		{
			if (pathParts.Length < 2) {
				throw new ArgumentException("pathParts must have at least two strings.");
			}
			string path = pathParts[0];
			for (int x = 1; x < pathParts.Length; x++) {
				path = System.IO.Path.Combine(path, pathParts[x]);
			}
			return path;
		}

		public static string SHA1(byte[] bytesIn)
		{
			SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
			byte[] bytesOut = sha1.ComputeHash(bytesIn);
			string strOut = BitConverter.ToString(bytesOut).Replace("-", String.Empty);
			return strOut.ToLower();
		}

		public static string ReadResource (string name)
		{
			var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(name);
			using (StreamReader reader = new StreamReader(stream)) {
				return reader.ReadToEnd();
			}
		}
	}
}