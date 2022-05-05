using System.Linq;
using ReModAres.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReModCE_ARES.Managers
{
    public class QMLable
    {
        public TextMeshProUGUI textText;
        public GameObject lable;
        private Image backgroundImage;
        private static GameObject TextObject;

        public QMLable(Transform menu, float x, float y, string contents)
        {
            VRC.UI.Elements.QuickMenu quickMenu = Resources.FindObjectsOfTypeAll<VRC.UI.Elements.QuickMenu>().First();
            lable = UnityEngine.Object.Instantiate<GameObject>(quickMenu.transform.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Header_QuickLinks").gameObject, menu);
            lable.name = contents;
            lable.transform.localPosition = new Vector3(x, y, 0);
            backgroundImage = lable.AddComponent<Image>();
            backgroundImage.GetComponent<RectTransform>().pivot = new Vector2(-0.15f, 1f);
            backgroundImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -2.0f);
            backgroundImage.GetComponent<RectTransform>().sizeDelta = new Vector2(900f, 1000f);
            backgroundImage.sprite = ResourceManager.GetSprite("remodce.ares_panel");
            backgroundImage.transform.SetParent(lable.transform, false);

            TextMeshProUGUI text = lable.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "";

            TextObject = new GameObject("Text");
            TextObject.AddComponent<CanvasRenderer>();
            //text = lable.GetComponentInChildren<TextMeshProUGUI>();
            TextObject.transform.SetParent(backgroundImage.transform, false);
            textText = TextObject.AddComponent<TextMeshProUGUI>();
            textText.text = contents;
            //textText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            //text.enableAutoSizing = true;
            textText.fontSize = 25;
            textText.GetComponent<RectTransform>().localPosition += new Vector3(-350f, 300f, 0f);

            textText.transform.SetParent(lable.transform, false);

            

            lable.gameObject.SetActive(false);
        }
    }
}