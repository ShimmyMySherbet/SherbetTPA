using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using RocketExtensions.Models;
using SherbetTPA.Models;
using SherbetTPA.Models.Mode;

namespace SherbetTPA
{
    public class TPAPlugin : RocketPlugin<TPAConfig>
    {
        public TPAManager TPAManager;
        private ModeParser m_ModeParser = new ModeParser();
        public TPAConfig Config => Configuration.Instance;

        public override void LoadPlugin()
        {
            base.LoadPlugin();
            TPAManager = gameObject.AddComponent<TPAManager>();
            StringTypeConverter.RegisterParser(m_ModeParser);
            U.Events.OnPlayerDisconnected += OnPlayerDisconnect;
            Logger.Log("SherbetTPA by ShimmyMySherbet loaded!");
        }

        private void OnPlayerDisconnect(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            TPAManager.Disconnected(player.CSteamID.m_SteamID);
        }

        public override void UnloadPlugin(PluginState state = PluginState.Unloaded)
        {
            base.UnloadPlugin(state);
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnect;
            StringTypeConverter.DeregisterParser(m_ModeParser);
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "Tpa_Cooldown", "[color=red]Tpa is on cooldown for {0} sec[/color]" },
            { "Tpa_Sent", "[color=green]Tpa request sent to {0}[/color]" },
            { "Tpa_From", "[color=yellow]{0} requested to tp to you. Use /tpa a[/color]" },
            { "Tpa_Failed_Denied", "[color=yellow]{0} denied your teleport request[/color]" },
            { "Tpa_Failed_TimedOut", "[color=yellow]Tpa request timed out[/color]" },
            { "Tpa_Accepted", "[color=yellow]{0} accepted your tpa request, don't move for {1} sec...[/color]" },
            { "Tpa_Failed_Moved", "[color=red]Tpa request aborted because you moved[/color]" },
            { "Tpa_Failed_Blocked", "[color=red]Teleport failed because there wasn't enough room[/color]" },
            { "Abort_NoRequests", "[color=red]No outgoing tpa requests to abort[/color]"},
            { "Accept_NoRequests", "[color=red]No incoming tpa requests to accept[/color]"},
            { "Deny_NoRequests", "[color=red]No incoming tpa requests to deny[/color]"},
            { "Aborted", "[color=yellow]Tpa request to {0} aborted[/color]"},
            { "Accepted", "[color=yellow]Accepted {0}'s TPA request[/color]"},
            { "Denied", "[color=yellow]Denied {0}'s TPA request[/color]"},
            { "SentToSelf", "[color=yellow]You can't tpa to yourself[/color]"},
        };
    }
}