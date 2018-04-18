using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Docker.AppFrontend
{
    interface ICommand
    {
        string Name { get; }
        string Description { get; }
        void Execute();
        void PreExecute();
        IEnumerable<ICommand> SubCommands { get; }
        IEnumerable<Flag> PositionalArguments { get; }
        IEnumerable<Flag> NamedFlags { get; }

    }

    class HelpPrintedException : Exception
    {
        public HelpPrintedException(Exception e) : base(e.Message, e) { }
    }

    class CommandParser
    {
        private readonly ICommand _rootCommand;

        public CommandParser(ICommand rootCommand)
        {
            _rootCommand = rootCommand;
        }

        public void ParseAndExecute(string[] args)
        {
            ParseAndExecute(_rootCommand, args, new string[] { });
        }

        private void ParseAndExecute(ICommand command, string[] args, string[] consumed)
        {
            IList<Flag> positionalArgs = command.PositionalArguments?.ToList() ?? new List<Flag>();
            try {
                for (int ix = 0; ix < args.Length; ++ix) {
                    if (args[ix].StartsWith("-")) {
                        // named flag
                        (var name, var value, var isShortHand, var printHelp) = ParseNamedFlag(args, ref ix);
                        if (printHelp) {
                            PrintHelp(command, consumed);
                            return;
                        }
                        var flag = FindFlag(command, name, isShortHand);
                        if (flag == null) {
                            throw new FlagNotFoundException(isShortHand ? $"-{name}" : $"--{name}");
                        }
                        if (flag.Value.IsSwitch && string.IsNullOrEmpty(value)) {
                            value = "true";
                        }
                        flag.Value.OnValueSet(value);
                    } else {
                        if (positionalArgs.Count > 0) {
                            positionalArgs[0].OnValueSet(args[ix]);
                            positionalArgs = positionalArgs.Skip(1).ToList();
                        } else {
                            var subCommands = command.SubCommands;
                            if (subCommands == null) {
                                throw new SubcommandNotFoundException(args[ix]);
                            }
                            var sc = subCommands.Where(s => s.Name == args[ix]).FirstOrDefault();
                            if (sc == null) {
                                throw new SubcommandNotFoundException(args[ix]);
                            }
                            command.PreExecute();
                            ParseAndExecute(sc, args.Skip(ix + 1).ToArray(), consumed.Concat(args.Take(ix + 1)).ToArray());
                            return;
                        }
                    }
                }
                command.Execute();
            } catch (HelpPrintedException) {

            } catch (Exception e) {
                PrintHelp(command, consumed);
                throw new HelpPrintedException(e);
            }
        }

        private void PrintHelp(ICommand command, string[] consumed)
        {
            var tokens = new List<string> { AppDomain.CurrentDomain.FriendlyName };
            if (consumed != null) {
                tokens.AddRange(consumed);
            }

            if (command.NamedFlags?.Any() ?? false) {
                tokens.Add("<flags>");
            }

            foreach (var positional in command.PositionalArguments ?? Enumerable.Empty<Flag>()) {
                if (positional.Mandatory) {
                    tokens.Add($"<{positional.Name}>");
                } else {
                    tokens.Add($"[{positional.Name}]");
                }
            }

            if (command.SubCommands?.Any() ?? false) {
                tokens.Add("<subcommand>");
            }


            Console.WriteLine($"{string.Join(" ", tokens)}:");
            Console.WriteLine(command.Description);
            Console.WriteLine();

            if (command.SubCommands?.Any() ?? false) {
                Console.WriteLine("Subcommands:");
                var formatter = new TableFormatter<ICommand>(
                    new ColumnDefinition<ICommand>("  NAME", c => "  "+ c.Name),
                    new ColumnDefinition<ICommand>("DESCRIPTION", c => c.Description));
                formatter.Print(command.SubCommands.ToList(), Console.Out);
                Console.WriteLine();
            }
            if (command.NamedFlags?.Any() ?? false) {
                Console.WriteLine("Flags:");
                var formatter = new TableFormatter<Flag>(
                    new ColumnDefinition<Flag>("  NAME", c => "  " + c.NameAndShortHand),
                    new ColumnDefinition<Flag>("DESCRIPTION", c => c.Description));
                formatter.Print(command.NamedFlags.ToList(), Console.Out);
                Console.WriteLine();
            }

        }

        Flag? FindFlag(ICommand command, string name, bool isShortHand)
        {
            if (isShortHand) {
                foreach (var f in command.NamedFlags.Where(flag => flag.HasShortHand && flag.ShortHand == name[0])) {
                    return f;
                }
                return null;
            }
            foreach (var f in command.NamedFlags.Where(flag => flag.Name == name)) {
                return f;
            }
            return null;
        }

        (string name, string value, bool isShortHand, bool printHelp) ParseNamedFlag(string[] args, ref int ix)
        {
            var nameToken = args[ix];
            if (nameToken == "--help") {
                return ("", "", false, true);
            }
            var isShortHand = false;
            var name = nameToken.Substring(2);
            if (!nameToken.StartsWith("--")) {
                isShortHand = true;
                name = nameToken.Substring(1, 1);
                if (nameToken.Length > 2) {
                    return (name, nameToken.Substring(2), true, false);
                }
            } else {
                int indexOfEqual = name.IndexOf('=');
                if (indexOfEqual != -1) {
                    return (name.Substring(0, indexOfEqual), Unquote(name.Substring(indexOfEqual + 1)), isShortHand, false);
                }
            }
            ++ix;
            if (args.Length <= ix) {
                return (name, "", isShortHand, false);
            }
            if (args[ix].StartsWith("-")) {
                // next flag
                --ix;
                return (name, "", isShortHand, false);
            }
            return (name, Unquote(args[ix]), isShortHand, false);
        }

        private string Unquote(string v)
        {
            if (v == null) {
                return null;
            }
            if (v.Length < 2) {
                return v;
            }
            var quoteChars = new char[] { '\'', '\"' };
            foreach (var c in quoteChars) {
                if (v[0] == c && v[v.Length - 1] == c) {
                    return v.Substring(1, v.Length - 2);
                }
            }
            return v;
        }
    }
}
