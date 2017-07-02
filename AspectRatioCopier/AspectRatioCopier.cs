using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static FindImagesWithAspectRatio.HelperFunctions;

namespace FindImagesWithAspectRatio
{
	public class AspectRatioCopier
	{
		private double _AspectRatio;
		private bool _Absolute;
		private double _PercentageCloseness;
		private double _UpperBound;
		private double _LowerBound;
		private bool _HasMinWidth;
		private int _MinWidth;
		private string _Source;
		private string _Destination;
		private string[] _ValidExtensions = new[] { ".jpg", ".jpeg", ".tiff", ".tif", ".bmp", ".png", ".webp" };

		public AspectRatioCopier()
		{
			//I think it looks neater to set the fields in here instead of where the fields are.
			_AspectRatio = 1.77;
			_Absolute = false;
			_PercentageCloseness = .05;
			_HasMinWidth = false;
			_MinWidth = 0;
			_Source = null;
			_Destination = null;
		}

		public void GetAspectRatio(string input)
		{
			if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double aspectRatio))
			{
			}
			else if (input.Contains(":") && GetAspectRatioFromInputNeedingSplitting(input, ':', out aspectRatio))
			{
			}
			else if (input.Contains("/") && GetAspectRatioFromInputNeedingSplitting(input, '/', out aspectRatio))
			{
			}

			if (aspectRatio > 0)
			{
				_AspectRatio = aspectRatio;
			}
			else
			{
				Console.WriteLine("Invalid aspect ratio provided.");
				GetAspectRatio(Console.ReadLine());
			}
		}
		public void GetAbsoluteOrClose(string input)
		{
			if (CaseInsEquals(input, "y") || CaseInsEquals(input, "yes"))
			{
				_Absolute = true;
			}
			else if (CaseInsEquals(input, "n") || CaseInsEquals(input, "no"))
			{
				_Absolute = false;

				Console.WriteLine("Enter the percentage for how close an image's aspect ratio can be to the given aspect ratio.");
				GetPercentageCloseness(Console.ReadLine());
			}
			else
			{
				Console.WriteLine("Invalid option provided.");
				GetAbsoluteOrClose(Console.ReadLine());
			}
		}
		public void GetPercentageCloseness(string input)
		{
			if (_Absolute)
			{
				return;
			}

			if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double percentageCloseness))
			{
			}
			else if (int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int closeness))
			{
				percentageCloseness = closeness / 100;
			}

			if (percentageCloseness > 0)
			{
				_PercentageCloseness = percentageCloseness;
				_UpperBound = _AspectRatio * (1.0 + _PercentageCloseness);
				_LowerBound = _AspectRatio * (1.0 - _PercentageCloseness);
			}
			else
			{
				Console.WriteLine("Invalid percentage closeness provided.");
				GetPercentageCloseness(Console.ReadLine());
			}
		}
		public void GetMinWidth(string input)
		{
			if (int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int minWidth))
			{
				if (minWidth > 0)
				{
					_MinWidth = minWidth;
					_HasMinWidth = true;
				}
				else
				{
					_HasMinWidth = false;
				}
			}
			else
			{
				Console.WriteLine("Invalid minimum width provided.");
				GetMinWidth(Console.ReadLine());
			}
		}
		public void GetSourceFolder(string input)
		{
			if (Directory.Exists(input))
			{
				_Source = input;
			}
			else
			{
				Console.WriteLine("Invalid source directory provided.");
				GetSourceFolder(Console.ReadLine());
			}
		}
		public void GetDestinationFolder(string input)
		{
			if (Directory.Exists(input))
			{
				_Destination = input;
			}
			else
			{
				try
				{
					var dirInfo = Directory.CreateDirectory(Path.Combine(_Source, input));
					_Destination = dirInfo.FullName;
				}
				catch (Exception e)
				{
					PrintOutException(e, "Unable to create the provided sub-directory inside the source directory. ");
					Console.WriteLine("Please enter a valid directory or sub-directory name.");
					GetSourceFolder(Console.ReadLine());
				}
			}
		}

		private bool GetAspectRatioFromInputNeedingSplitting(string input, char splitChar, out double aspectRatio)
		{
			var splitInput = input.Split(new[] { splitChar }, 2);
			if (splitInput.Length == 2 && int.TryParse(splitInput[0], out int leftSide) && int.TryParse(splitInput[1], out int rightSide))
			{
				aspectRatio = (leftSide * 1.0) / rightSide;
				return true;
			}
			else
			{
				aspectRatio = -1;
				return false;
			}
		}

		public IEnumerable<string> Search()
		{
			var withImageExtension = Directory.GetFiles(_Source).Where(x => CaseInsContains(_ValidExtensions, Path.GetExtension(x))).ToArray();
			Console.WriteLine(String.Format("Gathered {0} files with valid image extensions.", withImageExtension.Length));

			var matchingSearchCriteria = new List<string>();
			for (int i = 0; i < withImageExtension.Length; i++)
			{
				var path = withImageExtension[i];

				var width = 0;
				var height = 1;
				try
				{
					using (var stream = new MemoryStream(File.ReadAllBytes(path)))
					{
						/* I read from https://stackoverflow.com/questions/552467/how-do-i-reliably-get-an-image-dimensions-in-net-without-loading-the-image
						 * that using Image.FromStream instead of Image.FromFile avoids loading the image into memory and that sounds better to do.
						 */
						var image = Image.FromStream(stream, false, false);

						width = image.Width;
						height = image.Height;
					}
				}
				catch (Exception e)
				{
					PrintOutException(e);
					continue;
				}

				if (_HasMinWidth && width < _MinWidth)
				{
					continue;
				}

				var aspectRatio = (width * 1.0) / height;
				if (_Absolute)
				{
					if (_AspectRatio == aspectRatio)
					{
						Console.WriteLine(String.Format("Another image match found. {0}/{1} images looked through.", i + 1, withImageExtension.Length));
						matchingSearchCriteria.Add(path);
						continue;
					}
				}
				else
				{
					if (_UpperBound >= aspectRatio && aspectRatio >= _LowerBound)
					{
						Console.WriteLine(String.Format("Another image match found. {0}/{1} images looked through.", i + 1, withImageExtension.Length));
						matchingSearchCriteria.Add(path);
						continue;
					}
				}
			}

			var success = matchingSearchCriteria.Count();
			var failure = withImageExtension.Count() - success;
			Console.WriteLine(String.Format("{0} files matched the provided search criteria. {1} files did not match the provided search criteria.", success, failure));

			return matchingSearchCriteria;
		}
		public void Copy(IEnumerable<string> imagePaths)
		{
			var failures = 0;
			foreach (var path in imagePaths)
			{
				var nameAndExtension = Path.GetFileName(path);
				var newPath = Path.Combine(_Destination, nameAndExtension);
				try
				{
					File.Copy(path, newPath, true);
				}
				catch (Exception e)
				{
					PrintOutException(e);
					failures++;
				}
			}

			Console.WriteLine(String.Format("Successfully copied {0} files. Failed to copy {1} files.", imagePaths.Count() - failures, failures));
		}

		public override string ToString()
		{
			//TODO: Format this
			return base.ToString();
		}
	}
}
