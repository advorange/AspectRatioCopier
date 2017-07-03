using System;

namespace FindImagesWithAspectRatio
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var keepProgramOpen = true;
			while (keepProgramOpen)
			{
				var copier = new AspectRatioCopier();

				Console.WriteLine("Enter an aspect ratio in one of the following formats: 'X:Y' or 'X/Y' or '.XX'");
				while (true)
				{
					if (copier.SetAspectRatio(Console.ReadLine()))
						break;
				}
				Console.WriteLine("Does the aspect ratio have to be exact? [Y/N]");
				while (true)
				{
					if (copier.SetAbsoluteOrClose(Console.ReadLine()))
						break;
				}
				if (!copier.Absolute)
				{
					Console.WriteLine("Enter the percentage for how close an image's aspect ratio can be to the given aspect ratio.");
					while (true)
					{
						if (copier.SetPercentageCloseness(Console.ReadLine()))
							break;
					}
				}
				Console.WriteLine("Is there a minimum width required? Enter any negative number or zero for no.");
				while (true)
				{
					if (copier.SetMinWidth(Console.ReadLine()))
						break;
				}
				Console.WriteLine("What is the source directory?");
				while (true)
				{
					if (copier.SetSourceFolder(Console.ReadLine()))
						break;
				}
				Console.WriteLine("What is the output directory? Inputting a sub-directory from the source folder that doesn't exist yet will lead to its creation.");
				while (true)
				{
					if (copier.SetDestinationFolder(Console.ReadLine()))
						break;
				}

				copier.Copy(copier.Search());

				Console.WriteLine("Type zero or any positive number to rerun. Type any negative number to close the program.");
				while (true)
				{
					if (int.TryParse(Console.ReadLine(), out int answer))
					{
						keepProgramOpen = answer >= 0;
						break;
					}
					else
					{
						Console.WriteLine("Invalid answer provided.");
					}
				}
			}
		}
	}
}