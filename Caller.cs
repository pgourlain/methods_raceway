using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using Timing;
using System.Threading;

namespace DynamicInvoker
{
	/// <summary>
	/// Summary description for Caller.
	/// </summary>
	public class Caller
	{
		public delegate int ProcessCaller(int value);
		public delegate int Proc2(int p2);

		public int theSum;	// public to make sure it isn't optimized away.

		public Caller()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public Result CallDirect(Processor processor, int iterations)
		{
			int sum = 0;
			Counter counter = new Counter();
			counter.Start();
			for (int i = 0; i < iterations; i++)
			{
				sum += processor.Process(i);	
			}
			counter.Stop();
			theSum = sum;
			return new Result("Direct", iterations, counter.Seconds, 0.0F);
		}

		public Result CallDirectVirtual(Processor2 processor, int iterations)
		{
			int sum = 0;
			Counter counter = new Counter();
			counter.Start();
			for (int i = 0; i < iterations; i++)
			{
				sum += processor.ProcessV(i);	
			}
			counter.Stop();
			theSum = sum;
			return new Result("DirectVirtual", iterations, counter.Seconds, 0.0F);
		}

		
		public Result CallDirectStatic(int iterations)
		{
			int sum = 0;
			Counter counter = new Counter();
			counter.Start();
			for (int i = 0; i < iterations; i++)
			{
				sum += Processor2.ProcessStatic(i);	
			}
			counter.Stop();
			theSum = sum;
			return new Result("DirectStatic", iterations, counter.Seconds, 0.0F);
		}
		
		public Result CallInterface(IProcessor processor, int iterations)
		{
			int sum = 0;
			Counter counter = new Counter();
			counter.Start();
			for (int i = 0; i < iterations; i++)
			{
				sum += processor.Process(i);	
			}
			counter.Stop();
			theSum = sum;
			return new Result("Interface", iterations, counter.Seconds, 0.0F);
		}

		public Result CallInterfaceVirtual(IProcessor2 processor, int iterations)
		{
			int sum = 0;
			Counter counter = new Counter();
			counter.Start();
			for (int i = 0; i < iterations; i++)
			{
				sum += processor.ProcessV(i);	
			}
			counter.Stop();
			theSum = sum;
			return new Result("InterfaceVirtual", iterations, counter.Seconds, 0.0F);
		}
		
		public Result CallDelegate(Processor processor, int iterations)
		{
			int sum = 0;
			Counter counter = new Counter();
			counter.Start();
			ProcessCaller processCaller = new ProcessCaller(processor.Process);
			counter.Stop();
			float setupTime = counter.Seconds;

			counter.Clear();
			counter.Start();
			for (int i = 0; i < iterations; i++)
			{
				sum += processCaller(i);
			}
			counter.Stop();
			theSum = sum;
			return new Result("Delegate", iterations, counter.Seconds, setupTime);
		}

		public Result CallInvoke(Processor processor, int iterations)
		{
			int sum = 0;
			Counter counter = new Counter();
			counter.Start();
			Type t = typeof(Processor);
			counter.Stop();
			float setupTime = counter.Seconds;

			counter.Clear();
			counter.Start();
			object[] parameters = new object[1];
			for (int i = 0; i < iterations; i++)
			{
				parameters[0] = i;
				sum += (int) t.InvokeMember(
					"Process", 
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, 
					null, processor, parameters);
			}
			counter.Stop();
			theSum = sum;
			return new Result("InvokeMember", iterations, counter.Seconds, setupTime);
		}

		public Result CallMethodInfo(Processor processor, int iterations)
		{
			int sum = 0;
			MethodInfo m = processor.GetType().GetMethod("Process");
			
			Counter counter = new Counter();
			counter.Start();
			object[] parameters = new object[1];
			for (int i = 0; i < iterations; i++)
			{
				parameters[0] = i;
				sum += (int)m.Invoke(processor, parameters);	
			}
			counter.Stop();
			theSum = sum;
			return new Result("MethodInfo", iterations, counter.Seconds, 0.0F);
		}


		public Result CallCustomDelegate(Processor process, int iterations)
		{
			int sum = 0;
			Counter counter = new Counter();
			counter.Start();
			Type t = typeof(Processor);
			MethodInfo methodInfo = t.GetMethod("Process");

			Type delegateType = CreateCustomDelegate(methodInfo);

			Delegate p = Delegate.CreateDelegate(delegateType, process, "Process");
			counter.Stop();
			float setupTime = counter.Seconds;

			counter.Clear();
			counter.Start();
			object[] parameters = new object[1];
			for (int i = 0; i < iterations; i++)
			{
				parameters[0] = i;
				sum += (int) p.DynamicInvoke(parameters);
			}
			counter.Stop();
			theSum = sum;
			return new Result("CustomDelegate", iterations, counter.Seconds, setupTime);
		}

		public Result CallCustomClass(Processor process, int iterations)
		{
			int sum = 0;
			Counter counter = new Counter();
			counter.Start();
			Type t = typeof(Processor);
			MethodInfo methodInfo = t.GetMethod("Process");

			Type customType = CreateCustomClass(t, methodInfo);

			IProcessor processorInterface = (IProcessor)
				Activator.CreateInstance(customType, new object[] {process});

			counter.Stop();
			float setupTime = counter.Seconds;

			counter.Clear();
			counter.Start();
			for (int i = 0; i < iterations; i++)
			{
				sum += processorInterface.Process(i);	
			}
			counter.Stop();
			theSum = sum;
			return new Result("CustomClass", iterations, counter.Seconds, setupTime);
		}

