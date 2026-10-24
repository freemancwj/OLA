/*******************************************************************************
 * Copyright © 2020 WaterCloud.Framework 版权所有
 * Author: WaterCloud
 * Description: WaterCloud快速开发平台
 * Website：
*********************************************************************************/

using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using System.DrawingCore.Imaging;
using System.IO;
using System.Linq;

namespace WaterCloud.Code
{
	/// <summary>
	/// 滑块拼图验证码生成
	/// </summary>
	public class SliderVerifyHelper
	{
		public const int Width = 300;
		public const int Height = 120;
		/// <summary>
		/// 拼图方块边长
		/// </summary>
		public const int PieceSize = 40;
		/// <summary>
		/// 拼图圆形凸起半径
		/// </summary>
		private const int KnobRadius = 8;
		/// <summary>
		/// 拼图凸起超出方块的范围，用于保证拼图图片不被裁切
		/// </summary>
		private const int PiecePadding = 10;
		/// <summary>
		/// 拼图图片（含凸起部分）的实际边长
		/// </summary>
		public const int PieceBoxSize = PieceSize + PiecePadding * 2;

		/// <summary>
		/// 生成滑块验证码图片
		/// </summary>
		/// <param name="webRootPath">站点静态文件根目录</param>
		/// <returns>验证码图片及拼图目标位置</returns>
		public SliderCaptchaResult Create(string webRootPath)
		{
			using (Bitmap background = CreateBackground(webRootPath))
			{
				int pieceX = RandomNumber(PiecePadding + 5, Width - PieceSize - PiecePadding - 5);
				int pieceY = RandomNumber(PiecePadding + 5, Height - PieceSize - PiecePadding - 5);

				string backgroundImage;
				string pieceImage;

				using (GraphicsPath path = CreatePuzzlePath())
				{
					// 在背景图上挖出拼图形状的缺口
					using (Bitmap gapImage = (Bitmap)background.Clone())
					{
						using (Graphics g = Graphics.FromImage(gapImage))
						{
							g.SmoothingMode = SmoothingMode.AntiAlias;
							GraphicsState state = g.Save();
							g.TranslateTransform(pieceX - PiecePadding, pieceY - PiecePadding);
							using (Brush brush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
							{
								g.FillPath(brush, path);
							}
							using (Pen pen = new Pen(Color.FromArgb(210, 255, 255, 255), 1.5f))
							{
								g.DrawPath(pen, path);
							}
							g.Restore(state);
						}
						backgroundImage = ToBase64(gapImage);
					}

					// 按拼图形状从背景图裁出拼图块
					using (Bitmap piece = new Bitmap(PieceBoxSize, PieceBoxSize))
					{
						using (Graphics g = Graphics.FromImage(piece))
						{
							g.SmoothingMode = SmoothingMode.AntiAlias;
							g.SetClip(path);
							g.DrawImage(background,
								new Rectangle(0, 0, PieceBoxSize, PieceBoxSize),
								new Rectangle(pieceX - PiecePadding, pieceY - PiecePadding, PieceBoxSize, PieceBoxSize),
								GraphicsUnit.Pixel);
							g.ResetClip();
							using (Pen pen = new Pen(Color.FromArgb(220, 255, 255, 255), 1.5f))
							{
								g.DrawPath(pen, path);
							}
						}
						pieceImage = ToBase64(piece);
					}
				}

				return new SliderCaptchaResult
				{
					BackgroundImage = backgroundImage,
					PieceImage = pieceImage,
					PieceX = pieceX,
					PieceOffsetX = pieceX - PiecePadding,
					PieceY = pieceY - PiecePadding,
					PieceBoxSize = PieceBoxSize,
					Width = Width,
					Height = Height
				};
			}
		}

		/// <summary>
		/// 拼图路径（上侧凹槽、右侧凸起）
		/// </summary>
		private static GraphicsPath CreatePuzzlePath()
		{
			int x0 = PiecePadding;
			int y0 = PiecePadding;
			int x1 = PiecePadding + PieceSize;
			int y1 = PiecePadding + PieceSize;
			int mx = (x0 + x1) / 2;
			int my = (y0 + y1) / 2;
			int r = KnobRadius;
			GraphicsPath path = new GraphicsPath();
			path.AddLine(x0, y0, mx - r, y0);
			path.AddArc(mx - r, y0 - r, 2 * r, 2 * r, 180, -180);
			path.AddLine(mx + r, y0, x1, y0);
			path.AddLine(x1, y0, x1, my - r);
			path.AddArc(x1 - r, my - r, 2 * r, 2 * r, 270, 180);
			path.AddLine(x1, my + r, x1, y1);
			path.AddLine(x1, y1, x0, y1);
			path.AddLine(x0, y1, x0, y0);
			path.CloseFigure();
			return path;
		}

		/// <summary>
		/// 随机裁剪一张站点图片作为背景，图片缺失时使用渐变背景
		/// </summary>
		private static Bitmap CreateBackground(string webRootPath)
		{
			List<string> files = new List<string> { "images/bgs.jpg", "images/bgnew.jpeg", "images/moon.jpg", "images/bg.jpg" };
			files = files.Select(f => string.IsNullOrEmpty(webRootPath) ? null : Path.Combine(webRootPath, f.Replace('/', Path.DirectorySeparatorChar)))
				.Where(f => f != null && File.Exists(f))
				.OrderBy(f => Guid.NewGuid())
				.ToList();
			foreach (string file in files)
			{
				try
				{
					using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
					using (Image source = Image.FromStream(fs))
					{
						double scale = Math.Max((double)Width / source.Width, (double)Height / source.Height);
						int drawWidth = (int)Math.Ceiling(source.Width * scale);
						int drawHeight = (int)Math.Ceiling(source.Height * scale);
						int offsetX = RandomNumber(0, Math.Max(0, drawWidth - Width));
						int offsetY = RandomNumber(0, Math.Max(0, drawHeight - Height));
						Bitmap bitmap = new Bitmap(Width, Height);
						using (Graphics g = Graphics.FromImage(bitmap))
						{
							g.InterpolationMode = InterpolationMode.HighQualityBicubic;
							g.PixelOffsetMode = PixelOffsetMode.HighQuality;
							g.DrawImage(source, new Rectangle(-offsetX, -offsetY, drawWidth, drawHeight));
						}
						return bitmap;
					}
				}
				catch (Exception)
				{
					// 当前图片不可用时尝试下一张
				}
			}
			return CreateGradientBackground();
		}

		/// <summary>
		/// 生成渐变背景（没有可用背景图片时使用）
		/// </summary>
		private static Bitmap CreateGradientBackground()
		{
			Bitmap bitmap = new Bitmap(Width, Height);
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				Color from = Color.FromArgb(RandomNumber(30, 120), RandomNumber(80, 200), RandomNumber(120, 220));
				Color to = Color.FromArgb(RandomNumber(30, 120), RandomNumber(80, 200), RandomNumber(120, 220));
				using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), from, to, RandomNumber(0, 360)))
				{
					g.FillRectangle(brush, 0, 0, Width, Height);
				}
				for (int i = 0; i < 12; i++)
				{
					using (Brush brush = new SolidBrush(Color.FromArgb(RandomNumber(30, 70), RandomNumber(200, 255), RandomNumber(200, 255), RandomNumber(200, 255))))
					{
						int size = RandomNumber(20, 80);
						g.FillEllipse(brush, RandomNumber(-size, Width), RandomNumber(-size, Height), size, size);
					}
				}
			}
			return bitmap;
		}

		private static string ToBase64(Bitmap bitmap)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				bitmap.Save(ms, ImageFormat.Png);
				return Convert.ToBase64String(ms.ToArray());
			}
		}

		private static int RandomNumber(int min, int max)
		{
			if (max <= min)
			{
				return min;
			}
			return new Random(Guid.NewGuid().GetHashCode()).Next(min, max + 1);
		}
	}

	/// <summary>
	/// 滑块验证码生成结果
	/// </summary>
	public class SliderCaptchaResult
	{
		/// <summary>
		/// 带缺口的背景图（Base64）
		/// </summary>
		public string BackgroundImage { get; set; }
		/// <summary>
		/// 拼图块图片（Base64）
		/// </summary>
		public string PieceImage { get; set; }
		/// <summary>
		/// 拼图目标横向位置，仅保存在服务端
		/// </summary>
		public int PieceX { get; set; }
		/// <summary>
		/// 拼图图片目标左边距，用于与前端拖动位置比对，仅保存在服务端
		/// </summary>
		public int PieceOffsetX { get; set; }
		/// <summary>
		/// 拼图图片上边距
		/// </summary>
		public int PieceY { get; set; }
		/// <summary>
		/// 拼图图片实际边长
		/// </summary>
		public int PieceBoxSize { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
	}
}
