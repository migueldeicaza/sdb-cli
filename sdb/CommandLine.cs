using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mono.Debugger.Cli.Commands;
using Mono.Debugger.Cli.Debugging;
using Mono.Debugger.Cli.Logging;
using Mono.Debugger.Soft;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using Mono.Terminal;

namespace Mono.Debugger.Cli
{
    public static class CommandLine
    {
        internal static bool Stopped { get; set; }

        internal static bool Suspended { get; set; }

        internal static CommandDialect Dialect { get; private set; }

        internal static AutoResetEvent ResumeEvent { get; private set; }

        static CommandLine()
        {
            ResumeEvent = new AutoResetEvent(false);

	    InitializeGdbDialect();
        }

        private static void InitializeGdbDialect()
        {
            Dialect = new CommandDialect(CommandDialect.Gdb, new Dictionary<string, ICommand>
            {
                { "help", new HelpCommand() },
                { "quit", new ExitCommand() },
                { "log", new LogCommand() },
                { "cd", new CurrentDirectoryCommand() },
                { "init", new InitializeCommand() },
                { "run", new StartCommand() },
                { "continue", new ContinueCommand() },
                { "cont", new ContinueCommand() },
                { "c", new ContinueCommand() },
                { "step", new StepCommand() },
                { "s", new StepCommand() },
                { "next", new NextCommand() },
                { "n", new NextCommand() },
                { "stepi", new StepInstructionCommand() },
                { "si", new StepInstructionCommand() },
                { "nexti", new NextInstructionCommand() },
                { "ni", new NextInstructionCommand() },
		{ "f", new FinishCommand () },
		{ "finish", new FinishCommand () },
                { "stop", new StopCommand() },
                { "b", new BreakpointCommand() },
                { "break", new BreakpointCommand() },
                { "catch", new CatchpointCommand() },
                { "db", new DatabaseCommand() },
                { "fc", new FirstChanceCommand() },
                { "backtrace", new BacktraceCommand() },
                { "bt", new BacktraceCommand() },
                { "frame", new FrameCommand() },
                { "disassemble", new DisassembleCommand() },
                { "disas", new DisassembleCommand() },
                { "source", new SourceCommand() },
                { "decompile", new DecompileCommand() },
                { "locals", new LocalsCommand() },
                { "print", new EvaluationCommand() },
                { "p", new EvaluationCommand() },
                { "watch", new WatchCommand() },
                { "thread", new ThreadCommand() },
            });
        }

        internal static void CommandLoop()
        {
            Logger.WriteInfoLine("Welcome to the Mono Soft Debugger CLI!");
            Logger.WriteInfoLine("Using {0} and {1} with features: {2}", typeof(VirtualMachine).Assembly.GetName().Name,
                typeof(SoftDebuggerSession).Assembly.GetName().Name, SoftDebugger.Features);
            Logger.WriteInfoLine("Type \"Help\" for a list of commands or \"Exit\" to quit.");

	    var lineEditor = new LineEditor ("sdb");
            string line;
	    
            while (true)
            {
                line = lineEditor.Edit ("sdb> ", "");
		if (line == null){
			if (SoftDebugger.State != DebuggerState.Null && SoftDebugger.State != DebuggerState.Initialized){
				var answer = lineEditor.Edit ("Do you really want to quit? (y/n) ", "");
				if (answer == null || answer.ToLower ().StartsWith ("y"))
					break;
			} else
				break;
		}

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var fullCmd = line.Split(' ');
                var cmd = fullCmd[0];
                var command = Dialect.Commands.SingleOrDefault(x => x.Key.Equals(cmd, StringComparison.OrdinalIgnoreCase)).Value;

                if (command == null)
                {
                    Logger.WriteErrorLine("No such command: {0}", cmd);
                    continue;
                }

                try
                {
                    command.Execute(new CommandArguments(fullCmd.Skip(1)));
                }
                catch (Exception ex)
                {
                    Logger.WriteErrorLine("Error executing command:", cmd);
                    Logger.WriteErrorLine(ex.Message);

                    if (!(ex is CommandArgumentException))
                        Logger.WriteErrorLine(ex.StackTrace);
                }

                if (Suspended)
                {
                    ResumeEvent.WaitOne();
                    Suspended = false;
                }

                if (Stopped)
                    break;
            }

            if (SoftDebugger.State != DebuggerState.Null)
                SoftDebugger.Stop();

            if (Logger.LogOutput != null)
            {
                Logger.LogOutput.Dispose();
                Logger.LogOutput = null;
            }
        }
    }
}
