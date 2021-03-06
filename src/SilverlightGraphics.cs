﻿//
// Copyright (c) 2010-2012 Frank A. Krueger
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Point = Windows.Foundation.Point;
using Rectangle = Windows.UI.Xaml.Shapes.Rectangle;
using Size = Windows.Foundation.Size;
using NativePolygon = Windows.UI.Xaml.Shapes.Polygon;
using NativeTextAlignment = Windows.UI.Xaml.TextAlignment;
using NativeColor = Windows.UI.Color;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Windows.Size;
using NativePolygon = System.Windows.Shapes.Polygon;
using NativeTextAlignment = System.Windows.TextAlignment;
using NativeColor = System.Windows.Media.Color;
#endif

namespace CrossGraphics.SilverlightGraphics
{
	public class SilverlightGraphics : IGraphics
	{
		Dictionary<object, EntityShapes> _shapes = new Dictionary<object, EntityShapes> ();
		Dictionary<object, EntityShapes> _drawnShapes = new Dictionary<object, EntityShapes> ();

		EntityShapes _eshape = null;

		Canvas _canvas;

		class State
		{
			public float TranslateX = 0;
			public float TranslateY = 0;
			public float ScaleX = 0;
			public float ScaleY = 0;
		}

		Stack<State> _states = new Stack<State>();

		public SilverlightGraphics (Canvas canvas)
		{
			if (canvas == null) throw new ArgumentNullException ("canvas");
			_canvas = canvas;
			_states.Push(new State());
		}

		public void SaveState()
		{
			var s = _states.Peek();

			_states.Push(new State()
			{
				TranslateX = s.TranslateX,
				TranslateY = s.TranslateY,
				ScaleX = s.ScaleX,
				ScaleY = s.ScaleY,
			});            
		}

		public void RestoreState()
		{
			if (_states.Count > 1)
			{
				_states.Pop();
			}
		}

		public void Translate(float dx, float dy)
		{
			var s = _states.Peek();
			s.TranslateX += dx;
			s.TranslateY += dy;
		}

		public void Scale(float sx, float sy)
		{
			var s = _states.Peek();
			s.ScaleX *= sx;
			s.ScaleY *= sy;
		}

		public void SetClippingRect(float x, float y, float w, float h)
		{
		}

		public void BeginDrawing ()
		{
			_drawnShapes.Clear ();
		}

		public void EndDrawing ()
		{
			if (_eshape != null) {
				_eshape.End ();
				_eshape = null;
			}

			var toRemove = new List<object> ();
			foreach (var k in _shapes.Keys) {
				if (!_drawnShapes.ContainsKey (k)) {
					toRemove.Add (k);
				}
			}
			foreach (var k in toRemove) {
				// Log.println ("Clearing " + k);
				var s = _shapes[k];
				s.Clear ();
				_shapes.Remove (k);
			}
		}

		public void BeginEntity (object entity)
		{
			var font = default(Font);

			if (_eshape != null) {
				_eshape.End ();
				font = _eshape.CurrentFont;
				_eshape = null;                
			}

			EntityShapes eshape = null;
			if (!_shapes.TryGetValue (entity, out eshape)) {
				eshape = new EntityShapes (entity, _canvas);
				_shapes[entity] = eshape;
			}
			_eshape = eshape;
			if (font != null) _eshape.SetFont(font);
			_drawnShapes.Add (entity, _eshape);
			_eshape.Begin ();
		}

		public void SetFont (Font font)
		{
			_eshape.SetFont (font);
		}

		public void SetColor (Color color)
		{
			_eshape.SetColor (color);
		}

		public void FillPolygon (Polygon poly)
		{
			_eshape.FillPolygon (poly);
		}

		public void DrawPolygon (Polygon poly, float w)
		{
			_eshape.DrawPolygon (poly, w);
		}

		public void FillRoundedRect (float x, float y, float width, float height, float radius)
		{
			_eshape.FillRoundedRect (x, y, width, height, radius);
		}

		public void DrawRoundedRect (float x, float y, float width, float height, float radius, float w)
		{
			_eshape.DrawRoundedRect (x, y, width, height, radius, w);
		}

		public void FillRect (float x, float y, float width, float height)
		{
			_eshape.FillRect (x, y, width, height);
		}

		public void DrawRect (float x, float y, float width, float height, float w)
		{
			_eshape.DrawRect (x, y, width, height, w);
		}

