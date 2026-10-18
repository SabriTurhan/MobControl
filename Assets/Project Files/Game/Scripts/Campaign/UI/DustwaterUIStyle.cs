using TMPro;
using UnityEngine;

namespace Watermelon
{
    // Sprites, fonts and colours of the template UI used by the Dustwater screens
    [CreateAssetMenu(fileName = "Dustwater UI Style", menuName = "Data/Campaign/UI Style")]
    public class DustwaterUIStyle : ScriptableObject
    {
        [Header("Fonts")]
        public TMP_FontAsset font;
        public TMP_FontAsset fontOutline;
        [Tooltip("SDF presets of the font (outline/plain); fallback glyphs (Cyrillic) inherit them")]
        public Material fontMaterial;
        public Material fontOutlineMaterial;

        [Header("Panels")]
        public Sprite panelWhite;
        public Sprite panelBlue;
        public Sprite panelDark;
        public Sprite panelRed;
        public Sprite panelOutlinedDark;
        public Sprite itemBackground;
        public Sprite itemFrame;
        public Sprite cardCommon;
        public Sprite cardRare;
        public Sprite cardEpic;
        public Sprite headerOrange;
        public Sprite roundedBack;
        public Sprite square;
        public Sprite circle;
        public Sprite shine;

        [Header("Buttons")]
        public Sprite buttonGreen;
        public Sprite buttonOrange;
        public Sprite buttonRed;
        public Sprite buttonGray;
        public Sprite buttonPurple;
        public Sprite buttonClose;

        [Header("Icons")]
        public Sprite money;
        public Sprite supplies;
        public Sprite star;
        public Sprite checkmark;
        public Sprite questionMark;
        public Sprite plus;
        public Sprite store;
        public Sprite skins;
        public Sprite town;
        public Sprite squad;
        public Sprite battle;
        public Sprite chest;
        public Sprite heart;

        [Header("Story")]
        public Sprite commanderPortrait;
        public string commanderName = "SGT. MASON";

        [Header("Colours")]
        public Color dark = new Color32(46, 44, 62, 255);
        public Color textDark = new Color32(60, 58, 78, 255);
        public Color green = new Color32(62, 200, 90, 255);
        public Color gold = new Color32(255, 196, 46, 255);
        public Color red = new Color32(232, 70, 70, 255);
        public Color blue = new Color32(78, 132, 253, 255);
        public Color dim = new Color(0f, 0f, 0f, 0.62f);

        public Sprite CardSprite(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Rare: return cardRare;
                case CardRarity.Epic: return cardEpic;
                default: return cardCommon;
            }
        }

        public Color RarityColor(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Rare: return new Color32(170, 100, 255, 255);
                case CardRarity.Epic: return new Color32(255, 190, 40, 255);
                default: return new Color32(80, 160, 255, 255);
            }
        }
    }
}
