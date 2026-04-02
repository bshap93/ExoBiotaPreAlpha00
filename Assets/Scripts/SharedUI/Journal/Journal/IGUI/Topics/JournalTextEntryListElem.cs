using System.Text.RegularExpressions;
using JournalData.JournalEntries;
using SharedUI.BaseElement;
using TMPro;
using UnityEngine;

namespace SharedUI.Journal.Journal.IGUI.Topics
{
    public class JournalTextEntryListElem : SelectionListElementNavigable<JournalTextEntry>
    {
        public TMP_Text entryNameText;
        public TMP_Text entryDescriptionText;

        [Header("Keyword Emphasis")] public Color keywordColor = Color.yellow;
        public bool keywordBold = true;

        public override void Select()
        {
            // N/A for now
        }
        public override void Deselect()
        {
            // N/A
        }
        public override void Initialize(JournalTextEntry data)
        {
            ObjectData = data;
            entryNameText.text = data.entryName;
            // entryDescriptionText.text = data.entryTextDescription;
            entryNameText.color = data.nameTextColor;
            entryDescriptionText.color = data.descriptionTextColor;

            entryDescriptionText.text = BuildEmphasizedText(
                data.entryTextDescription,
                data.keywords,
                keywordColor,
                keywordBold
            );
        }

        // ---------------------------------------------------------------
        // Wraps every keyword occurrence in TMP rich-text tags.
        // Matching is case-insensitive; original casing is preserved.
        // ---------------------------------------------------------------
        static string BuildEmphasizedText(
            string source,
            string[] keywords,
            Color emphasisColor,
            bool bold)
        {
            if (string.IsNullOrEmpty(source) || keywords == null || keywords.Length == 0)
                return source;

            var colorHex = ColorUtility.ToHtmlStringRGB(emphasisColor); // e.g. "FFD700"

            var result = source;

            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword)) continue;

                // Build open/close tags around the matched (original-case) word
                var pattern = Regex.Escape(keyword);
                var replacement = bold
                    ? $"<b><color=#{colorHex}>$0</color></b>"
                    : $"<color=#{colorHex}>$0</color>";

                result = Regex.Replace(
                    result, pattern, replacement,
                    RegexOptions.IgnoreCase);
            }

            return result;
        }
    }
}
