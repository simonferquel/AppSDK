using System;
using System.Collections.Generic;
using System.Text;

namespace Docker.AppFrontend
{
    class SelectCommand : ICommand
    {
        private string _appName;

        public string Name => "app";
        public string Description => "set the app to work with";

        public void Execute() => throw new NotImplementedException();
        public void PreExecute()
        {
            AppRegistry.Select(_appName);
        }

        public IEnumerable<ICommand> SubCommands
        {
            get
            {
                yield return new ParametersCommand();
            }
        }
        public IEnumerable<Flag> PositionalArguments
        {
            get
            {
                yield return new Flag("app-name", n => _appName = n, description: "name of the application", mandatory: true);

            }
        }
        public IEnumerable<Flag> NamedFlags => null;
    }
}
