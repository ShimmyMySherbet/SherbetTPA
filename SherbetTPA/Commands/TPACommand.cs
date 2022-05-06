using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using SDG.Unturned;
using SherbetTPA.Models.Mode;

namespace SherbetTPA.Commands
{
    [CommandInfo("Manages tpa requests", "Tpa [Accept/Deny/Abort | Player]")]
    public class TPACommand : RocketCommand
    {
        public new TPAPlugin Plugin => base.Plugin as TPAPlugin;
        private ConcurrentDictionary<ulong, DateTime> m_Timeouts = new ConcurrentDictionary<ulong, DateTime>();

        public override async UniTask Execute(CommandContext context)
        {
            var mode = context.Arguments.Get(0, EMode.Invalid);

            switch (mode)
            {
                case EMode.Invalid:
                    await context.ReplyAsync(Syntax);
                    return;

                case EMode.Request:
                    if (m_Timeouts.TryGetValue(context.PlayerID, out var allowed))
                    {
                        if (allowed > DateTime.Now)
                        {
                            await context.ReplyKeyAsync("Tpa_Cooldown", Math.Round((allowed - DateTime.Now).TotalSeconds));
                            return;
                        }
                    }

                    var targetPlayer = context.Arguments.Get<Player>(0, paramName: "Player");

                    if (targetPlayer.channel.owner.playerID.steamID.m_SteamID == context.PlayerID)
                    {
                        await context.ReplyKeyAsync("SentToSelf");
                        return;
                    }

                    Plugin.TPAManager.StartRequest(LDMPlayer.FromPlayer(targetPlayer), context);
                    m_Timeouts[context.PlayerID] = DateTime.Now.AddSeconds(Plugin.Config.TPACooldownSec);
                    return;

                case EMode.Abort:
                    var aborted = Plugin.TPAManager.Abort(context.LDMPlayer);
                    if (aborted == null)
                    {
                        await context.ReplyKeyAsync("Abort_NoRequests");
                    } else
                    {
                        await context.ReplyKeyAsync("Aborted", aborted.To.DisplayName);
                    }
                    return;

                case EMode.Accept:
                    var accepted = Plugin.TPAManager.Accept(context.LDMPlayer);
                    if (accepted == null)
                    {
                        await context.ReplyKeyAsync("Accept_NoRequests");
                    }
                    else
                    {
                        await context.ReplyKeyAsync("Accepted", accepted.From.DisplayName);
                    }
                    return;

                case EMode.Deny:
                    var denied = Plugin.TPAManager.Deny(context.LDMPlayer);
                    if (denied == null)
                    {
                        await context.ReplyKeyAsync("Deny_NoRequests");
                    }
                    else
                    {
                        await context.ReplyKeyAsync("Denied", denied.From.DisplayName);
                    }
                    return;
            }
        }
    }
}