using Newtonsoft.Json.Linq;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Core;
using System.Linq;
using System.Collections.Generic;
 
namespace Oxide.Plugins
{
    [Info("Clan info", "Bazz3l", "1.0.0")]
    [Description("List all clan members in a given clan")]
    class ClanInfo : RustPlugin
    {
        #region Plugins
        [PluginReference] Plugin Clans;
        #endregion

        #region Props
        private const string Perm = "claninfo.use";
        #endregion

        #region Oxide
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Message"] = "Clan: {0} Members:\n{1}",
                ["NotFound"] = "No clan found",
                ["NoTag"] = "No clan tag specified"
            }, this);
        }

        private void Init() => permission.RegisterPermission(Perm, this);
        #endregion

        #region Clan
        public JObject GetClan(string tag) => Clans?.Call<JObject>("GetClan", new object[] { tag });
        public JArray GetClanMembers(string tag) => (JArray)GetClan(tag)?.SelectToken("members");
        #endregion

        #region Chat Commands
        [ChatCommand("claninfo")]
        private void cmdCinfo(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, Perm)) return;
            if (args.Length < 1)
            {
                PrintToChat(player, Lang("NoTag"));
                return;
            }

            JArray clanMembers = GetClanMembers(args[0]);
            if (clanMembers == null)
            {
                PrintToChat(player, Lang("NotFound"));
                return;
            }

            List<string> members = new List<string>();
            foreach(JToken member in clanMembers)
            {
                var mPlayer = covalence?.FindPlayerById((string) member)?.Name;
                if (mPlayer != null)
                    members.Add(mPlayer);
            }

            PrintToChat(player, Lang("Message", null, args[0], string.Join("\n", members.ToArray())));
        }
        #endregion

        #region Helpers
        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);
        #endregion
    }
}
