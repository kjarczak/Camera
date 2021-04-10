using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Camera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double _canvasHeight;
        private double _canvasWidth;

        private readonly Scene _scene;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly short[] _motion = {0,0,0};
        private readonly short[] _rotation = { 0, 0, 0 };
        private short _zoom = 0;
        public MainWindow()
        {
            InitializeComponent();
            Canvas.Focus();
            _scene = new Scene();
            _scene.LoadScene();
            _timer.Tick += TimerEvent;
            _timer.Interval = TimeSpan.FromMilliseconds(20);
            _timer.Start();

            _canvasHeight = Canvas.Height;
            _canvasWidth = Canvas.Width;
        }

        private void TimerEvent(object sender, EventArgs e)
        {
            if (_zoom != 0)
            {
                _scene.AddZoom(_zoom);
            }

            if (_rotation[0]!= 0)
            {
                _scene.AddRotationX(_rotation[0]);
            }

            if (_rotation[1] != 0)
            {
                _scene.AddRotationY(_rotation[1]);
            }

            if (_rotation[2] != 0)
            {
                _scene.AddRotationZ(_rotation[2]);
            }

            if (_motion[0] != 0 || _motion[1] != 0 || _motion[2] != 0)
            {
                _scene.AddTranslation(_motion);
            }

            var (points, figures)=_scene.Update(window.Width, window.Height);
            DrawView(points,figures);
        }

        private void DrawView(Dictionary<int, Point> points2D, List<Figure> figures)
        {
            Canvas.Children.Clear();
            foreach (var figure in figures)
            {
                foreach (var (a, b, c) in figure.Triangles)
                {
                    try
                    {
                        var p1 = new Point(_canvasWidth - points2D[a].X, _canvasHeight - points2D[a].Y);
                        var p2 = new Point(_canvasWidth - points2D[b].X, _canvasHeight - points2D[b].Y);
                        var p3 = new Point(_canvasWidth - points2D[c].X, _canvasHeight - points2D[c].Y);

                        var triangle = new Polygon()
                        {
                            Stroke = figure.Stroke,
                            Fill = figure.Fill,
                            StrokeThickness = 0
                        };
                        triangle.Points.Add(p1);
                        triangle.Points.Add(p2);
                        triangle.Points.Add(p3);

                        Canvas.Children.Add(triangle);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception" + e.StackTrace);
                    }
                }
            }
        }

        private void Canvas_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {   //Translation
                case Key.D:
                    _motion[0] = -1;
                    break;
                case Key.A:
                    _motion[0] = 1;
                    break;
                case Key.Q:
                    _motion[1] = -1;
                    break;
                case Key.E:
                    _motion[1] = 1;
                    break;
                case Key.W:
                    _motion[2] = -1;
                    break;
                case Key.S:
                    _motion[2] = 1;
                    break;
                //Rotation
                case Key.K:
                    _rotation[0] = -1;
                    break;
                case Key.I:
                    _rotation[0] = 1;
                    break;
                case Key.J:
                    _rotation[1] = 1;
                    break;
                case Key.L:
                    _rotation[1] = -1;
                    break;
                case Key.O:
                    _rotation[2] = -1;
                    break;
                case Key.U:
                    _rotation[2] = 1;
                    break;
                //Zoom
                case Key.Z:
                    _zoom = -1;
                    break;
                case Key.X:
                    _zoom = 1;
                    break;
            }
        }

        private void Canvas_OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {   //Translation
                case Key.D:
                case Key.A:
                    _motion[0] = 0;
                    break;
                case Key.Q:
                case Key.E:
                    _motion[1] = 0;
                    break;
                case Key.W:
                case Key.S:
                    _motion[2] = 0;
                    break;
                //Rotation
                case Key.I:
                case Key.K:
                    _rotation[0] = 0;
                    break;
                case Key.L:
                case Key.J:
                    _rotation[1] = 0;
                    break;
                case Key.O:
                case Key.U:
                    _rotation[2] = 0;
                    break;
                //Zoom
                case Key.Z:
                case Key.X:
                    _zoom = 0;
                    break;
            }
        }
    }
}
