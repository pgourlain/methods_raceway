using System;

namespace DynamicInvoker
{
	/// <summary>
	/// Summary description for IProcessor.
	/// </summary>
	public interface IProcessor
	{
		int Process(int value);
	}
	
	public interface IProcessor2 : IProcessor
	{
		int ProcessV(int value);
	}
}
