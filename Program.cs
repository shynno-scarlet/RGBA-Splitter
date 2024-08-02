using System.Drawing.Imaging;

namespace RGBA_Splitter;

/// <summary>
/// a program to extract a multi-channel texture into the 4 single channel maps
/// </summary>
internal static class Program
{
	/// <summary>
	/// rgba channels
	/// </summary>
	enum RGBA
	{
		R, G, B, A,
	}

	/// <summary>
	/// supported file formats
	/// </summary>
	static readonly string[] fileFormats = [
		"bmp",
		"png",
		"jpg",
		"jpeg",
		"webp",
		"gif",
		"tif",
		"tiff",
		"emf",
		"wmf",
		"exif",
		"heif",
		"ico"
	];

	/// <summary>
	/// program entry point
	/// </summary>
	[STAThread]
	static void Main()
	{
		// select files to convert
		var formats = string.Join(";", fileFormats.Select(s => $"*.{s}"));
		var open = new OpenFileDialog
		{
			Title = "Select Image Files",
			Filter = $"Image-Files ({formats})|{formats}",
			Multiselect = true,
		};

		// exit if no files are selected
		if (open.ShowDialog() != DialogResult.OK || open.FileNames.Length == 0) return;

		// iterate over every file
		foreach (var file in open.FileNames)
		{
			var fi = new FileInfo(file);

			// create output dir
			var noExt = fi.Name.Replace(fi.Extension, string.Empty);
			var dir = @$"{fi.Directory!.FullName}\{noExt}";
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			// extract all channels from bitmap and save to files
			var bmp = new Bitmap(Image.FromFile(file, true));
			foreach (RGBA rgba in Enum.GetValues<RGBA>())
			{
				bmp.ExtractChannel(rgba).Save($@"{dir}\{noExt}_{rgba}.png", ImageFormat.Png);
			}
		}
	}

	/// <summary>
	/// converts the bitmap into a single channel bitmap
	/// </summary>
	/// <param name="bmp"></param>
	/// <param name="rgba"></param>
	/// <returns></returns>
	static Bitmap ExtractChannel(this Bitmap bmp, RGBA rgba)
	{
		var copy = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
		for (var x = 0; x < copy.Width; x++)
		{
			for (var y = 0; y < copy.Height; y++)
			{
				var px = copy.GetPixel(x, y);
				px = px.ExtractChannel(rgba);
				copy.SetPixel(x, y, px);
			}
		}
		return copy;
	}

	/// <summary>
	/// converts the color into a single channel (grayscale) color
	/// </summary>
	/// <param name="color"></param>
	/// <param name="rgba"></param>
	/// <returns></returns>
	static Color ExtractChannel(this Color color, RGBA rgba)
	{
		var ch = rgba switch
		{
			RGBA.R => color.R,
			RGBA.G => color.G,
			RGBA.B => color.B,
			_ => color.A,
		};
		return Color.FromArgb(255, ch, ch, ch);
	}
}