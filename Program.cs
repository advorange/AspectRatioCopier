using System;

namespace FindImagesWithAspectRatio
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var copier = new AspectRatioCopier();

			Console.WriteLine("Enter an aspect ratio in one of the following formats: 'X:Y' or 'X/Y' or '.XXX'");
			copier.GetAspectRatio(Console.ReadLine());
			Console.WriteLine("Does the aspect ratio have to be exact? [Y/N]");
			copier.GetAbsoluteOrClose(Console.ReadLine());
			Console.WriteLine("Is there a minimum width required? Enter any negative number or zero for no.");
			copier.GetMinWidth(Console.ReadLine());
			Console.WriteLine("What is the source folder?");
			copier.GetSourceFolder(Console.ReadLine());
			Console.WriteLine("What is the output folder? Inputting a sub-directory from the source folder that doesn't exist yet will lead to its creation.");
			copier.GetDestinationFolder(Console.ReadLine());

			var imagePaths = copier.Search();
			copier.Copy(imagePaths);
		}
	}
}