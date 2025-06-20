using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ItemScroller.Patches;
using Zorro.Core.Serizalization;


namespace ItemScroller
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class ItemScroller : BaseUnityPlugin
    {
        public const string GUID = "MasterAli2.ItemScroller";
        public const string NAME = "Item Scroller";
        public const string VERSION = "1.0.0";

        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        public static ItemScroller Instance { get; private set; } = null!;

        public ConfigEntry<float> ScrollThreshold;

        void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Patch();

            ScrollThreshold = Config.Bind("Scrolling",
                                          "ScrollThreshold",
                                          1f,
                                          "The minimum amount of scroll before the being able to scroll beetween items");

            Logger.LogInfo($"{GUID} v{VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll(typeof(General));

            Logger.LogDebug("Finished patching!");
        }
    }
}
