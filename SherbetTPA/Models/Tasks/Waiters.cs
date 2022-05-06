using System.Threading.Tasks;
using UnityEngine;

namespace SherbetTPA.Models.Tasks
{
    public static class Waiters
    {
        public static async Task MovementWaiter(TPARequest request)
        {
            await Task.Delay(1000); // 1 sec grace period
            var startPos = request.From.Position;
            while (request.State == ETPAState.Accepted)
            {
                var dist = Vector3.Distance(request.From.Position, startPos);
                if (dist > 1)
                {
                    request.ReleaseTeleportWaiter(ETPAState.Aborted_PlayerMoved);
                    return;
                }
                await Task.Delay(200);
            }
        }

        public static async Task TeleportWaiter(TPARequest request)
        {
            await Task.Delay(request.TeleportTimeRemaining);
            if (request.State == ETPAState.Accepted)
            {
                request.ReleaseTeleportWaiter(ETPAState.Teleport);
            }
        }

        public static async Task TimeoutWaiter(TPARequest request, int deley)
        {
            await Task.Delay(deley * 1000);
            if (request.State == ETPAState.Waiting_Accept)
            {
                request.ReleaseAcceptWaiter(ETPAState.TimedOut);
            }
        }
    }
}