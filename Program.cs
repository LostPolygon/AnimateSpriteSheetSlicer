using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using log4net;
using log4net.Config;

namespace AnimateSpriteSheetSlicer
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("ConsoleSlicer");

        public static void Main(params string[] args) {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Console.OutputEncoding = Encoding.Unicode;
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
#endif
            SetupLog4Net();

            ConsoleSlicer.Run(args);
        }

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e) {
            Log.Fatal(
                "Fatal error:" + Environment.NewLine +
                ((Exception) e.ExceptionObject) + Environment.NewLine +
                ((Exception) e.ExceptionObject).InnerException
            );
            Thread.Sleep(3000);
            Environment.Exit(1);
        }

        private static void SetupLog4Net() {
            XmlDocument objDocument = new XmlDocument();
            objDocument.LoadXml(Resources.log4netConfiguration);
            XmlElement objElement = objDocument.DocumentElement;

            XmlConfigurator.Configure(objElement);
        }
    }
}
