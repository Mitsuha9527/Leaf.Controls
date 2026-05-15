using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CvCommon;
using Leaf.Controls.CustomControls;

namespace Leaf.Controls.Gallery.Pages;

public partial class ImageControlsPage : Page, INotifyPropertyChanged
{
	private const string EmbeddedImageUri = "pack://application:,,,/Leaf.Controls.Gallery;component/image.jpg";

	private BrushOption _selectedCodeTextBrushOption = null!;
	private bool _isCodeCenteredInRoi;
	private double _codeCenterFillRatioValue = 0.92;

	public BitmapFrame DemoImage { get; }

	public ObservableCollection<ROI> DemoRois { get; } = [];

	public IReadOnlyList<BrushOption> CodeTextBrushOptions { get; } =
	[
		new BrushOption("黄色", Brushes.Yellow),
		new BrushOption("白色", Brushes.White),
		new BrushOption("红色", Brushes.OrangeRed),
		new BrushOption("绿色", Brushes.LimeGreen),
		new BrushOption("青色", Brushes.Cyan),
	];

	public BrushOption SelectedCodeTextBrushOption
	{
		get => _selectedCodeTextBrushOption;
		set
		{
			if (SetField(ref _selectedCodeTextBrushOption, value))
			{
				OnPropertyChanged(nameof(CodeTextBrushValue));
			}
		}
	}

	public Brush CodeTextBrushValue => SelectedCodeTextBrushOption.Brush;

	public bool IsCodeCenteredInRoi
	{
		get => _isCodeCenteredInRoi;
		set => SetField(ref _isCodeCenteredInRoi, value);
	}

	public double CodeCenterFillRatioValue
	{
		get => _codeCenterFillRatioValue;
		set => SetField(ref _codeCenterFillRatioValue, value);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ImageControlsPage()
	{
		SelectedCodeTextBrushOption = CodeTextBrushOptions[0];
		DemoImage = LoadDemoImage() ?? BuildDemoImage(960, 600);
		InitializeComponent();
		DataContext = this;
		ResetRois();
	}

	private void OnResetRoiClick(object sender, RoutedEventArgs e)
	{
		ResetRois();
	}

	private void OnSwitchEditClick(object sender, RoutedEventArgs e)
	{
		WrapImageViewerDemo.Mode = InteractionMode.Edit;
	}

	private void OnSwitchDrawClick(object sender, RoutedEventArgs e)
	{
		WrapImageViewerDemo.Mode = InteractionMode.DrawROI;
	}

	private void OnSwitchPanClick(object sender, RoutedEventArgs e)
	{
		WrapImageViewerDemo.Mode = InteractionMode.Pan;
	}

	private void ResetRois()
	{
		DemoRois.Clear();
		DemoRois.Add(ROI.FromDisplayRect(new CvRect(80, 70, 180, 120), 1));
		DemoRois.Add(ROI.FromDisplayRect(new CvRect(320, 120, 240, 160), 1));
		DemoRois.Add(ROI.FromDisplayRect(new CvRect(620, 220, 180, 140), 1));

		DemoRois[0].Code = "ROI-01";
		DemoRois[0].ShowIndicator = true;
		DemoRois[0].IsOK = true;

		DemoRois[1].Code = "ROI-02";
		DemoRois[1].ShowIndicator = true;
		DemoRois[1].IsOK = false;

		DemoRois[2].Code = "ROI-03";
		DemoRois[2].ShowIndicator = false;
		DemoRois[2].IsOK = null;

		if (WrapImageViewerDemo is not null)
		{
			WrapImageViewerDemo.Mode = InteractionMode.DrawROI;
		}
	}

	private static BitmapFrame? LoadDemoImage()
	{
		try
		{
			BitmapImage image = new();
			image.BeginInit();
			image.UriSource = new Uri(EmbeddedImageUri, UriKind.Absolute);
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.EndInit();
			image.Freeze();
			return BitmapFrame.Create(image);
		}
		catch (FileNotFoundException)
		{
			return null;
		}
		catch (IOException)
		{
			return null;
		}
	}

	private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}

		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	private static BitmapFrame BuildDemoImage(int width, int height)
	{
		WriteableBitmap bitmap = new(width, height, 96, 96, PixelFormats.Bgra32, null);
		int stride = width * 4;
		byte[] pixels = new byte[stride * height];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int index = y * stride + (x * 4);
				byte r = (byte)((x * 255) / width);
				byte g = (byte)((y * 255) / height);
				byte b = (byte)(90 + ((x + y) % 120));

				if (x % 80 == 0 || y % 80 == 0)
				{
					r = 240;
					g = 240;
					b = 240;
				}

				pixels[index] = b;
				pixels[index + 1] = g;
				pixels[index + 2] = r;
				pixels[index + 3] = 255;
			}
		}

		bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
		bitmap.Freeze();
		return BitmapFrame.Create(bitmap);
	}
}

public sealed class BrushOption
{
	public BrushOption(string name, Brush brush)
	{
		Name = name;
		Brush = brush;
	}

	public string Name { get; }

	public Brush Brush { get; }
}

