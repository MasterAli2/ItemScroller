﻿using HarmonyLib;
using System.Collections;
using UnityEngine;
using Zorro.ControllerSupport;
using Zorro.Core;
using static Zorro.ControllerSupport.Rumble.RumbleClip;

namespace ItemScroller.Patches
{
    internal class General
    {
        [HarmonyPatch(typeof(CharacterItems), nameof(CharacterItems.DoSwitching))]
        [HarmonyPrefix]
        static void _(CharacterItems __instance)
        {
            if (!__instance.character.IsLocal ||
                !__instance.character.CanDoInput() ||
                !(Time.time > __instance.lastEquippedSlotTime + __instance.equippedSlotCooldown) ||
                __instance.character.data.isClimbing ||
                __instance.character.data.isRopeClimbing ||
                __instance.character.data.fullyPassedOut)
            {
                return;
            }

            if (__instance.character.data.currentItem &&
                !__instance.character.data.passedOut)
            {
                Item item = __instance.character.data.currentItem;
                
                if (hasScrollFunction(item))
                {
                   // return;
                }
            }



            float scrollDelta = __instance.character.input.scrollInput;

            if (scrollDelta != 0)
            {
                if (Mathf.Abs(scrollDelta) >= ItemScroller.Instance.ScrollThreshold.Value)
                {
                    bool forward = true;

                    if (Mathf.Sign(scrollDelta) == 1)
                    {
                        forward = false;
                    }

                    var temp = General.Next(forward, __instance.lastSelectedSlot.Value, __instance);

                    byte? nextSlot = temp.Item1;

                    if (!temp.Item2 || nextSlot == null)
                    {
                        ItemScroller.Logger.LogDebug("No empty slot found");
                        return;
                    }

                    __instance.lastSwitched = Time.time;
                    __instance.timesSwitchedRecently++;

                    __instance.EquipSlot(Optionable<byte>.Some(nextSlot.Value));
                }
            }
        }

       

        static bool hasScrollFunction(Item? item)
        {
            if (item == null)
            {
                return false;
            }

            bool isMouseInput = InputHandler.GetCurrentUsedInputScheme() == InputScheme.KeyboardMouse;

            return item.OnScrolled != null || (isMouseInput && item.OnScrolledMouseOnly != null);
        }


        public static (byte?, bool) Next(bool forward, byte target, CharacterItems instance)
        {
            bool hasBackPack = !instance.character.player.GetItemSlot(3).IsEmpty();
            const byte regularSlotsCount = 3;
            byte slotsToCheck = (byte)(hasBackPack ? 4 : 3);

            if (target >= slotsToCheck || (target == 3 && !hasBackPack))
            {
                target = 0;
            }

            bool IsValidSlot(byte slot)
            {
                if (slot == 3) return hasBackPack;
                if (slot >= regularSlotsCount) return false;

                var itemSlot = instance.character.player.itemSlots[slot];
                if (itemSlot.IsEmpty()) return false;

                Item item = itemSlot.prefab;

                bool ScrollFunctionPresent = false;

                item.GetItemActions();
                foreach (ItemActionBase iab in item.itemActions)
                {
                    iab.item = item;
                    iab.Subscribe();

                    if (hasScrollFunction(item))
                    {
                        ScrollFunctionPresent = true;
                    }

                    iab.Unsubscribe();
                    iab.item = null;
                }
                item.itemActions = null;


                ItemScroller.Logger.LogError(item.OnScrolled?.Method);
                ItemScroller.Logger.LogError(item.OnScrolledMouseOnly?.Method);

                return !ScrollFunctionPresent;
            }


            byte current = target;
            int attempts = 0;
            int maxAttempts = slotsToCheck + 1;

            do
            {
                if (forward)
                {
                    current = (byte)((current + 1) % slotsToCheck);
                }
                else
                {
                    current = (byte)((current - 1 + slotsToCheck) % slotsToCheck);
                }

                if (current == 3 && !hasBackPack)
                {
                    attempts++;
                    continue;
                }

                if (IsValidSlot(current) && target != current)
                {
                    return (current, true);
                }

                attempts++;
            } while (attempts < maxAttempts);

            return (null, false);
        }
    }
}
