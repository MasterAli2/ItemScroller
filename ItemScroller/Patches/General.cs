using HarmonyLib;
using UnityEngine;
using Zorro.ControllerSupport;
using Zorro.Core;

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
                bool isMouseInput = InputHandler.GetCurrentUsedInputScheme() == InputScheme.KeyboardMouse;

                if (item.OnScrolled != null || (isMouseInput && item.OnScrolledMouseOnly != null))
                {
                    return;
                }
            }



            float scrollDelta = __instance.character.input.scrollInput;

            if (scrollDelta != 0)
            {
                if (Mathf.Abs(scrollDelta) >= ItemScroller.Instance.ScrollThreshold.Value)
                {
                    int steps = Mathf.Clamp(Mathf.FloorToInt(scrollDelta / ItemScroller.Instance.ScrollThreshold.Value),-1,1);
                    bool forward = steps == 1 ? false : true;

                   
                    var temp = Next(forward, __instance.lastSelectedSlot.Value, __instance);

                    byte nextSlot = temp.Item1;
                    bool failed = !temp.Item2;

                    if (failed)
                    {
                        ItemScroller.Logger.LogDebug("No empty slot found");

                        return;
                    }

                    __instance.lastSwitched = Time.time;
                    __instance.timesSwitchedRecently++;

                    __instance.EquipSlot(Optionable<byte>.Some(nextSlot));
                }
            }
        }

        static (byte, bool) Next(bool forward, byte target, CharacterItems instance)
        {
            bool hasBackPack = !instance.character.player.GetItemSlot(3).IsEmpty();
            const byte regularSlotsCount = 3; 
            byte slotsToCheck = (byte)(hasBackPack ? 3 : 2);


            if (target >= slotsToCheck || (target == 3 && !hasBackPack))
            {
                target = 0; 
            }

            
            bool IsValidAndNonEmpty(byte slot)
            {  
                if (slot == 3) return hasBackPack;
                if (slot >= regularSlotsCount) return false; 
                return !instance.character.player.itemSlots[slot].IsEmpty();
            }

            byte current = target;

            int attempts = 0;
            int maxAttempts = slotsToCheck;

            do
            {
                if (forward)
                {
                    current = (byte)((current + 1) % slotsToCheck);
                }
                else
                {
                    current = (byte)((current - 1 + slotsToCheck) % slotsToCheck + 1);
                }

                if (current == 3 && !hasBackPack)
                {
                    attempts++;
                    continue;
                }

                if (IsValidAndNonEmpty(current) && current != target)
                {
                    return (current, true);
                }

                attempts++;
            } while (attempts < maxAttempts);

            //No empty slot
            return (target, false);
        }

    }
}
