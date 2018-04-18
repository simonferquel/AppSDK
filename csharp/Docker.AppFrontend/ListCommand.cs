using Docker.AppSDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Docker.AppFrontend
{
    class ListCommand :ICommand
    {
        public string Name => "list";

        public void Execute()
        {
            var apps = AppRegistry.List();
            var formatter = new TableFormatter<(string[] name, AppAnalyzer app)>(
                new ColumnDefinition<(string[] name, AppAnalyzer app)>("NAME", v => string.Join(", ", v.name)),
                new ColumnDefinition<(string[] name, AppAnalyzer app)>("VERSION", v => v.app.Version.ToString()),
                new ColumnDefinition<(string[] name, AppAnalyzer app)>("DESCRIPTION", v => v.app.Description)
                );
            formatter.Print(apps, Console.Out);
        }
        public void PreExecute() { }

        public IEnumerable<ICommand> SubCommands => null;
        public IEnumerable<Flag> PositionalArguments => null;
        public IEnumerable<Flag> NamedFlags => null;

        public string Description => "List available apps";
    }
}