		public void FillOval (float x, float y, float width, float height)
		{
			_eshape.FillOval (x, y, width, height);
		}

		public void DrawOval (float x, float y, float width, float height, float w)
		{
			_eshape.DrawOval (x, y, width, height, w);
		}

		public void BeginLines (bool rounded)
		{
			_eshape.BeginLines ();
		}

		public void DrawLine (float sx, float sy, float ex, float ey, float w)
		{
			_eshape.DrawLine (sx, sy, ex, ey, w);
		}

		public void EndLines ()
		{
			_eshape.EndLines ();
		}

		public void DrawImage (IImage img, float x, float y, float width, float height)
		{
			_eshape.DrawImage (img, x, y, width, height);
		}

		public void DrawString (string s, float x, float y)
		{
			_eshape.DrawString (s, x, y);
		}

		public void FillArc (float cx, float cy, float radius, float startAngle, float endAngle)
		{
			_eshape.FillArc (cx, cy, radius, startAngle, endAngle);
		}

		public void DrawArc(float cx, float cy, float radius, float startAngle, float endAngle, float w)
		{
			_eshape.DrawArc (cx, cy, radius, startAngle, endAngle, w);
		}

		public void DrawString(string s, float x, float y, float width, float height, LineBreakMode lineBreak, TextAlignment align)
		{
			_eshape.DrawString (s, x, y, width, height, lineBreak, align);
		}

		public IFontMetrics GetFontMetrics ()
		{
			var f = _eshape.CurrentFont;
			var fm = f.Tag as FontMetrics;
			if (fm == null) {
				fm = new FontMetrics(f);
				f.Tag = fm;
			}
			return fm;
		}

		public IImage ImageFromFile (string path)
		{
			return new SilverlightImage (path);
		}
	}

	public class SilverlightImage : IImage
	{
		public BitmapSource Bitmap { get; private set; }
		public SilverlightImage (string path)
		{
			Bitmap = new BitmapImage (new Uri ("Images\\" + path, UriKind.RelativeOrAbsolute));
		}
	}

	public class FontMetrics : IFontMetrics
	{
		float[] _widths;
		int _height = 10;
		static float DefaultWidth = 9.5f;

		public FontMetrics(Font f)
		{
			int fsz = f.Size;

			var mmSize = StringSize("MM", fsz);

			_height = f.Size;// (int)mmSize.Height;// f.Size;// (int)(mmSize.Height * 1.2f);

			_widths = new float[0x80];

			for (var i = ' '; i < 127; i++) {

				var s = "M" + ((char)i).ToString() + "M";

				var ssz = StringSize(s, fsz);

				var w = ssz.Width - mmSize.Width;

				_widths[i] = w;
			}
		}

		static System.Drawing.SizeF StringSize(string text, int fontSize)
		{
			TextBlock txtMeasure = new TextBlock();
			txtMeasure.FontSize = fontSize;
			txtMeasure.Text = text;
			txtMeasure.Measure (new Size (1, 1));
			return new System.Drawing.SizeF((float)txtMeasure.ActualWidth, (float)txtMeasure.ActualHeight);
		}


		public int StringWidth(string str, int startIndex, int length)
		{
			if (str == null) return 0;

			var end = startIndex + length;
			if (end <= 0) return 0;

			var w = 0.0f;

			for (var i = startIndex; i < end; i++) {
				var ch = (int)str[i];
				if (ch < 128) {
					w += _widths[ch];
				}
				else {
					w += DefaultWidth;
				}
			}
			return (int)(w + 0.5f);
		}

		public int Height
		{
			get
			{
				return _height;
			}
		}

		public int Ascent
		{
			get
			{
				return _height;
			}
		}

		public int Descent
		{
			get
			{
				return 0;
			}
		}
	}

	enum TypeId
	{
		Line = 0,
		Text,
		Oval,
		RoundedRect,
		Rect,
		Image,
		Polygon,
		Arc,
	}

	class EntityShapes
	{
		Canvas _canvas;

		Color _currentColor = null;

		public Font CurrentFont = null;

		public bool LogNewShapes = false;
		public bool LogBadShapes = false;

		//static int[] typeCounts = new int[8];

		enum DrawOp
		{
			Draw,
			Fill
		}

		class ShapeData
		{
			public TypeId TypeId;
			public UIElement Element;
			public CrossGraphics.Color Color;
			public CrossGraphics.Font Font;
			public float X, Y, Width, Height;
			public float Thickness, Radius;
			public string Text;
			public TextAlignment TextAlignment;
			public int Count;
			public SilverlightImage Image;
			public DrawOp DrawOp;
			public override string ToString ()
			{
				return string.Format("{0} {1}", DrawOp, TypeId);
			}
		}

