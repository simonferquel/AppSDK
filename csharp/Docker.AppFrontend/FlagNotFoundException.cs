using System;
using System.Collections.Generic;
using System.Text;

namespace Docker.AppFrontend
{
    class FlagNotFoundException : Exception
    {
        public FlagNotFoundException(string flagName) : base($"Flag \"{flagName}\" not found")
        {
            FlagName = flagName;
        }

        public string FlagName { get; }
    }

    class SubcommandNotFoundException : Exception
    {
        public SubcommandNotFoundException(string name) : base($"Subcommand \"{name}\" not found")
        {
            SubcommandName = name;
        }

        public string SubcommandName { get; }
    }
}
