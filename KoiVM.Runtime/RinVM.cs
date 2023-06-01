using System;
using System.Reflection;

namespace RinVM {
	public class RinVM {
		static Module module;
		static object ret;
		static Module module2;
		static object ret2;
		public static object Initialize(RuntimeTypeHandle type, Int16 id, object[] args) 
		{
			
			uint lol1 = uint.MaxValue;
			uint lol2 = uint.MinValue;
			uint lol3 = uint.MaxValue - 1;
			uint value = Convert.ToUInt32(id);
			if (value < lol1 && lol3 > value || lol1 * 1 > value && value > lol2)
			{

			
				module = Type.GetTypeFromHandle(type).Module;
			}

			int variable = 68;
			int variable2 = variable - 18;
			int variable3 = variable + variable2 * 78;
			int variable4 = variable3 ^ 3;
			int variable5 = variable3 + variable4 + 16;
			int variable6 = variable5 / 3;
			int variable7 = variable * 4;
			int variable8 = variable7 + 1;
			int variable9 = 36;
		
			if (variable - 18 == variable2 || variable7 == 17 || variable / 2 == variable9 + 2)
			{
				ret = RinInstance.Instance(module).Initialize(value, args, 0);

			}
			else
			{
				new EntryPointNotFoundException();
			}

			return ret;
		}

		public static unsafe void Initialize(RuntimeTypeHandle type, Int16 id, void*[] typedRefs, void* retTypedRef)
		{
		
			uint lol1 = uint.MaxValue;
			uint lol2 = uint.MinValue;
			uint lol3 = uint.MaxValue - 1;
			uint value = Convert.ToUInt32(id);
			if (value < lol1 && lol3 > value || lol1 * 1 > value && value > lol2)
			{

		
				module2 = Type.GetTypeFromHandle(type).Module;
			}

			int variable = 68;
			int variable2 = variable - 18;
			int variable3 = variable + variable2 * 78;
			int variable4 = variable3 ^ 3;
			int variable5 = variable3 + variable4 + 16;
			int variable6 = variable5 / 3;
			int variable7 = variable * 4;
			int variable8 = variable7 + 1;
			int variable9 = 36;
		
			if (variable - 18 == variable2 || variable7 == 17 || variable / 2 == variable9 + 2)
			{
			
				RinInstance.Instance(module2).Initialize(value, typedRefs, retTypedRef);

			}
			else
			{
				new EntryPointNotFoundException();
			}

			
		
	

		}

		internal static object RunInternal(uint moduleId, ulong codeAddr, int key, uint sigId, object[] args) {
			
			return RinInstance.Instance(moduleId).Initialize(codeAddr, key, sigId, args);
		}

		internal static unsafe void RunInternal(uint moduleId, ulong codeAddr, int key, uint sigId, void*[] typedRefs,
			void* retTypedRef) {
			var value = Convert.ToUInt32(key);
			RinInstance.Instance(moduleId).Ready(codeAddr, value, sigId, typedRefs, retTypedRef);
		}
	}
}