using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace General.CDN.Core.Tests
{
    [TestClass]
    public class UnitTestContext
    {
        static IReadOnlyDictionary<string, string> DefaultSettings { get; } =
         new Dictionary<string, string>()
         {
             [$"AzureStorageConnectionString"] = "DefaultEndpointsProtocol=https;AccountName=tradeshowtoolkit;AccountKey=NScWwpZfWanIiyjg0QMIAoPokRRVLJeA2DGVhaLNJej6fIdiFASR6YZBdPuPTsN8jhrpRQ4tpXyMXjvP6vAlFw==",
             [$"AzureBucket"] = "unittest"
         };


        public static string AzureStorageConnectionString
        {
            get
            {
                return UnitTestContext.Configuration["AzureStorageConnectionString"];
            }
        }

        public static string AzureBucket
        {
            get
            {
                return UnitTestContext.Configuration["AzureBucket"];
            }
        }

        public static IConfiguration Configuration { get; set; }
        /// <summary>
        /// This method will run before any unit test, and guarantee that a fresh client DB + master DB is ready for the test to run on.
        /// </summary>
        /// <param name="context"></param>
        [AssemblyInitialize]
        public static void Startup(TestContext context)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(
              DefaultSettings);
            Configuration = configurationBuilder.Build();
        }

    }
}
