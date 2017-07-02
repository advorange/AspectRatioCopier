using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FindImagesWithAspectRatio
{
	public class HelperFunctions
	{
		//Copied from my other project Advobot
		public static bool CaseInsEquals(string str1, string str2)
		{
			if (str1 == null)
			{
				return str2 == null;
			}
			if (str2 == null)
			{
				return false;
			}
			else
			{
				return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
			}
		}

		public static bool CaseInsIndexOf(string source, string search)
		{
			if (source == null || search == null)
			{
				return false;
			}
			else
			{
				return source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
			}
		}

		public static bool CaseInsIndexOf(string source, string search, out int position)
		{
			position = -1;
			if (source == null || search == null)
			{
				return false;
			}
			else
			{
				return (position = source.IndexOf(search, StringComparison.OrdinalIgnoreCase)) >= 0;
			}
		}

		public static bool CaseInsStartsWith(string source, string search)
		{
			if (source == null || search == null)
			{
				return false;
			}
			else
			{
				return source.StartsWith(search, StringComparison.OrdinalIgnoreCase);
			}
		}

		public static bool CaseInsContains(IEnumerable<string> enumerable, string search)
		{
			if (!enumerable.Any())
			{
				return false;
			}
			else
			{
				return enumerable.Contains(search, StringComparer.OrdinalIgnoreCase);
			}
		}
	}
}
