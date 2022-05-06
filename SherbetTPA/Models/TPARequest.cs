using System;
using System.Threading.Tasks;
using RocketExtensions.Models;

namespace SherbetTPA.Models
{
    public class TPARequest
    {
        public CommandContext Context { get; }
        public LDMPlayer From { get; }
        public LDMPlayer To { get; }

        public DateTime Requested { get; }

        public DateTime Expires { get; }

        public DateTime TeleportStarted { get; set; }

        public ETPAState State { get; set; }

        public int TeleportTime { get; }

        public TimeSpan TeleportTimeRemaining => TeleportStarted.AddSeconds(TeleportTime) - DateTime.Now;

        public TPARequest(CommandContext ctx, LDMPlayer to, int expires, int teleportTime)
        {
            Context = ctx;
            From = ctx.LDMPlayer;
            To = to;
            Requested = DateTime.Now;
            Expires = DateTime.Now.AddSeconds(expires);
            State = ETPAState.Waiting_Accept;
            TeleportTime = teleportTime;
            TeleportStarted = DateTime.Now;
        }

        private TaskCompletionSource<ETPAState> AcceptWaiter { get; } = new TaskCompletionSource<ETPAState>();
        private TaskCompletionSource<ETPAState> TeleportWaiter { get; } = new TaskCompletionSource<ETPAState>();

        public void ReleaseAcceptWaiter(ETPAState state)
        {
            if (!AcceptWaiter.Task.IsCompleted)
            {
                State = state;
                AcceptWaiter.SetResult(state);
            }
        }

        public void ReleaseTeleportWaiter(ETPAState state)
        {
            if (!TeleportWaiter.Task.IsCompleted)
            {
                State = state;
                TeleportWaiter.SetResult(state);
            }
        }

        public async Task<ETPAState> WaitForAccept()
        {
            if (AcceptWaiter.Task.IsCompleted)
            {
                return AcceptWaiter.Task.Result;
            }
            return await AcceptWaiter.Task;
        }

        public async Task<ETPAState> WaitForTeleport()
        {
            if (TeleportWaiter.Task.IsCompleted)
            {
                return AcceptWaiter.Task.Result;
            }
            return await TeleportWaiter.Task;
        }
    }
}