using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
class ScreenPeeker
{
	[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
	public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
	
	[DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point lpPoint);
	
	public static Point CursorPos
	{
		get
		{
			int screenWidth = Screen.PrimaryScreen.Bounds.Width;
			int screenHeight = Screen.PrimaryScreen.Bounds.Height;
			
			Point cursorPos = new Point(-1,-1);
			GetCursorPos(ref cursorPos);
			cursorPos = new Point((cursorPos.X * 100000) / screenWidth, (cursorPos.Y * 100000) / screenHeight);
			return cursorPos;
		}
	}
	public static Color Peek(Point point)
	{
		int x = point.X;
		int y = point.Y;
		double percentX = (double)x/100000;
		double percentY = (double)y/100000;
		
		int screenWidth = Screen.PrimaryScreen.Bounds.Width;
		int screenHeight = Screen.PrimaryScreen.Bounds.Height;
		
		Point screenLocation = new Point((int)Math.Round(screenWidth * percentX), (int)Math.Round(screenHeight * percentY));
		return GetColorValue(screenLocation);
	}
	public static Color GetColorValue(Point screenPos)
	{
		Bitmap screenPixel = new Bitmap(1,1);
		using (Graphics gdest = Graphics.FromImage(screenPixel))
		{
			using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
			{
				IntPtr hSrcDC = gsrc.GetHdc();
				IntPtr hDC = gdest.GetHdc();
				int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, screenPos.X, screenPos.Y, (int)CopyPixelOperation.SourceCopy);
				gdest.ReleaseHdc();
				gsrc.ReleaseHdc();
			}
		}
		return screenPixel.GetPixel(0, 0);
	}
}