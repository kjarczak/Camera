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

            var (points, lines)=_scene.Update(window.Width, window.Height);
            DrawView(points, lines);
        }

        private void DrawView(Dictionary<int, Point> points2D, List<(int, int)> lines2D)
        {
            Canvas.Children.Clear();
            foreach (var (a, b) in lines2D)
            {
                try
                {   
                    var line = new Line()
                    {
                        X1 = points2D[a].X,
                        Y1 = _canvasHeight- points2D[a].Y,
                        X2 = points2D[b].X,
                        Y2 = _canvasHeight - points2D[b].Y,
                        Stroke = Brushes.Black
                    };
                    Canvas.Children.Add(line);
                }
                catch (Exception e)
                {
                   Console.WriteLine("Exception"+ e.StackTrace);
                }
            }
        }

        private void Canvas_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {   //Translation
                case Key.D:
                    _motion[0] = 1;
                    break;
                case Key.A:
                    _motion[0] = -1;
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
                    _rotation[1] = -1;
                    break;
                case Key.L:
                    _rotation[1] = 1;
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
