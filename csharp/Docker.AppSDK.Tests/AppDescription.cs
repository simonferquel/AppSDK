using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Docker.AppSDK.Tests
{
    public class AppDescription
    {
        class AppWithoutAttribute : IApp {
            public Task Build(IAppBuilder builder) => Task.CompletedTask;
        }

        [App("test-app", "1.1.1.1", Author = "tester", Description ="test description")]
        class AppWithAttribute : IApp {

            public string NotAParam { get; set; }

            [Parameter("mandatory", Mandatory = true)]
            public string MandatoryParam { get; set; }

            [Parameter("non-mandatory")]
            public int NonMandatoryParam { get; set; }

            [Dependency]
            public AppWithoutAttribute Dep { get; } = new AppWithoutAttribute();

            public Task Build(IAppBuilder builder) => Task.CompletedTask;
        }

        [Fact]
        public void AppWithoutAttributeHasCorrectDescription()
        {
            var analyzer = new AppAnalyzer(new AppWithoutAttribute());
            Assert.Equal("AppWithoutAttribute", analyzer.Name);
            Assert.Equal(new Version(0, 0, 0, 0), analyzer.Version);
            Assert.Null(analyzer.Author);
            Assert.Null(analyzer.Description);
        }
        [Fact]
        public void AppWithAttributeHasCorrectDescription()
        {
            var app = new AppWithAttribute();
            var analyzer = new AppAnalyzer(app);
            Assert.Equal("test-app", analyzer.Name);
            Assert.Equal(new Version(1, 1, 1, 1), analyzer.Version);
            Assert.Equal("tester", analyzer.Author);
            Assert.Equal("test description", analyzer.Description);
            Assert.Equal(2, analyzer.Parameters.Count());
            Assert.NotNull(analyzer.GetParameter("mandatory"));
            Assert.True(analyzer.GetParameter("mandatory").Mandatory);
            Assert.False(analyzer.GetParameter("mandatory").IsSet);
            Assert.NotNull(analyzer.GetParameter("non-mandatory"));
            Assert.False(analyzer.GetParameter("non-mandatory").Mandatory);

            analyzer.GetParameter("mandatory").Set("test value");
            Assert.True(analyzer.GetParameter("mandatory").IsSet);
            Assert.Equal("test value", app.MandatoryParam);
            analyzer.GetParameter("non-mandatory").Set("42");
            Assert.Equal(42, app.NonMandatoryParam);

            Assert.Single(analyzer.Dependencies);
            Assert.NotNull(analyzer.GetDependency("Dep"));
        }
    }
}
