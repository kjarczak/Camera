using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Camera
{
    class Figure
    {
        public List<(int, int, int)> Triangles { get; set; } = new List<(int, int, int)>();
        private SolidColorBrush _fill;
        private Brush _stroke;
        public SolidColorBrush Fill
        {
            get
            {
                if (_fill == null)
                {
                    var rng = new Random();
                    var color = new byte[3];
                    rng.NextBytes(color);
                    _fill = new SolidColorBrush(Color.FromRgb(color[0], color[1], color[2]));
                }
                return _fill;
            }
            set => _fill = value;
        }
        public Brush Stroke
        {
            get
            {
                if (_stroke == null)
                { 
                    _stroke= Brushes.Black;
                }
                return _stroke;
            }
            set=>_stroke = value;
        }

        public Figure()
        { }

        public Figure(List<(int, int, int)> triangles)
        {
            Triangles = triangles;
        }
    }
}
