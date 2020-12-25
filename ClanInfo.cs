//Requires: Clans

using System.Collections.Generic;
using System.Text;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Clan Info", "Bazz3l", "1.0.6")]
    [Description("List all clan members in a given clan")]
    public class ClanInfo : CovalencePlugin
    {
        [PluginReference] Plugin Clans;

        #region Fields
        
        private const string PermUse = "claninfo.use";
        
        #endregion

        #region Lang

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                { "InvalidSyntax", "/cinfo <clan-tag>" },
                { "NoPermission", "No permission" },
                { "NotFound", "No clan found by that name" },
                { "ColumnName", "Name" },
                { "ColumnStatus", "Status" },
                { "ColumnID", "SteamID" },
                { "Tag", "[#AAFF55]{0}[/#]" },
                { "Online", "[#21BF4D]Online[/#]" },
                { "Offline", "[#DC143C]Offline[/#]" },
                { "DisplayMsg", "[#DC143C]Clan[/#]: [#EDDf45]{0}[/#], current clan members.\n{1}" }
            }, this);
        }

        #endregion

        #region Oxide

        private void Init() => AddCovalenceCommand("cinfo", nameof(InfoCommand), PermUse);

        #endregion

        #region Core

        private void DisplayInfo(IPlayer player, string clanTag)
        {
            JArray clanMembers = GetClanMembers(clanTag);
            
            if (clanMembers == null)
            {
                player.Reply(Lang("NotFound", player.Id));
                
                return;
            }

            CustomTable customTable = new CustomTable();
            
            customTable.AddColumn(Lang("ColumnName", player.Id));
            customTable.AddColumn(Lang("ColumnID", player.Id));
            customTable.AddColumn(Lang("ColumnStatus", player.Id));
            
            foreach(JToken userID in clanMembers)
            {
                IPlayer mPlayer = covalence.Players.FindPlayerById((string)userID);

                if (mPlayer == null)
                {
                    continue;
                }
                
                customTable.AddRow(mPlayer.Name, mPlayer.Id, mPlayer.IsConnected ? Lang("Online", player.Id) : Lang("Offline", player.Id));
            }

            player.Reply(Lang("DisplayMsg", player.Id, clanTag, customTable));
        }

        #endregion

        #region Commands
        
        private void InfoCommand(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                player.Reply(Lang("InvalidSyntax", player.Id));
                
                return;
            }

            DisplayInfo(player,string.Join(" ", args));
        }
        
        #endregion

        #region Classes

        private class CustomTable
        {
            private readonly StringBuilder _builder = new StringBuilder();
            private readonly List<Column> _columns = new List<Column>();
            private readonly List<Row> _rows = new List<Row>();
            private string _text = string.Empty;
            private bool _dirty;

            public void AddColumn(string title)
            {
                _columns.Add(new Column(title));
                
                _dirty = true;
            }

            public void AddRow(params string[] values)
            {
                int num = Mathf.Min(_columns.Count, values.Length);
                
                for (int index = 0; index < num; ++index)
                {
                    _columns[index].width = Mathf.Max(_columns[index].width, values[index].Length);
                }
                
                _rows.Add(new Row(values));
                
                _dirty = true;
            }

            public override string ToString()
            {
                if (_dirty)
                {
                    _builder.Clear();
                  
                    for (int index = 0; index < _columns.Count; ++index)
                    {
                        _builder.Append(_columns[index].title.PadRight(_columns[index].width + 1));
                    }
                  
                    _builder.AppendLine();
                  
                    for (int index1 = 0; index1 < _rows.Count; ++index1)
                    {
                        Row row = _rows[index1];
                    
                        int num = Mathf.Min(_columns.Count, row.values.Length);
                    
                        for (int index2 = 0; index2 < num; ++index2)
                        {
                            _builder.Append(row.values[index2].PadRight(_columns[index2].width + 1));
                        }
                    
                        _builder.AppendLine();
                    }
                    
                    _text = _builder.ToString();
                    
                    _dirty = false;
                }
                
                return _text;
            }

            private class Row
            {
                public string[] values;

                public Row(string[] values)
                {
                    this.values = values;
                }
            }

            private class Column
            {
                public string title;
                public int width;

                public Column(string title)
                {
                    this.title = title;
                    width = title.Length;
                }
            }
        }

        #endregion

        #region Helpers
        
        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);
        
        private JObject GetClan(string tag) => Clans?.Call<JObject>("GetClan", new object[] { tag });
        
        private JArray GetClanMembers(string tag) => (JArray)GetClan(tag)?.SelectToken("members");
        
        #endregion
    }
}