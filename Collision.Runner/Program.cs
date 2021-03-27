using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ArcGIS.Core.Hosting;
using Collision.Python;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Collision.Runner
{
    class Program
    {
        private static ILogger s_Logger;

        [STAThread]
        static void Main(string[] args)
        {
            
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddNLog();
            });
            s_Logger = loggerFactory.CreateLogger<Program>();
            
            // the license line bellow will cause to throw exceptions when uncommented
            // Host.Initialize();
            
            Run();
            
        }

        private static void Run()
        {
            
            using var pythonScripts =  new PythonScripts(s_Logger);
            try
            {
                var errors = new List<Error>();
                errors.AddRange(pythonScripts.CreateCity("test", 32733).Errors);
                if (errors.Any())
                {
                    s_Logger.LogCritical($"Run complete with errors above. Messages:\n{string.Join(",", errors)}");
                }
            }
            catch (Exception e)
            {
                s_Logger.LogError(e, e.Message);
            }
            finally
            {
                pythonScripts.Dispose();
                s_Logger.LogInformation("Done");
            }
        }
    }
}
