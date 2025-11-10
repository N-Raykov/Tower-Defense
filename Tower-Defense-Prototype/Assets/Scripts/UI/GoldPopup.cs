// Floating text that shows gained gold, moves up and fades out.

using UnityEngine;
using TMPro;

namespace TowerDefense.UI
{
    public class GoldPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private float lifetime = 1.0f;
        [SerializeField] private float moveUpSpeed = 0.8f;
        [SerializeField] private float fadeOutSpeed = 2f;

        [Header("Rendering")]
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private int sortingOrder = 50;

        private float timer;

        public void Initialize(int amount)
        {
            if (textMesh == null)
            {
                textMesh = GetComponentInChildren<TextMeshPro>();
            }

            if (textMesh != null)
            {
                textMesh.text = $"+{amount}";

                var renderer = textMesh.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sortingLayerName = sortingLayerName;
                    renderer.sortingOrder = sortingOrder;
                }
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += Vector3.up * (moveUpSpeed * Time.deltaTime);

            if (textMesh != null)
            {
                Color c = textMesh.color;
                c.a = Mathf.Lerp(1f, 0f, timer * fadeOutSpeed / lifetime);
                textMesh.color = c;
            }
        }
    }

}

