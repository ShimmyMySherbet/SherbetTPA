using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RocketExtensions.Models;
using RocketExtensions.Utilities;
using SherbetTPA.Models;
using SherbetTPA.Models.Tasks;
using UnityEngine;

namespace SherbetTPA
{
    public class TPAManager : MonoBehaviour
    {
        private ConcurrentDictionary<ulong, List<TPARequest>> m_InboundQueue = new ConcurrentDictionary<ulong, List<TPARequest>>();
        private ConcurrentDictionary<ulong, List<TPARequest>> m_OutboundQueue = new ConcurrentDictionary<ulong, List<TPARequest>>();

        public TPAPlugin Plugin => GetComponent<TPAPlugin>();

        public void StartRequest(LDMPlayer to, CommandContext context)
        {
            var request = new TPARequest(context, to, Plugin.Config.TPATimeout, Plugin.Config.TPADeleySec);

            InitFor(to.PlayerID);
            InitFor(context.PlayerID);
            m_InboundQueue[to.PlayerID].Insert(0, request);
            m_OutboundQueue[context.PlayerID].Insert(0, request);
            Task.Run(() => TPAWaiter(request))
                .ContinueWith((_) => CloseTPA(request));
        }

        private void InitFor(ulong playerID)
        {
            if (!m_InboundQueue.ContainsKey(playerID))
            {
                m_InboundQueue[playerID] = new List<TPARequest>();
            }
            if (!m_OutboundQueue.ContainsKey(playerID))
            {
                m_OutboundQueue[playerID] = new List<TPARequest>();
            }
        }

        public void Disconnected(ulong player)
        {
            if (m_OutboundQueue.TryGetValue(player, out var outb))
            {
                foreach (var q in outb)
                {
                    q.ReleaseAcceptWaiter(ETPAState.Aborted_PlayerDisconnect);
                    q.ReleaseTeleportWaiter(ETPAState.Aborted_PlayerDisconnect);
                }
            }

            if (m_InboundQueue.TryGetValue(player, out var inb))
            {
                foreach (var q in inb)
                {
                    q.ReleaseAcceptWaiter(ETPAState.Aborted_PlayerDisconnect);
                    q.ReleaseTeleportWaiter(ETPAState.Aborted_PlayerDisconnect);
                }
            }
        }

        private void CloseTPA(TPARequest request)
        {
            request.State = ETPAState.Finished;
            if (m_InboundQueue.TryGetValue(request.To.PlayerID, out var inbound))
            {
                inbound.Remove(request);
            }

            if (m_OutboundQueue.TryGetValue(request.From.PlayerID, out var outbound))
            {
                outbound.Remove(request);
            }

            request.ReleaseAcceptWaiter(ETPAState.Finished);
            request.ReleaseTeleportWaiter(ETPAState.Finished);
        }

        public TPARequest Abort(LDMPlayer from)
        {
            if (m_OutboundQueue.TryGetValue(from.PlayerID, out var queue))
            {
                var match = queue.FirstOrDefault(x => x.State == ETPAState.Waiting_Accept);
                if (match != null)
                    match.ReleaseAcceptWaiter(ETPAState.Aborted_SenderAborted);
                return match;
            }
            return null;
        }

        public TPARequest Deny(LDMPlayer to)
        {
            if (m_InboundQueue.TryGetValue(to.PlayerID, out var queue))
            {
                var match = queue.FirstOrDefault(x => x.State == ETPAState.Waiting_Accept);
                if (match != null)
                    match.ReleaseAcceptWaiter(ETPAState.Aborted_TargetDenied);
                return match;
            }
            return null;
        }

        public TPARequest Accept(LDMPlayer to)
        {
            if (m_InboundQueue.TryGetValue(to.PlayerID, out var queue))
            {
                var match = queue.FirstOrDefault(x => x.State == ETPAState.Waiting_Accept);
                if (match != null)
                    match.ReleaseAcceptWaiter(ETPAState.Accepted);
                return match;
            }
            return null;
        }

 
        private async Task TPAWaiter(TPARequest request)
        {
            await request.Context.ReplyKeyAsync("Tpa_Sent", request.To.DisplayName);
            await request.To.MessageAsync(Plugin.Translate("Tpa_From", request.From.DisplayName).ReformatColor());
            var timeoutWaiter = Waiters.TimeoutWaiter(request, Plugin.Config.TPATimeout);
            request.State = ETPAState.Waiting_Accept;
            await request.WaitForAccept();

            switch (request.State)
            {
                case ETPAState.Aborted_PlayerDisconnect:
                case ETPAState.Aborted_SenderAborted:
                case ETPAState.Finished:
                    timeoutWaiter.Dispose();
                    return;

                case ETPAState.Aborted_TargetDenied:
                    await request.Context.ReplyKeyAsync("Tpa_Failed_Denied", request.To.DisplayName);
                    return;

                case ETPAState.TimedOut:
                    await request.Context.ReplyKeyAsync("Tpa_Failed_TimedOut", request.To.DisplayName);
                    return;

                case ETPAState.Accepted:
                    request.TeleportStarted = DateTime.Now;
                    break;
            }

            var movementWaiter = Waiters.MovementWaiter(request);
            var teleportWaiter = Waiters.TeleportWaiter(request);
            await request.WaitForTeleport();
            switch (request.State)
            {
                case ETPAState.Aborted_PlayerMoved:
                    return;

                case ETPAState.Aborted_PlayerDisconnect:
                case ETPAState.Finished:
                    movementWaiter.Dispose();
                    teleportWaiter.Dispose();
                    return;

                case ETPAState.Teleport:
                    if (!await request.From.TeleportAsync(request.To.Position, request.To.Rotation))
                    {
                        await request.Context.ReplyKeyAsync("Tpa_Failed_Blocked");
                        return;
                    }

                    return;
            }
        }
    }
}