using System;
using MoreMountains.InventoryEngine;
using SharedUI.Interface;
using UnityEngine;

namespace OWPData.DataClasses
{
    [Serializable]
    public class ExaminableObjectData : SceneObjectData
    {
        public string FullDescription;
        public IdentificationMode IdentificationMode;
        public Sprite UnknownIcon;
        public string UnknownShortBlurb;
        public string UnkonwnName;


        public ExaminableObjectData(string name, Sprite icon, string shortBlurb, Sprite actionIcon, string actionText,
            string fullDescription, IdentificationMode identificationMode, Sprite unknownIcon, string unknownShortBlurb,
            string unkonwnName) : base(name, icon, shortBlurb, actionIcon, actionText)
        {
            FullDescription = fullDescription;
            IdentificationMode = identificationMode;
            UnknownIcon = unknownIcon;
            UnknownShortBlurb = unknownShortBlurb;
            UnkonwnName = unkonwnName;
        }

        public SceneObjectData FromExaminableObjectData()
        {
            return new SceneObjectData(Name, Icon, ShortBlurb, ActionIcon, ActionText);
        }

        public new static ExaminableObjectData Empty()
        {
            return new ExaminableObjectData(
                string.Empty,
                null,
                string.Empty,
                null,
                string.Empty,
                string.Empty,
                IdentificationMode.None,
                null,
                string.Empty,
                string.Empty
            );
        }
    }
}