using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
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
		private readonly string[] _ValidExtensions = new[] { ".jpg", ".jpeg", ".tiff", ".tif", ".bmp", ".png", ".webp" };

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

		//Should be able to easily hook up a UI to this now that these are bools. Too lazy to do that though.
		public bool SetAspectRatio(string input)
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
				return true;
			}
			else
			{
				Console.WriteLine("Invalid aspect ratio provided.");
				return false;
			}
		}
		public bool SetAbsoluteOrClose(string input)
		{
			if (CaseInsEquals(input, "y") || CaseInsEquals(input, "yes"))
			{
				_Absolute = true;
				return true;
			}
			else if (CaseInsEquals(input, "n") || CaseInsEquals(input, "no"))
			{
				_Absolute = false;
				return true;
			}
			else
			{
				Console.WriteLine("Invalid option provided.");
				return false;
			}
		}
		public bool SetPercentageCloseness(string input)
		{
			if (_Absolute)
			{
				return true;
			}

			double percentageCloseness = 0;
			if (int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int closeness))
			{
				percentageCloseness = closeness / 100.0;
			}
			else if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out percentageCloseness))
			{
			}

			if (percentageCloseness > 0)
			{
				_PercentageCloseness = percentageCloseness;
				_UpperBound = _AspectRatio * (1.0 + _PercentageCloseness);
				_LowerBound = _AspectRatio * (1.0 - _PercentageCloseness);
				return true;
			}
			else
			{
				Console.WriteLine("Invalid percentage closeness provided.");
				return false;
			}
		}
		public bool SetMinWidth(string input)
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
				return true;
			}
			else
			{
				Console.WriteLine("Invalid minimum width provided.");
				return false;
			}
		}
		public bool SetSourceFolder(string input)
		{
			if (Directory.Exists(input))
			{
				_Source = input;
				return true;
			}
			else
			{
				Console.WriteLine("Invalid source directory provided.");
				return false;
			}
		}
		public bool SetDestinationFolder(string input)
		{
			if (Directory.Exists(input))
			{
				_Destination = input;
				return true;
			}
			else
			{
				try
				{
					var dirInfo = Directory.CreateDirectory(Path.Combine(_Source, input));
					_Destination = dirInfo.FullName;
					return true;
				}
				catch (Exception e)
				{
					PrintOutException(e, "Unable to create the provided sub-directory inside the source directory. ");
					Console.WriteLine("Please enter a valid directory or sub-directory name.");
					return false;
				}
			}
		}

		public double AspectRatio
		{
			get { return _AspectRatio; }
			set { _AspectRatio = value; }
		}
		public bool Absolute
		{
			get { return _Absolute; }
			set { _Absolute = value; }
		}
		public double PercentageCloseness
		{
			get { return _PercentageCloseness; }
			set { _PercentageCloseness = value; }
		}
		public double UpperBound
		{
			get { return _UpperBound; }
			set { _UpperBound = value; }
		}
		public double LowerBound
		{
			get { return _LowerBound; }
			set { _LowerBound = value; }
		}
		public bool HasMinWidth
		{
			get { return _HasMinWidth; }
			set { _HasMinWidth = value; }
		}
		public int MinWidth
		{
			get { return _MinWidth; }
			set { _MinWidth = value; }
		}
		public string Source
		{
			get { return _Source; }
			set { _Source = value; }
		}
		public string Destination
		{
			get { return _Destination; }
			set { _Destination = value; }
		}

		private bool GetAspectRatioFromInputNeedingSplitting(string input, char splitChar, out double aspectRatio)
		{
			var splitInput = input.Split(new[] { splitChar }, 2);
			if (splitInput.Length == 2 && int.TryParse(splitInput[0], out int leftSide) && int.TryParse(splitInput[1], out int rightSide))
			{
				aspectRatio = (double)leftSide / rightSide;
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

				var aspectRatio = (double)width / height;
				if (_Absolute)
				{
					if (_AspectRatio == aspectRatio)
					{
						Console.WriteLine(String.Format("Image match found. {0}/{1} images looked through.", i + 1, withImageExtension.Length));
						matchingSearchCriteria.Add(path);
						continue;
					}
				}
				else
				{
					if (_UpperBound >= aspectRatio && aspectRatio >= _LowerBound)
					{
						Console.WriteLine(String.Format("Image match found. {0}/{1} images looked through.", i + 1, withImageExtension.Length));
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
	}
}
