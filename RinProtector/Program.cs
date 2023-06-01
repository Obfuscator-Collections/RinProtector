using dnlib.DotNet;
using KVM;
using RinProtector.Protections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KoiVM.Console
{
    internal class Program
    {
		private static void Main(string[] args)
		{

			bool flag2 = args.Length == 1;
			if (flag2)
			{
				string directoryName = Path.GetDirectoryName(args[0]);
		
				ModuleDef moduleDef = ModuleDefMD.Load(args[0]);

				System.Console.Title = "Rin Protector - //lexy#8726//";

				Program.ExceuteKoi(args[0], directoryName + "//Rin//" + Path.GetFileNameWithoutExtension(moduleDef.Name) + "-RinProtected.exe", "", null);
			}
			
			Process.GetCurrentProcess().Kill();
		}

	
		private static void ExceuteKoi(string input, string outPath, string snPath, string snPass)
		{
			
			try
			{
				bool flag = !Directory.Exists(Path.GetDirectoryName(input) + "//Rin");
				if (flag)
				{
					Directory.CreateDirectory(Path.GetDirectoryName(input) + "//Rin");
				}
				new KVMTask().VMAndOtherProtections(ModuleDefMD.Load(input), outPath, snPath, snPass);
				System.Console.ForegroundColor = ConsoleColor.Green;
				System.Console.WriteLine("Protected All Methods!");
				Thread.Sleep(1500);
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
	}
}