		public Type CreateCustomDelegate(MethodInfo targetMethod)
		{
			AssemblyName assembly;
			AssemblyBuilder assemblyBuilder;
			ModuleBuilder modbuilder;
			TypeBuilder typeBuilder;
			MethodBuilder methodBuilder;

			assembly = new AssemblyName();
			assembly.Version = new Version(1, 0, 0, 0);
			assembly.Name = "ReflectionEmitDelegateTest";
			
			assemblyBuilder = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);

			//assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.RunAndSave);
			modbuilder = assemblyBuilder.DefineDynamicModule("MyModule");
			//modbuilder = assemblyBuilder.DefineDynamicModule("MyModule", "mydelegatetest.exe", true);

			// Create a delegate that has the same signature as the method we would like to hook up to
			typeBuilder = modbuilder.DefineType("MyDelegateType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass, typeof(System.MulticastDelegate));
			ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object), typeof(int) });
			constructorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
			
			// Grab the parameters of the method
			ParameterInfo[] parameters = targetMethod.GetParameters();
			Type[] paramTypes = new Type[parameters.Length];

			for (int i = 0; i < parameters.Length; i++)
			{
				paramTypes[i] = parameters[i].ParameterType;
			}

			// Define the Invoke method for the delegate
			methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, targetMethod.ReturnType, paramTypes);
			methodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

			// bake it!
			Type t = typeBuilder.CreateType();
			// assemblyBuilder.Save("mydelegatetest.exe"); // useful for testing			

			return t;
		}

			// Create custom type that implements the interface, use it to 
			// call through to the real instance. This is assuming that the
			// type doesn't already implement the interface directly (though
			// in this example, it does...)
		public Type CreateCustomClass(Type classType, MethodInfo wrappedMethod)
		{
			ILGenerator il;
			MethodBuilder simpleMethod;

			// Create an assembly name
			//
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = "CustomClass";

			//
			// Create a new assembly with one module
			//
			
			AssemblyBuilder newAssembly = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			//AssemblyBuilder newAssembly = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder newModule = newAssembly.DefineDynamicModule("CustomClass");

			//		
			//  Define a public class named "CustomClass" in the assembly.
			//			
			TypeBuilder myType = 
				newModule.DefineType("CustomClass", TypeAttributes.Public);

			//
			// Mark the class as implementing IProcessor. This is
			// the first step in that process.
			//
			myType.AddInterfaceImplementation(typeof(IProcessor));

			//
			// Add a field to hold the wrapped type
			//
			FieldInfo wrappedField = myType.DefineField("wrapped", classType, FieldAttributes.Private);

			//
			// Add a constructor that takes the wrapped type as a parameter,
			// and write the IL for it. It calls the base class constructor, 
			// and then stores the parameter into the field. 
			//
			ConstructorBuilder constructor = 
				myType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
									     new Type[] {classType});
			ILGenerator constructorMethod =
				constructor.GetILGenerator();

			constructorMethod.Emit(OpCodes.Ldarg_0);

				// object constructor
			ConstructorInfo objectConstructor = typeof(object).GetConstructor(new Type[] {});
			constructorMethod.Emit(OpCodes.Call, objectConstructor);

				// store the field away...
			constructorMethod.Emit(OpCodes.Ldarg_0);
			constructorMethod.Emit(OpCodes.Ldarg_1);
			constructorMethod.Emit(OpCodes.Stfld, wrappedField);

			constructorMethod.Emit(OpCodes.Ret);
			//
			// Define a method on the type to call. We pass an
			// array that defines the types of the parameters, 
			// the type of the return type, the name of the method,
			// and the method attributes.
			//
			simpleMethod = 
				myType.DefineMethod("Process", 
				MethodAttributes.Public | MethodAttributes.Virtual, 
				typeof(int), 
				new Type[] {typeof(int)});

			//
			// From the method, get an ILGenerator. This is used to
			// emit the IL that we want.
			//
			il = simpleMethod.GetILGenerator();

			//
			// Emit the IL. This is a hand-coded version of what
			// you'd get if you compiled the code example and then ran
			// ILDASM on the output.
			//

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, wrappedField);
			il.Emit(OpCodes.Ldarg_1);
			il.EmitCall(OpCodes.Callvirt, wrappedMethod, null);
			il.Emit(OpCodes.Ret);

			//
			// Hook up the interface member to the member function
			// that implements that member.
			// 1) Get the interface member.
			// 2) Get the type of the new class that was created
			// 3) Get the member of the class that does the evaluation
			// 4) Hook that member to the interface member.
			//
			MethodInfo methodInterfaceEval = typeof(IProcessor).GetMethod("Process");

			myType.DefineMethodOverride(simpleMethod, methodInterfaceEval);
			Type returnType = myType.CreateType();
			//newAssembly.Save("wrapper.dll"); // for testing

			return returnType;
		}
			
		public ArrayList Call(int iterations)
		{
			ArrayList results = new ArrayList();

			Processor processor = new Processor();
			Processor2 processor2 = new Processor2();

			results.Add(CallDirect(processor, iterations));
			results.Add(CallInvoke(processor, iterations));
			results.Add(CallInterface(processor, iterations));
			results.Add(CallDelegate(processor, iterations));
			results.Add(CallCustomDelegate(processor, iterations));
			results.Add(CallCustomClass(processor, iterations));
			results.Add(CallDirectVirtual(processor2, iterations));
			results.Add(CallInterfaceVirtual(processor2, iterations));
			results.Add(CallDirectStatic(iterations));
			results.Add(CallMethodInfo(processor, iterations));

			return results;
		}
	}
}
