using Manager;
using MoreMountains.InventoryEngine;
using UnityEngine;
// ExaminationManager

namespace Utilities.Static
{
    public static class InventoryItemDisplayExtensions
    {
        public static bool IsKnownToPlayer(this InventoryItem item)
        {
            if (item == null) return false;

            // Always known on sight / none
            if (item.identificationMode == IdentificationMode.RecognizableOnSight ||
                item.identificationMode == IdentificationMode.None)
                return true;

            // Known if type was unlocked by any path
            return ExaminationManager.Instance != null &&
                   ExaminationManager.Instance.HasTypeBeenExamined(item.ItemID);
        }

        public static string GetDisplayName(this InventoryItem item)
        {
            return item != null && item.IsKnownToPlayer() ? item.ItemName : item?.UnknownName ?? "Unknown Item";
        }

        public static Sprite GetDisplayIcon(this InventoryItem item)
        {
            if (item == null) return null;
            if (item.IsKnownToPlayer()) return item.Icon;

            var fallback = ExaminationManager.Instance != null
                ? ExaminationManager.Instance.defaultUnknownIcon
                : null;

            return item.Icon != null ? item.Icon : fallback;
        }

        public static string GetDisplayShortDescription(this InventoryItem item)
        {
            return item != null && item.IsKnownToPlayer() ? item.ShortDescription : string.Empty;
        }
    }
}