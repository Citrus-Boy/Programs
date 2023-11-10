using System;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Citrus.Input;
class Program
{
	static void Main()
    {
        MouseContoller mc = new MouseContoller();
	bool inLoop = true;
	Thread t = new Thread(()=>{Console.ReadLine(); inLoop = false;});
	t.Start();
	while(inLoop)
	{
		if(ScreenPeeker.Peek(ScreenPeeker.CursorPos) == Color.FromArgb(255, 75, 219, 106))
		{
			mc.Click();
			Thread.Sleep(500);
		}
	}
		Console.WriteLine("END");
		System.Threading.Thread.Sleep(-1);
	}
}