		List<ShapeData> _shapes = new List<ShapeData>();

		int _shapeIndex = 0;

		object _entity;

		public EntityShapes (object entity, Canvas canvas)
		{
			_canvas = canvas;
			_entity = entity;
		}

		ShapeData GetNextShape(TypeId typeId)
		{
			ShapeData s = null;

			//typeCounts[(int)typeId]++;

			if (_shapeIndex >= _shapes.Count) {
				if (LogNewShapes) {
					Debug.WriteLine("Adding shape " + typeId + " for " + _entity);
				}
				s = ConstructAndAddShape(typeId);
			}
			else if (_shapes[_shapeIndex].TypeId != typeId) {
				if (LogBadShapes) {
					Debug.WriteLine("Bad shape " + _shapeIndex + "! Wanted " + typeId + " got " + _shapes[_shapeIndex].TypeId + " for " + _entity);
				}
				TrimShapes();
				s = ConstructAndAddShape(typeId);
			}
			else {
				s = _shapes[_shapeIndex];
			}

			_shapeIndex++;

			_lastShape = s;

			return s;
		}

		ShapeData _lastShape;
		int _lastAddElementIndex = -1;

		ShapeData ConstructAndAddShape (TypeId typeId)
		{
			UIElement element;

			if (typeId == TypeId.Line) {
				var line = new Line {StrokeEndLineCap = PenLineCap.Round};
				element = line;
			}
			else if (typeId == TypeId.Text) {
				element = new TextBlock ();
			}
			else if (typeId == TypeId.Oval) {
				element = new Ellipse ();
			}
			else if (typeId == TypeId.Arc) {
				element = new Path();
			}
			else if (typeId == TypeId.RoundedRect) {
				element = new Rectangle ();
			}
			else if (typeId == TypeId.Rect) {
				element = new Rectangle ();
			}
			else if (typeId == TypeId.Image) {
				element = new Image ();
			}
			else if (typeId == TypeId.Polygon) {
				element = new NativePolygon ();
			}
			else {
				throw new NotSupportedException ("Don't know how to construct: " + typeId);
			}

			var sd = new ShapeData {
				Element = element,
				TypeId = typeId,
			};

			_shapes.Add (sd);

			//
			// Insert it in the right place so it gets drawn in the right order
			//
			if (_lastAddElementIndex >= 0) {
				_lastAddElementIndex++;
				_canvas.Children.Insert(_lastAddElementIndex, element);
			}
			else {
				if (_lastShape != null) {
					_lastAddElementIndex = _canvas.Children.IndexOf(_lastShape.Element) + 1;
					_canvas.Children.Insert(_lastAddElementIndex, element);
				}
				else {
					_lastAddElementIndex = _canvas.Children.Count;
					_canvas.Children.Add(element);
				}
			}

			return sd;
		}

		void TrimShapes ()
		{
			if (_shapeIndex < _shapes.Count) {
				var n = _shapes.Count - _shapeIndex;
				for (var i = 0; i < n; i++) {
					_canvas.Children.Remove (_shapes[_shapeIndex + i].Element);
				}
				_shapes.RemoveRange (_shapeIndex, n);
			}
		}

		public void Clear ()
		{
			_shapeIndex = 0;
			_lastShape = null;
			_lastAddElementIndex = -1;
			TrimShapes ();
		}

		public void Begin ()
		{
			_currentColor = Colors.Black;
			_shapeIndex = 0;
			_lastShape = null;
			_lastAddElementIndex = -1;
		}

		public void End ()
		{
			TrimShapes ();
		}

		public void SetFont (Font font)
		{
			CurrentFont = font;
		}

		public void SetColor (Color color)
		{
			_currentColor = color;
		}

		public void FillPolygon (Polygon poly)
		{
			var s = GetNextShape(TypeId.Polygon);
			var e = s.Element as NativePolygon;

			var n = poly.Points.Count;
			if (n == 0) return;

			var p0 = poly.Points[0];

			if ((s.Count != n) || (p0.X != s.X) || (p0.Y != s.Y))
			{
				s.Count = n;
				s.X = p0.X;
				s.Y = p0.Y;

				var ps = new PointCollection();
				for (var i = 0; i < n; i++) {
					ps.Add(new Point(poly.Points[i].X, poly.Points[i].Y));
				}
				e.Points = ps;
			}

			if (s.Color != _currentColor || s.DrawOp != DrawOp.Fill) {
				s.Color = _currentColor;
				s.DrawOp = DrawOp.Fill;
				e.Fill = _currentColor.GetBrush();
				e.Stroke = null;                
			}
		}

