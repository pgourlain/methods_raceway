using System;

namespace DynamicInvoker
{
	/// <summary>
	/// Summary description for Processor.
	/// </summary>
	public class Processor: IProcessor
	{
		public int Val = 0;

		public Processor()
		{
		}

		public int Process(int value)
		{
			return value + 1;
		}
	}

	public class Processor2 : Processor, IProcessor2
	{
		public virtual int ProcessV(int value)
		{
			return value + 1;
		}
		
		public static int ProcessStatic(int value)
		{
			return value + 1;
		}
	}
}
