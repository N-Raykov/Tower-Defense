// Visual ghost for tower placement that changes color based on
// whether placement is valid / too expensive / invalid.

using UnityEngine;

namespace TowerDefense.Towers
{
    public class TowerGhostVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Colors")]
        [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.8f);       // green
        [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.8f);     // red
        [SerializeField] private Color tooExpensiveColor = new Color(1f, 1f, 0f, 0.8f); // yellow

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        public void SetSprite(Sprite sprite)
        {
            if (spriteRenderer == null) return;
            spriteRenderer.sprite = sprite;
        }

        public void SetState(bool canBuildHere, bool hasEnoughGold)
        {
            if (spriteRenderer == null) return;

            if (!canBuildHere)
            {
                spriteRenderer.color = invalidColor;
            }
            else if (!hasEnoughGold)
            {
                spriteRenderer.color = tooExpensiveColor;
            }
            else
            {
                spriteRenderer.color = validColor;
            }
        }
    }
}
