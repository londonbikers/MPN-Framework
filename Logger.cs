using System;
using System.Reflection;
using log4net;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace MPN.Framework
{
	internal class Logger
	{
		#region members
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region public methods
		public static void LogDebug(string message)
		{
			Logger.Log.Debug(message);
		}

		public static void LogInfo(string message)
		{
			Logger.Log.Info(message);
		}

		public static void LogWarning(string warning)
		{
			Logger.Log.Warn(warning);
		}

		public static void LogWarning(string warning, Exception exception)
		{
			Logger.Log.Warn(warning, exception);
		}

		public static void LogError(string message, Exception exception)
		{
			Logger.Log.Error(message, exception);
		}

		public static void LogError(string message)
		{
			Logger.Log.Error(message);
		}
		#endregion
	}
}