		public void DrawPolygon (Polygon poly, float w)
		{
			var s = GetNextShape(TypeId.Polygon);
			var e = s.Element as NativePolygon;

			var n = poly.Points.Count;
			if (n == 0) return;

			var p0 = poly.Points[0];

			if ((s.Count != n) || (p0.X != s.X) || (p0.Y != s.Y))
			{
				s.Count = n;
				s.X = p0.X;
				s.Y = p0.Y;

				var ps = new PointCollection();                
				for (var i = 0; i < n; i++)
				{
					ps.Add(new Point(poly.Points[i].X, poly.Points[i].Y));
				}
				e.Points = ps;
			}

			if (s.Color != _currentColor || s.DrawOp != DrawOp.Draw) {
				s.Color = _currentColor;
				s.DrawOp = DrawOp.Draw;
				e.Stroke = _currentColor.GetBrush();                
				e.Fill = null;                
			}
			if (s.Thickness != w) {
				s.Thickness = w;
				e.StrokeThickness = w;
			}
		}

		public void FillRoundedRect (float x, float y, float width, float height, float radius)
		{
			var s = GetNextShape(TypeId.RoundedRect);
			var e = s.Element as Rectangle;

			if (s.Y != y) {
				s.Y = y;
				Canvas.SetTop(e, y);
			}
			if (s.X != x) {
				s.X = x;
				Canvas.SetLeft(e, x);
			}
			if (s.Width != width) {
				s.Width = width;
				e.Width = width;
			}
			if (s.Height != height) {
				s.Height = height;
				e.Height = height;
			}
			if (s.Color != _currentColor || s.DrawOp != DrawOp.Fill) {
				s.Color = _currentColor;
				s.DrawOp = DrawOp.Fill;
				e.Fill = _currentColor.GetBrush();
				e.Stroke = null;                
			}
			if (s.Radius != radius) {
				s.Radius = radius;
				e.RadiusX = e.RadiusY = radius;
			}
		}

		public void DrawRoundedRect (float x, float y, float width, float height, float radius, float w)
		{
			var s = GetNextShape(TypeId.RoundedRect);
			var e = s.Element as Rectangle;

			if (s.Y != y) {
				s.Y = y;
				Canvas.SetTop(e, y - w / 2);
			}
			if (s.X != x) {
				s.X = x;
				Canvas.SetLeft (e, x - w / 2);
			}
			if (s.Width != width) {
				s.Width = width;
				e.Width = width + w;
			}
			if (s.Height != height) {
				s.Height = height;
				e.Height = height + w;
			}
			if (s.Color != _currentColor || s.DrawOp != DrawOp.Draw) {
				s.Color = _currentColor;
				s.DrawOp = DrawOp.Draw;
				e.Fill = null;
				e.Stroke = _currentColor.GetBrush();                
			}
			if (s.Radius != radius) {
				s.Radius = radius;
				e.RadiusX = e.RadiusY = radius;
			}
			if (s.Thickness != w) {
				s.Thickness = w;
				e.StrokeThickness = w;
			}
		}

		public void FillRect (float x, float y, float width, float height)
		{
			var s = GetNextShape(TypeId.Rect);
			var e = s.Element as Rectangle;

			if (s.Y != y) {
				s.Y = y;
				Canvas.SetTop(e, y);
			}
			if (s.X != x) {
				s.X = x;
				Canvas.SetLeft(e, x);
			}
			if (s.Width != width) {
				s.Width = width;
				e.Width = width;
			}
			if (s.Height != height) {
				s.Height = height;
				e.Height = height;
			}
			if (s.Color != _currentColor || s.DrawOp != DrawOp.Fill) {
				s.Color = _currentColor;
				s.DrawOp = DrawOp.Fill;
				e.Fill = _currentColor.GetBrush();
				e.Stroke = null;                
			}
		}

