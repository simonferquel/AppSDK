using System;

namespace Docker.AppFrontend
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandParser = new CommandParser(new RootCommand());
            commandParser.ParseAndExecute(args);
        }
    }
}
