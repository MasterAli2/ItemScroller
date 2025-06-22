using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ItemScroller.Patches;


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
        public ConfigEntry<float> MaxHoldTimeForScrollables;

        void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Patch();

            ScrollThreshold = Config.Bind("Scrolling",
                                          "ScrollThreshold",
                                          0.75f,
                                          "The minimum amount of scroll before being able to scroll beetween items");

            MaxHoldTimeForScrollables = Config.Bind("Scrolling",
                                          "MaxHoldTime",
                                          0.75f,
                                          "The time (in seconds) that an item with a scroll function must be held before it cant be scrolled away from");


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