		public void DrawRect (float x, float y, float width, float height, float w)
		{
			var s = GetNextShape(TypeId.Rect);
			var e = s.Element as Rectangle;

			if (s.Y != y) {
				s.Y = y;
				Canvas.SetTop(e, y);
			}
			if (s.X != x) {
				s.X = x;
				Canvas.SetLeft(e, x);
			}
			if (s.Width != width) {
				s.Width = width;
				e.Width = width;
			}
			if (s.Height != height) {
				s.Height = height;
				e.Height = height;
			}
			if (s.Color != _currentColor || s.DrawOp != DrawOp.Draw) {
				s.Color = _currentColor;
				s.DrawOp = DrawOp.Draw;
				e.Fill = null;
				e.Stroke = _currentColor.GetBrush();
			}
			if (s.Thickness != w) {
				s.Thickness = w;
				e.StrokeThickness = w;
			}
		}

		public void FillOval (float x, float y, float width, float height)
		{
			var s = GetNextShape(TypeId.Oval);
			var e = s.Element as Ellipse;

			if (s.Y != y) {
				s.Y = y;
				Canvas.SetTop(e, y);
			}
			if (s.X != x) {
				s.X = x;
				Canvas.SetLeft(e, x);
			}
			if (s.Width != width) {
				s.Width = width;
				e.Width = width;
			}
			if (s.Height != height) {
				s.Height = height;
				e.Height = height;
			}
			if (s.Color != _currentColor || s.DrawOp != DrawOp.Fill) {
				s.Color = _currentColor;
				s.DrawOp = DrawOp.Fill;
				e.Fill = _currentColor.GetBrush();
				e.Stroke = null;
			}
		}

		public void DrawOval (float x, float y, float width, float height, float w)
		{
			var s = GetNextShape(TypeId.Oval);
			var e = s.Element as Ellipse;

			if (s.Y != y) {
				s.Y = y;
				Canvas.SetTop(e, y - w / 2);
			}
			if (s.X != x) {
				s.X = x;
				Canvas.SetLeft(e, x - w / 2);
			}
			if (s.Width != width) {
				s.Width = width;
				e.Width = width + w;
			}
			if (s.Height != height) {
				s.Height = height;
				e.Height = height + w;
			}
			if (s.Color != _currentColor || s.DrawOp != DrawOp.Draw) {
				s.Color = _currentColor;
				s.DrawOp = DrawOp.Draw;
				e.Stroke = _currentColor.GetBrush();
				e.Fill = null;
			}
			if (s.Thickness != w) {
				s.Thickness = w;
				e.StrokeThickness = w;
			}
		}

		public void FillArc (float cx, float cy, float radius, float startAngle, float endAngle)
		{
			var s = DoArc (cx, cy, radius, startAngle, endAngle);
			var e = (Path)s.Element;

			if (s.Color != _currentColor || s.DrawOp != DrawOp.Draw) {
				s.Color = _currentColor;
				s.DrawOp = DrawOp.Draw;
				e.Stroke = null;
				e.Fill = _currentColor.GetBrush ();
			}
		}

		public void DrawArc(float cx, float cy, float radius, float startAngle, float endAngle, float w)
		{
			var s = DoArc (cx, cy, radius, startAngle, endAngle);
			var e = (Path)s.Element;

			if (s.Thickness != w) {
				s.Thickness = w;
				e.StrokeThickness = w;
			}

			if (s.Color != _currentColor || s.DrawOp != DrawOp.Draw) {
				s.Color = _currentColor;
				s.DrawOp = DrawOp.Draw;
				e.Stroke = _currentColor.GetBrush ();
				e.Fill = null;
			}
		}

		ShapeData DoArc (float cx, float cy, float radius, float startAngle, float endAngle)
		{
			var s = GetNextShape(TypeId.Arc);
			var e = s.Element as Path;

			if (e.Data == null || s.X != cx || s.Y != cy || s.Radius != radius || s.Width != startAngle || s.Height != endAngle) {
				s.X = cx;
				s.Y = cy;
				s.Radius = radius;
				s.Width = startAngle;
				s.Height = endAngle;

				var fig = new PathFigure();
				var sa = -startAngle;
				var ea = -endAngle;
				fig.StartPoint = new Point(
					cx + radius * Math.Cos(sa),
					cy + radius * Math.Sin(sa));
				fig.Segments.Add(new ArcSegment() {
					Point = new Point(
						cx + radius * Math.Cos(ea),
						cy + radius * Math.Sin(ea)),
					Size = new Size(radius, radius),
					SweepDirection = SweepDirection.Counterclockwise,
				});
				var geo = new PathGeometry();
				geo.Figures.Add(fig);
				e.Data = geo; 
			}

			return s;
		}

