using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace Camera
{
    class Scene
    {
        private double _fov = 1.117011f;
        private const float FovDelta = 0.01f;
        private const float FovMin = 0.5f;
        private const float FovMax = 3f;

        private const double Near = 100;
        private const double Far = 20000;
        private const double Ratio = 1.6; //    Width/Height

        private const float Tx = 20f;
        private const float Ty = 20f;
        private const float Tz = 20f;

        private const double Fi = 0.03;//0.01745329;
        private readonly double _sinFi;
        private readonly double _cosFi;
        
        private double[,] _matrixOfChanges;
        private double[,] _projectionMatrix;
        
        private List<Figure> _figures;
        private Dictionary<int, Point3D> _points3D;

        public Scene()
        {
            _sinFi = Math.Sin(Fi);
            _cosFi = Math.Cos(Fi);

            _matrixOfChanges = new double[,]{
                { 1, 0, 0, 0},
                { 0, 1, 0, 0},
                { 0, 0, 1, 0},
                { 0, 0, 0, 1}
            };

            var top = Math.Tan(_fov / 2.0) * Near;
            var bottom = -top;
            var right = top * Ratio;
            var left = -top * Ratio;

            _projectionMatrix = new[,]{
                { (2 * Near) / (right - left), 0, (right + left) / (right - left), 0},
                { 0, (2 * Near) / (top - bottom), 0, 0},
                { 0, 0, -((Far + Near) / (Far - Near)), -((2 * Far * Near) / (Far - Near))},
                { 0, 0, -1, 0}};
        }

        public void LoadScene()
        {
            _points3D = new Dictionary<int, Point3D>();
            _figures = new List<Figure>();
            SetRandomScene();
        }

        public void SetRandomScene()
        {

            var size = 160;
            AddGround(new Point3D(-300,0,-300), 5, 6,100,100);

            var rng = new Random();
            var col = new byte[3]; 
            for (int i = 0; i < 3; i++)
            {
                rng.NextBytes(col);
                AddCube(rng.NextDouble() * size- size/2, rng.NextDouble() * size - 20, rng.NextDouble() * size- size/2, rng.NextDouble() * 50 + 10, rng.NextDouble() * 50 + 10, rng.NextDouble() * 50 + 10,col);
            }
        }

        private void AddGround(Point3D p, int xNumber , int zNumber, double xSize, double zSize)
        {
            var figure = new Figure()
            {
                Fill = Brushes.Purple
            };

            var index = _points3D.Count;
            for (var i = 0; i < zNumber+1; i++)
            {
                for (var j = 0; j < xNumber+1; j++)
                {
                    if (j<xNumber && i<zNumber)
                    {
                        figure.Triangles.Add((index, index + xNumber+1, index + xNumber + 2));
                        figure.Triangles.Add((index, index + xNumber+2, index + 1));
                    }
                    _points3D.Add(index++, new Point3D(p.X + j * xSize, p.Y, p.Z + i * zSize));
                }
            }
            _figures.Add(figure);
        }
        public void AddCube(double xPos, double yPos, double zPos, double xSize, double ySize, double zSize, byte[] col)
        {
            var index = _points3D.Count;
            _points3D.Add(index, new Point3D(xPos, yPos, zPos));
            _points3D.Add(index+1, new Point3D(xPos, yPos+ySize, zPos));
            _points3D.Add(index+2, new Point3D(xPos+xSize, yPos+ySize, zPos));
            _points3D.Add(index+3, new Point3D(xPos+xSize, yPos, zPos));

            _points3D.Add(index+4, new Point3D(xPos, yPos, zPos+zSize));
            _points3D.Add(index+5, new Point3D(xPos, yPos + ySize, zPos+zSize));
            _points3D.Add(index+6, new Point3D(xPos + xSize, yPos + ySize, zPos+zSize));
            _points3D.Add(index+7, new Point3D(xPos + xSize, yPos, zPos+zSize));

            var figure = new Figure(new List<(int, int, int)>()
            {
                (index + 3, index, index + 1),
                (index + 3, index + 1, index + 2),
                (index + 7, index + 3, index + 2),
                (index + 7, index + 2, index + 6),
                (index + 4, index + 7, index + 6),
                (index + 4, index + 6, index + 5),
                (index, index + 4, index + 5),
                (index, index + 5, index + 1),
                (index + 2, index + 1, index + 5),
                (index + 2, index + 5, index + 6),
                (index + 3, index + 4, index),
                (index + 3, index + 7, index + 4)
            }) {Fill = new SolidColorBrush(Color.FromRgb(col[0], col[1], col[2]))};
            _figures.Add(figure);
        }

        public void AddZoom(short zoom)
        {
            _fov += zoom*FovDelta;
            if (_fov < FovMin)
            {
               _fov = FovMin;
            }else if (_fov > FovMax)
            {
                _fov = FovMax;
            }

            var top = Math.Tan(_fov / 2.0) * Near;
            var bottom = -top;
            var right = top * Ratio;
            var left = -top * Ratio;

            _projectionMatrix = new[,]{
                { (2 * Near) / (right - left), 0, (right + left) / (right - left), 0},
                { 0, (2 * Near) / (top - bottom), 0, 0},
                { 0, 0, -((Far + Near) / (Far - Near)), -((2 * Far * Near) / (Far - Near))},
                { 0, 0, -1, 0}};
        }
        public void AddRotationX(short rotation)
        {
            double[,] xRotationMatrix = {
                {1,0,0,0},
                {0,_cosFi,-_sinFi *rotation,0},
                {0,_sinFi *rotation,_cosFi,0},
                {0,0,0,1}};
            _matrixOfChanges = Multiply(xRotationMatrix, _matrixOfChanges);
        }
        public void AddRotationY(short rotation)
        {
            double[,] yRotationMatrix = {
                {_cosFi,0,_sinFi*rotation,0},
                {0,1,0,0},
                {-_sinFi*rotation,0,_cosFi,0},
                {0,0,0,1}};
            _matrixOfChanges = Multiply(yRotationMatrix, _matrixOfChanges);
        }
        public void AddRotationZ(short rotation)
        {
            double[,] zRotationMatrix = {
                {_cosFi,-_sinFi *rotation,0,0},
                {_sinFi *rotation,_cosFi,0,0},
                {0,0,1,0},
                {0,0,0,1}};
            _matrixOfChanges = Multiply(zRotationMatrix, _matrixOfChanges);
        }
        public void AddTranslation(short[] motion)
        {
            double[,] motionMatrix = {
                {1,0,0,Tx*motion[0]},
                {0,1,0,Ty*motion[1]},
                {0,0,1,Tz*motion[2]},
                {0,0,0,1}};
            _matrixOfChanges = Multiply(motionMatrix, _matrixOfChanges);
        }

        public (Dictionary<int, Point>, List<Figure>) Update(double width, double height)
        { 
            UpdatePoints3D();
            return GeneratePoints2D(width, height);
        }

        private void UpdatePoints3D()
        {
            var updatedPoints = new Dictionary<int, Point3D>();
            foreach (var val in _points3D)
            {
                var mat = Multiply(_matrixOfChanges, new[,] { { val.Value.X }, { val.Value.Y }, { val.Value.Z }, { 1 } });
                if (Math.Abs(mat[3,0] - 1) > 0.00001 )
                {
                    mat[0, 0] = mat[0, 0] / mat[3, 0];
                    mat[1, 0] = mat[1, 0] / mat[3, 0];
                    mat[2, 0] = mat[2, 0] / mat[3, 0];
                }
                updatedPoints.Add(val.Key, new Point3D(mat[0,0], mat[1,0], mat[2,0]));
            }

            _points3D = updatedPoints;
            _matrixOfChanges = new double[,]{
                { 1, 0, 0, 0},
                { 0, 1, 0, 0},
                { 0, 0, 1, 0},
                { 0, 0, 0, 1}
            };
        }
        private (Dictionary<int, Point>, List<Figure>) GeneratePoints2D(double width, double height)
        {
            var (clippedPoints3D, clippedFigures) = GetClipped();
            var points2D = new Dictionary<int, Point>();

            foreach (var val in clippedPoints3D)
            {
                var mat = Multiply(_projectionMatrix, new[,] { { val.Value.X }, { val.Value.Y }, { val.Value.Z }, { 1 } });
                var x = mat[0,0] / mat[3,0];
                var y = mat[1,0] / mat[3,0];
                x = (x + 1) * 0.5 * (width - 1);
                y = (1 - (y + 1) * 0.5) * (height - 1);
                points2D.Add(val.Key, new Point { X = x, Y = y });
            }
            
            return (points2D, clippedFigures);
        }

        private (Dictionary<int, Point3D>, List<Figure>) GetClipped()//TODO Implement
        {
            var clippedPoints = _points3D.Where(point => point.Value.Z > 0).ToDictionary(point => point.Key, point => point.Value);
            var clippedFigures = new List<Figure>();
            foreach (var fig in _figures)
            {
                var clippedTriangles = fig.Triangles.Where(s =>
                    clippedPoints.ContainsKey(s.Item1) && clippedPoints.ContainsKey(s.Item2) &&
                    clippedPoints.ContainsKey(s.Item3)).ToList();

                //var x = clippedTriangles.Where(s => Vector3D.CrossProduct(
                //    new Vector3D(_points3D[s.Item2].X - _points3D[s.Item1].X, _points3D[s.Item2].Y - _points3D[s.Item1].Y, _points3D[s.Item2].Z - _points3D[s.Item1].Z),
                //    new Vector3D(_points3D[s.Item2].X - _points3D[s.Item3].X, _points3D[s.Item2].Y - _points3D[s.Item3].Y, _points3D[s.Item2].Z - _points3D[s.Item3].Z)).Z > -0.2).ToList();

                var clippedFigure = new Figure() { Triangles = clippedTriangles, Fill = fig.Fill, Stroke = fig.Stroke};
                clippedFigures.Add(clippedFigure);
            }
            return (clippedPoints, clippedFigures);
        }

        private double[,] Multiply(double[,]a, double[,] b)
        {
            var m = a.GetLength(0);
            var w = a.GetLength(1);
            var n = b.GetLength(1);
            var c = new double[m,n];
            for (var i = 0; i < m; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    double s = 0;
                    for (var k = 0; k < w; k++)
                    {
                        s += a[i,k] * b[k,j];
                    }
                    c[i, j] = s;
                }
            }
            return c;
        }
    }
}
