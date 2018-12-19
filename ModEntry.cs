using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace BetterActivateSprinklers
{
    public class ModEntry : Mod
    {
        private ModConfig Config;
        private object BetterSprinklersApi;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            Helper.Events.GameLoop.GameLaunched += this.OnGameLaunch;

            if (Config.ActivateOnAction)
            {
                Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            }

            if (Config.ActivateOnPlacement)
            {
                Helper.Events.World.ObjectListChanged += this.OnWorld_ObjectListChanged;
            }
        }

        private void OnGameLaunch(object sender, GameLaunchedEventArgs e)
        {
            if (Helper.ModRegistry.IsLoaded("Speeder.BetterSprinklers"))
            {
                BetterSprinklersApi = Helper.ModRegistry.GetApi("Speeder.BetterSprinklers");
            }
        }

        private void OnWorld_ObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            foreach (var pair in e.Added)
            {
                ActivateSprinkler(pair.Value);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton())
            {
                var tile = e.Cursor.GrabTile;
                if (tile == null) return;

                var obj = Game1.currentLocation.getObjectAtTile((int) tile.X, (int) tile.Y);
                if (obj == null) return;

                ActivateSprinkler(obj);
            }
        }

        private void ActivateSprinkler(Object sprinkler)
        {
            if (sprinkler == null) return;

            if (sprinkler.Name.Contains("Sprinkler"))
            {
                if (BetterSprinklersApi == null)
                {
                    sprinkler.DayUpdate(Game1.currentLocation);
                }
                else
                {
                    IDictionary<int, Vector2[]> coverageList = Helper.Reflection.GetMethod(BetterSprinklersApi, "GetSprinklerCoverage").Invoke<IDictionary<int, Vector2[]>>();
                    Vector2[] coverage = coverageList[sprinkler.ParentSheetIndex];
                    Vector2 sprinklerTile = sprinkler.TileLocation;

                    foreach (Vector2 v in coverage)
                    {
                        Vector2 coveredTile = sprinklerTile + v;
                        TerrainFeature terrainFeature;
                        HoeDirt hoeDirt;

                        if (Game1.currentLocation.terrainFeatures.TryGetValue(coveredTile, out terrainFeature) && (hoeDirt = terrainFeature as HoeDirt) != null)
                        {
                            hoeDirt.state.Value = 1;
                        }
                    }
                }
            }
        }
    }
}
