using System;
using UnityEngine;

namespace SharedUI.Interface
{
    public interface IBillboardable
    {
        public string GetName();
        public Sprite GetIcon();
        public string ShortBlurb();

        public Sprite GetActionIcon();
        public string GetActionText();
    }

    [Serializable]
    public class SceneObjectData
    {
        public Sprite ActionIcon;
        public string ActionText;
        public Sprite Icon;
        public string Id;
        public string Name;
        public string ShortBlurb;

        public SceneObjectData(string name, Sprite icon, string shortBlurb, Sprite actionIcon, string actionText)
        {
            Name = name;
            Icon = icon;
            ShortBlurb = shortBlurb;
            ActionIcon = actionIcon;
            ActionText = actionText;
        }

        public static SceneObjectData Empty()
        {
            return new SceneObjectData(
                string.Empty,
                null,
                string.Empty,
                null,
                string.Empty
            );
        }
    }
}
