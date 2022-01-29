using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;

namespace JuneNPC
{
    public class ModEntry : Mod
    {

        public override void Entry(IModHelper helper)
        {
            NPCPatches.Initialize(NPCPatches.Monitor);

            var harmony = new Harmony(this.ModManifest.UniqueID);
            try
            {
                System.Reflection.MethodBase NPC_StartRouteBehavior = AccessTools.Method(typeof(NPC), "startRouteBehavior");
                System.Reflection.MethodBase NPC_FinishRouteBehavior = AccessTools.Method(typeof(NPC), "finishRouteBehavior");

                harmony.Patch(
                    original: NPC_StartRouteBehavior,
                    postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_StartRouteBehavior_Postfix))
                    );

                harmony.Patch(
                    original: NPC_FinishRouteBehavior,
                    postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_FinishRouteBehavior_Postfix))
                    );

                helper.Events.GameLoop.DayEnding += DayEnd;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in JuneNPC ModEntry stage:\n{ex}", LogLevel.Error);
            }
        }
        private void DayEnd(object sender, DayEndingEventArgs args)
        {// reset June's sprite at end of day in case day ended while he was playing piano
            try
            {
                NPC June = Game1.getCharacterFromName("June");
                June.Sprite.SpriteHeight = 32;
                June.Sprite.SpriteWidth = 16;
                June.Sprite.ignoreSourceRectUpdates = false;
                June.Sprite.UpdateSourceRect();
                June.drawOffset.Value = Vector2.Zero;
                June.IsInvisible = false;
            }
            catch (Exception ex1)
            {
                Monitor.Log($"Failed in JuneNPC Day End reset:\n{ex1}", LogLevel.Error);
            }
            

        }
    };

    public class NPCPatches
    {
        public static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }


        public static void NPC_StartRouteBehavior_Postfix(StardewValley.NPC __instance, string behaviorName)
        {
            try
            {
                switch (behaviorName)
                {
                    case "june_piano":
                        __instance.extendSourceRect(16, 0);
                        __instance.Sprite.SpriteWidth = 32;
                        __instance.Sprite.ignoreSourceRectUpdates = false;
                        __instance.Sprite.currentFrame = 8;
                        break;
                }
            }
            catch (Exception ex2)
            {
                Monitor.Log($"Failed in JuneNPC start postfix:\n{ex2}", LogLevel.Error);
            }
        }

        public static void NPC_FinishRouteBehavior_Postfix(StardewValley.NPC __instance, string behaviorName)
        {
            try
            {
                switch (behaviorName)
                {
                    case "june_piano":
                        __instance.reloadSprite();
                        __instance.Sprite.SpriteWidth = 16;
                        __instance.Sprite.SpriteHeight = 32;
                        __instance.Sprite.UpdateSourceRect();
                        __instance.drawOffset.Value = Vector2.Zero;
                        __instance.Halt();
                        __instance.movementPause = 1;
                        break;
                }
            }
            catch (Exception ex3)
            {
                Monitor.Log($"Failed in JuneNPC finish postfix:\n{ex3}", LogLevel.Error);
            }
        }
    }
}