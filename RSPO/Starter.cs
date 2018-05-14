using System;
using System.IO;
using Mono.Unix;
using Mono.Unix.Native;
using Nancy;
using Nancy.Hosting.Self;

namespace RSPO
{
	public partial class Application
	{
		static public MyEntityContext Context = null;
		static public string DESIGN_DIR = Path.Combine("design-studio_one-page-template", "build");
		static public string TEMPLATE_LOCATION = null;
		static public string STATIC_DIR = null;

		static string ConnectionString = "type=embedded;storesdirectory=./;storename=RSPO";

		[STAThread]
		static void Main(string[] args)
		{
			InitializeEntityContext();
			InitializeTemplating();

			var uri = "http://localhost:8888";
			Console.WriteLine("Starting Nancy on " + uri + "\n Ctrl-C to Stop.");

			// initialize an instance of NancyHost
			HostConfiguration hostConfigs = new HostConfiguration();
			hostConfigs.UrlReservations.CreateAutomatically = true;
			var host = new NancyHost(new Uri(uri),
				new Bootstrapper(),
				hostConfigs);


			host.Start();  // start hosting

			RunWithoutInterface();

			Console.WriteLine("Stopping Nancy");
			host.Stop();  // stop hosting
		}

		public static void InitializeEntityContext()
		{
			Context = new MyEntityContext(ConnectionString);
		}


		public static void InitializeTemplating()
		{
			string basePath =
			Path.GetDirectoryName(
				Path.GetDirectoryName(
					Path.GetDirectoryName(
						Path.GetDirectoryName(
							System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))));

			if (Type.GetType("Mono.Runtime") != null)
			{
				basePath = basePath.Replace("file:", "");
			}
			else
			{
				basePath = basePath.Replace("file:\\", "");
			}

			TEMPLATE_LOCATION = Path.Combine(basePath, DESIGN_DIR);
			Console.WriteLine("Templates are at " + TEMPLATE_LOCATION);
			STATIC_DIR = Path.Combine(TEMPLATE_LOCATION, "static");

		}


		public static string GenerateHash(string input)
		{
			return input;
		}

		private static void RunWithoutInterface()
		{
			// check if we're running on mono
			if (Type.GetType("Mono.Runtime") != null)
			{
				// on mono, processes will usually run as daemons - this allows you to listen
				// for termination signals (ctrl+c, shutdown, etc) and finalize correctly
				UnixSignal.WaitAny(new[] {
					new UnixSignal(Signum.SIGINT),
					new UnixSignal(Signum.SIGTERM),
					new UnixSignal(Signum.SIGQUIT),
					new UnixSignal(Signum.SIGHUP)
				});
			}
			else
			{
				Console.ReadLine();
			}
		}
	}
}
