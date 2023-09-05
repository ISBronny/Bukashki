using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Bukashki.Domain.Models;
using Point = System.Windows.Point;

namespace Bukashki
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private AntManager _antManager;
		public object _mutext = new object();

		public MainWindow()
		{
			InitializeComponent();

			_antManager = new AntManager(new Rectangle(0, 0, (int)Image.Width, (int)Image.Height))
			{
			};

			DispatcherTimer t = new DispatcherTimer();
			t.Tick += t_Tick;
			t.Interval = new TimeSpan(0, 0, 0, 0, 20);
			t.Start();

		}

		GeometryGroup antsEllipses;

		public void t_Tick(object? sender, EventArgs e)
		{
			_antManager.Step();


			antsEllipses = new GeometryGroup();

			if (!antsEllipses.Children.Any())
				foreach (var ant in _antManager.Ants)
					antsEllipses.Children.Add(new EllipseGeometry(new Point(ant.Point.X, ant.Point.Y), 1, 1));
			else
				for (int i = 0; i <= _antManager.Ants.Count; i++)
					((EllipseGeometry)antsEllipses.Children[i]).Center =
						new Point(_antManager.Ants[i].Point.X, _antManager.Ants[i].Point.Y);


			GeometryDrawing ants = new GeometryDrawing();
			ants.Geometry = antsEllipses;
			ants.Brush = Brushes.Azure;

			GeometryGroup homeEllipses = new GeometryGroup();
			foreach (var destination in _antManager.Destinations.OfType<Home>())
			{
				homeEllipses.Children.Add(
					new EllipseGeometry(new Point(destination.Point.X, destination.Point.Y), 20, 20));
			}


			GeometryGroup resEllipses = new GeometryGroup();
			foreach (var destination in _antManager.Destinations.OfType<Resource>())
			{
				resEllipses.Children.Add(
					new EllipseGeometry(new Point(destination.Point.X, destination.Point.Y), 20, 20));
			}


			GeometryDrawing home = new GeometryDrawing();
			home.Geometry = homeEllipses;
			home.Brush = Brushes.Yellow;

			GeometryDrawing res = new GeometryDrawing();
			res.Geometry = resEllipses;
			res.Brush = Brushes.DodgerBlue;



			var linesHome = new GeometryGroup();
			var linesRes = new GeometryGroup();


			foreach (var ant in _antManager.Ants)
			{
				var m = ant.ReceivedMessage;
				if (m is null)
					continue;
				var line = new LineGeometry(new Point(ant.Point.X, ant.Point.Y),
					new Point(m.Value.Sender.Point.X, m.Value.Sender.Point.Y));
				if (m.Value.Destination is Home)
					linesHome.Children.Add(line);
				else
					linesRes.Children.Add(line);

			}


			/*GeometryDrawing homelinksSolid = new GeometryDrawing();
			homelinksSolid.Geometry = linesHome;
			homelinksSolid.Brush = Brushes.Yellow;

			GeometryDrawing reslinksSolid = new GeometryDrawing();
			reslinksSolid.Geometry = linesRes;
			reslinksSolid.Brush = Brushes.DodgerBlue;*/



			var collection = new DrawingCollection()
			{
				ants,

				//homelinksSolid,
				//reslinksSolid,

				new GeometryDrawing()
				{
					Geometry = new RectangleGeometry(new Rect(-15, -15, Image.Width + 15, Image.Height + 15))
					{

					},
					Brush = Brushes.Transparent
				}

			};

			foreach (var line in linesHome.Children)
			{
				var color = System.Windows.Media.Color.FromRgb(245, 218, 66);

				GeometryDrawing glowHome = new GeometryDrawing();
				glowHome.Geometry = line;
				glowHome.Pen = new Pen(new SolidColorBrush(color)
				{
					Opacity = 1,
				}, 1);
				
				

				GeometryDrawing glowHome2 = new GeometryDrawing();
				glowHome2.Geometry = line;
				glowHome2.Pen = new Pen(new SolidColorBrush(color)
				{
					Opacity = 0.2
				}, 10)
				{
					EndLineCap = PenLineCap.Round,
					StartLineCap = PenLineCap.Round
				};
				
				
				GeometryDrawing glowHome3 = new GeometryDrawing();
				glowHome3.Geometry = line;
				glowHome3.Pen = new Pen(new SolidColorBrush(color)
				{
					Opacity = 0.1
				}, 20){
					EndLineCap = PenLineCap.Round,
					StartLineCap = PenLineCap.Round
				};

				collection.Add(glowHome3);
				collection.Add(glowHome2);
				collection.Add(glowHome);

			}

			foreach (var line in linesRes.Children)
			{
				var color = System.Windows.Media.Color.FromRgb(0, 96, 191);

				GeometryDrawing glowRes = new GeometryDrawing();
				glowRes.Geometry = line;
				glowRes.Pen = new Pen(new SolidColorBrush(color)
				{
					Opacity = 1
				}, 1);
				
				GeometryDrawing glowRes2 = new GeometryDrawing();
				glowRes2.Geometry = line;
				glowRes2.Pen = new Pen(new SolidColorBrush(color)
				{
					Opacity = 0.2
				}, 10){
					EndLineCap = PenLineCap.Round,
					StartLineCap = PenLineCap.Round
				};
				
				
				GeometryDrawing glowRes3 = new GeometryDrawing();
				glowRes3.Geometry = line;
				glowRes3.Pen = new Pen(new SolidColorBrush(color)
				{
					Opacity = 0.1
				}, 20){
					EndLineCap = PenLineCap.Round,
					StartLineCap = PenLineCap.Round
				};
				
				
				collection.Add(glowRes3);
				collection.Add(glowRes2);
				collection.Add(glowRes);

			}

			collection.Add(res);
			collection.Add(home);



			DrawingImage geometryImage = new DrawingImage()
			{
				Drawing = new DrawingGroup()
				{
					Children = collection,
				}
			};


			// Freeze the DrawingImage for performance benefits.
			geometryImage.Freeze();


			lock (_mutext)
			{
				Image.Source = geometryImage;
				Image.Stretch = Stretch.None;
				Image.HorizontalAlignment = HorizontalAlignment.Center;
				Image.VerticalAlignment = VerticalAlignment.Center;
			}


		}

		private void Image_OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
		}
	}
}