		Path _linePath = null;

		public void BeginLines ()
		{
			//_linePath = GetNextElement ("LinePath") as Path;
		}

		public void DrawLine (float sx, float sy, float ex, float ey, float w)
		{
			if (_linePath != null) {

				//_linePath.D

			}
			else {
				var s = GetNextShape(TypeId.Line);
				var line = s.Element as Line;

				if (s.X != sx) {
					s.X = sx;
					line.X1 = sx;
				}
				if (s.Y != sy) {
					s.Y = sy;
					line.Y1 = sy;
				}
				if (s.Width != ex) {
					s.Width = ex;
					line.X2 = ex;
				}
				if (s.Height != ey) {
					s.Height = ey;
					line.Y2 = ey;
				}
				if (s.Color != _currentColor) {
					s.Color = _currentColor;
					line.Stroke = _currentColor.GetBrush();
				}
				if (s.Thickness != w) {
					s.Thickness = w;
					line.StrokeThickness = w;
				}				
			}
		}

		public void EndLines ()
		{
			if (_linePath != null) {
			}
		}

		public void DrawImage (IImage img, float x, float y, float width, float height)
		{
			var simg = img as SilverlightImage;
			if (simg != null) {

				var s = GetNextShape(TypeId.Image);
				var e = s.Element as Image;

				if (s.Image != simg) {
					s.Image = simg;
					e.Source = simg.Bitmap;
					e.Stretch = Stretch.Fill;
				}

				if (s.X != x) {
					s.X = x;
					Canvas.SetLeft(e, x);
				}

				if (s.Y != y) {
					s.Y = y;
					Canvas.SetLeft(e, y);
				}

				if (s.Width != width) {
					s.Width = width;
					e.Width = width;
				}

				if (s.Height != height) {
					s.Height = height;
					e.Height = height;
				}
			}
		}

		public void DrawString(string str, float x, float y, float width = 0, float height = 0, LineBreakMode lineBreak = LineBreakMode.None, TextAlignment align = TextAlignment.Left)
		{
			var s = GetNextShape(TypeId.Text);
			var e = (TextBlock)s.Element;
			//var b = s.Element as Border;
			//var e = b.Child as TextBlock;
			//if (e == null) {
			//    e = new TextBlock();
			//    b.Child = e;
			//}

			if (s.TextAlignment != align) {
				switch (align) {
					case TextAlignment.Center:
						e.TextAlignment = NativeTextAlignment.Center;
						e.Width = width;
						e.Height = height;
						break;
					default:
						e.TextAlignment = NativeTextAlignment.Left;
						break;
				}
				s.TextAlignment = align;
			}

			if (s.X != x) {
				s.X = x;
				Canvas.SetLeft(e, x);
			}
			if (s.Y != y) {
				s.Y = y;
				Canvas.SetTop(e, y);
			}
			if (s.Text != str) {
				s.Text = str;
				e.Text = str;                
				//b.Background = new SolidColorBrush(System.Windows.Media.Colors.Red);
			}
			if (s.Color != _currentColor) {
				s.Color = _currentColor;
				e.Foreground = _currentColor.GetBrush();
			}
			if (s.Font != CurrentFont) {
				s.Font = CurrentFont;
				e.Padding = new Thickness(0);
				e.RenderTransform = new TranslateTransform() {
					X = 0,
#if NETFX_CORE
					Y = -0.08 * CurrentFont.Size,
#else
					Y = -0.333 * CurrentFont.Size,
#endif
				};
				e.FontSize = CurrentFont.Size;
				//e.Height = CurrentFont.Size;
			}
		}
	}

	public static class ColorEx
	{
		public static SolidColorBrush GetBrush (this Color color)
		{
			var b = color.Tag as SolidColorBrush;
			if (b == null) {
				b = new SolidColorBrush (
					NativeColor.FromArgb (
					(byte)color.Alpha,
					(byte)color.Red,
					(byte)color.Green,
					(byte)color.Blue));
				color.Tag = b;
			}
			return b;
		}
	}

	public static class PointFEx
	{
		public static PointF ToPointF(this Point pt)
		{
			return new PointF((float)pt.X, (float)pt.Y);
		}

		public static float DistanceTo (this PointF from, PointF to)
		{
			var dx = to.X - from.X;
			var dy = to.Y - from.Y;
			return (float)Math.Sqrt(dx * dx + dy * dy);
		}
	}
}
