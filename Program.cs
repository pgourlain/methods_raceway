using System;
using System.Collections;
using DynamicInvoker;

namespace methods_raceway
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Methods call raceway");

            Caller caller = new Caller();
			ArrayList results = caller.Call(1);	// once to get things up and runnin'

            results = DoRace(caller, "10000000");

            foreach (Result resultItem in results)
            {
                System.Console.WriteLine($"{resultItem.Name} : {resultItem.GetCallsPerSecond(false)}"); 
            }
        }

        private static ArrayList DoRace(Caller caller, string laps)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			int iterations = Int32.Parse(laps);
			ArrayList results = caller.Call(iterations);

			//ClearPercentageValues();

			//currentStep = 0;
			//fastestCallsPerSecond = FindFastest(includeSetupTime);

			//callsPerPixel = fastestCallsPerSecond / 500F;
  
			//timer1.Enabled = true;
            return results;		
		}

        float FindFastest(bool includeSetupTime, ArrayList results)
		{
			float fastest = 0.0F;

			foreach (Result result in results)
			{
				float perSecond = result.GetCallsPerSecond(includeSetupTime);
				if (perSecond > fastest)
				{
					fastest = perSecond;
				}
			}
			return fastest;
		}
    }
}
