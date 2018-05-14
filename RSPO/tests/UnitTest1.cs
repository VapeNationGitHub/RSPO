using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Nancy;
using Nancy.Testing;

namespace RSPO.tests
{
	public class UnitTest1
	{
		private string basePath;

		public UnitTest1()
		{
			basePath =
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

			Application.InitializeEntityContext();
		}

		[Fact]
		public void PassingTest()
		{
			Assert.Equal(4, Add(2, 2));
		}

		[Fact]
		public void FailingTestThatPasses()
		{
			Assert.NotEqual(5, Add(2, 2));
		}

		int Add(int x, int y)
		{
			return x + y;
		}

		[Theory]
		[InlineData(3)]
		[InlineData(5)]
		public void MyFirstTheory(int value)
		{
			Assert.True(IsOdd(value));
		}

		bool IsOdd(int value)
		{
			return value % 2 == 1;
		}

		[Theory]
		[InlineData("all.xml")]
		[InlineData("all.xml.zip")]
		public void ImportTest(string importName)
		{
			string fileName = CombineWithDataPath(importName);

			ImportFromAtlcomru import = new ImportFromAtlcomru()
			{
				FileName = fileName
			};
			import.Import(onlyLoad: true);
			Assert.True(true);
		}

		private string CombineWithDataPath(string importName)
		{
			string dataDir = Path.Combine(basePath, "DATA");
			dataDir = Path.Combine(dataDir, "Import");
			string fileName = Path.Combine(dataDir, importName);
			return fileName;
		}

		/*
        [Theory]
        [InlineData("all.xml")]
        public void LongImportTest(string importName)
        {
            string fileName = CombineWithDataPath(importName);
            MyEntityContext ctx = Application.Context;
            ImportFromAtlcomru import = new ImportFromAtlcomru()
            {
                FileName = fileName
            };
            import.Import();
            Assert.True(true);
        }
        */

		[Fact]
		public void Should_return_status_ok_when_route_exists()
		{
			// Given
			var bootstrapper = new DefaultNancyBootstrapper();
			var browser = new Browser(bootstrapper);

			// When
			var result = browser.Get("/", with =>
			{
				with.HttpRequest();
			});

			// Then
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
		}

		[Fact]
		public void TestRenderer()
		{
			WebModule mod = new WebModule();
			Request request = new Request("GET", "", "1.1");
			string result = mod.Render("index.pt", request: request);
			Console.WriteLine(result);
			Assert.Contains(result, "</html>");
		}
	}
}
