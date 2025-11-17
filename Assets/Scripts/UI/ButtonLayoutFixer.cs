using UnityEngine;
using UnityEngine.UI;

namespace RTS.UI
{
    /// <summary>
    /// Ensures button layout is correctly configured at runtime
    /// </summary>
    public class ButtonLayoutFixer : MonoBehaviour
    {
        private void Start()
        {
            // Find the HorizontalLayoutGroup
            var layoutGroup = GetComponent<HorizontalLayoutGroup>();
            
            if (layoutGroup == null)
            {
                return;
            }

            // Force correct settings
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childScaleWidth = false;
            layoutGroup.childScaleHeight = false;


            // Force layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        public void FixButtonSizes()
        {
            // Add LayoutElement to each button to specify preferred size
            foreach (Transform child in transform)
            {
                var layoutElement = child.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = child.gameObject.AddComponent<LayoutElement>();
                }

                layoutElement.preferredWidth = 80;
                layoutElement.preferredHeight = 100;

            }

            // Force layout rebuild again
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        private void LateUpdate()
        {
            // Force layout update every frame for first few frames
            if (Time.frameCount < 5)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            }
        }
    }
}
