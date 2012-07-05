using System.Collections.Generic;
using Mono.Debugger.Cli.Debugging;
using Mono.Debugger.Cli.Logging;

namespace Mono.Debugger.Cli.Commands
{
    internal abstract class BaseStepCommand : ICommand
    {
        public string Description
        {
            get { return "Steps into an instruction"; }
        }

        public string Arguments
        {
            get { return ""; }
        }

        public void Execute(CommandArguments args)
        {
            var op = args.NextString();

            if (SoftDebugger.State == DebuggerState.Null)
            {
                Logger.WriteErrorLine("No session active.");
                return;
            }

            if (SoftDebugger.State == DebuggerState.Initialized)
            {
                Logger.WriteErrorLine("No process active.");
                return;
            }

            if (SoftDebugger.State == DebuggerState.Running)
            {
                Logger.WriteErrorLine("Process is running.");
                return;
            }

	    DoStep (SoftDebugger.Session);
	}

	protected abstract void DoStep (SoftDebuggerCliSession session);
    }

    internal class StepCommand : BaseStepCommand {
	    protected override void DoStep (SoftDebuggerCliSession session)
	    {
		    session.NextLine ();
	    }
    }

    internal class StepInstructionCommand : BaseStepCommand {
	    protected override void DoStep (SoftDebuggerCliSession session)
	    {
		    session.NextInstruction ();
	    }
    }

    internal class NextCommand : BaseStepCommand {
	    protected override void DoStep (SoftDebuggerCliSession session)
	    {
		    session.StepLine ();
	    }
    }

    internal class NextInstructionCommand : BaseStepCommand {
	    protected override void DoStep (SoftDebuggerCliSession session)
	    {
		    session.NextInstruction ();
	    }
    }
    
    internal class FinishCommand : BaseStepCommand {
	    protected override void DoStep (SoftDebuggerCliSession session)
	    {
		    session.Finish ();
	    }
    }
}
