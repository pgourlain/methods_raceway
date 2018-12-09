using System;

namespace DynamicInvoker
{
	public class Result 
	{
		public string Name;
		public float SetupTime;
		public float ExecutionTime;
		public int Iterations;

		public Result(string name, int iterations, float executionTime, float setupTime)
		{
			Name = name;
			Iterations = iterations;
			SetupTime = setupTime;
			ExecutionTime = executionTime;
		}

		public float GetCallsPerSecond(bool includeSetupTime)
		{
			float totalTime = ExecutionTime;
			if (includeSetupTime)
			{
				totalTime += SetupTime;
			}
			return Iterations * (1 / totalTime);
		}
	}
}
