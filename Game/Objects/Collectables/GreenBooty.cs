﻿using MySQLManager.Database;
using Newtonsoft.Json;
using Ow.Game.Movements;
using Ow.Managers;
using Ow.Managers.MySQLManager;
using Ow.Net.netty.commands;
using Ow.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ow.Game.Objects.Collectables
{
    class GreenBooty : Collectable
    {
        public GreenBooty(Position position, Spacemap spacemap, bool respawnable, Player toPlayer = null) : base(AssetTypeModule.BOXTYPE_PIRATE_BOOTY, position, spacemap, respawnable, toPlayer) { }

        public override void Reward(Player player)
        {

            double rand = Randoms.random.NextDouble();


            if (rand <= 0.04)
            {
                double rand2 = Randoms.random.NextDouble();

                player.SendPacket($"0|A|STM|lf4_found|");
                //test = "lf4";
                using (var mySqlClient = SqlDatabaseManager.GetClient())
                {
                    var equipment = mySqlClient.ExecuteQueryTable($"SELECT * FROM player_equipment WHERE userId = {player.Id}");
                    var r = "";
                    object ri = null;
                    foreach (DataRow row in equipment.Rows)
                    {
                        ri = row["items"];
                        r = row["items"].ToString();

                        dynamic items = JsonConvert.DeserializeObject(r);
                        items["lf4Count"] += 1;

                        row["items"] = items;
                        var json = JsonConvert.SerializeObject(row["items"]);
                        string query = ($"UPDATE `player_equipment` SET `items` = '" + items + $"' WHERE userId = {player.Id}").Replace("\n", "").Replace("\r", "").Replace("\\", "");
                        mySqlClient.ExecuteQueryTable(query + ";");

                    }

                }
            }
            else if (rand <= 0.09)
            {
                //test = "gemi tasarım varsa 10k uri";
                player.SendPacket($"0|A|STM|10000 Uridium|");
                player.Data.uridium += 10000;

            }
            else if (rand <= 0.15)
            {
                var hours = Randoms.random.NextDouble() <= 0.1 ? 10 : 1;
                var boosterTypes = Randoms.random.NextDouble() <= 0.25 ? new int[] { 1, 16, 9, 11, 6, 3 } : new int[] { 0, 15, 8, 10, 5, 2 };
                var boosterType = boosterTypes[Randoms.random.Next(boosterTypes.Length)];

                player.BoosterManager.Add((BoosterType)boosterType, hours);
            }
            else
            {
                var uridium = Randoms.random.Next(1000, 4000);
                player.ChangeData(DataType.URIDIUM, uridium);
                QueryManager.SavePlayer.Information(player);
            }


            using (var mySqlClient = SqlDatabaseManager.GetClient())
            {
                var equipment = mySqlClient.ExecuteQueryTable($"SELECT * FROM player_accounts WHERE userId = {player.Id}");
                var r = "";
                object ri = null;
                foreach (DataRow row in equipment.Rows)
                {
                    ri = row["bootyKeys"];
                    r = ri.ToString();

                    dynamic items = JsonConvert.DeserializeObject(r);
                    items["greenKeys"].Value--;

                    row["bootyKeys"] = items;
                    var json = JsonConvert.SerializeObject(row["bootyKeys"]);
                    string query = ($"UPDATE `player_accounts` SET `bootyKeys` = '" + items + $"' WHERE userId = {player.Id}").Replace("\n", "").Replace("\r", "").Replace("\\", "");
                    mySqlClient.ExecuteQueryTable(query + ";");

                }

            }
            player.Equipment.Items.BootyKeys--;
            player.SendPacket($"0|A|BK|{player.Equipment.Items.BootyKeys}");

            

        }

        public override byte[] GetCollectableCreateCommand()
        {
            return CreateBoxCommand.write("PIRATE_BOOTY", Hash, Position.Y, Position.X);
        }
    }
}
