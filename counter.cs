using System;
using System.Diagnostics;

namespace Timing
{
	public class Counter 
	{
		Stopwatch stopWatch = new Stopwatch();
		long elapsedCount = 0;
		//long startCount = 0;

		public void Start()
		{
			//startCount = 0;
			//QueryPerformanceCounter(ref startCount);
			stopWatch.Start();
		}
		
		public void Stop()
		{
			//long stopCount = 0;
			//QueryPerformanceCounter(ref stopCount);
			stopWatch.Start();
			elapsedCount += stopWatch.ElapsedMilliseconds;
		}

		public void Clear()
		{
			elapsedCount = 0;
		}

		public float Seconds
		{
			get
			{
				//long freq = 0;
				//QueryPerformanceFrequency(ref freq);
				//return((float) elapsedCount / (float) freq);
				return elapsedCount;
			}
		}

		public override string ToString()
		{
			return String.Format("{0} seconds", Seconds);
		}

		static long Frequency 
		{
			get 
			{
				long freq = 0;
				//QueryPerformanceFrequency(ref freq);
				return freq;
			}
		}
		static long Value 
		{
			get 
			{
				long count = 0;
				//QueryPerformanceCounter(ref count);
				return count;
			}
		}

		//[System.Runtime.InteropServices.DllImport("KERNEL32")]
		//private static extern bool QueryPerformanceCounter(  ref long lpPerformanceCount);

		//[System.Runtime.InteropServices.DllImport("KERNEL32")]
		//private static extern bool QueryPerformanceFrequency( ref long lpFrequency);                     
	}
}
