using System;
using System.IO;
using System.Linq;
using EsfLibrary;

namespace faction_capital_change {
    class Program {
        static void Main(string[] args) {
            if (args == null || args.Length == 0) {
                Console.WriteLine("Error. No arguments to parse were detected. Terminating...");
            }

            if (args.Length > 1)
                Console.WriteLine("Warning. Detected more than 1 argument! Will only parse the first one.");

            if (!uint.TryParse(args[0], out uint newCapitalId)) {
                Console.WriteLine("Failed to parse passed argument!");
                return;
            }

            string game         = "Attila";                                                                                     // Hardcoded but can be exposed to command line args and passed from lua
            var appDataPath     = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directory       = new DirectoryInfo(@$"{appDataPath}\The Creative Assembly\{game}\save_games");
            var latestSaveFile  = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();

            var saveFile                    = EsfCodecUtil.LoadEsfFile(latestSaveFile.FullName);
            var campaign_save_game_node     = saveFile.RootNode as ParentNode;
            var compressed_data_node        = campaign_save_game_node   .Children.Find(n => n.GetName() == "COMPRESSED_DATA");
            var campaign_env_node           = compressed_data_node      .Children.Find(n => n.GetName() == "CAMPAIGN_ENV");
            var campaign_model_node         = campaign_env_node         .Children.Find(n => n.GetName() == "CAMPAIGN_MODEL");
            var world_node                  = campaign_model_node       .Children.Find(n => n.GetName() == "WORLD");
            var faction_array_node          = world_node                .Children.Find(n => n.GetName() == "FACTION_ARRAY");
            var player_faction_node         = faction_array_node        .Children.First().Children.First();                     // Note: It's assumed that the first child of the faction array is a player faction!!!
            var og_capital_id_value_node    = player_faction_node       .Value[21] as OptimizedUIntNode;                        // Assuming that the original capital id node is located at index 7 of the faction node
            var cur_capital_id_value_node   = player_faction_node       .Value[22] as OptimizedUIntNode;                        // Assuming that the current capital id node is located at index 8 of the faction node

            // Replace capital Id
            og_capital_id_value_node.Value  = newCapitalId;
            cur_capital_id_value_node.Value = newCapitalId;

            EsfCodecUtil.WriteEsfFile(latestSaveFile.FullName, saveFile);
        }
    }
}
