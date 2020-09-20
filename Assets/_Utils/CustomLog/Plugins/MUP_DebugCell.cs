using UnityEngine;
using System.Collections;

using UnityEngine.UI;

namespace Mup.CustomLog
{

    public class MUP_DebugCell : MonoBehaviour
    {

        [SerializeField]
        private Text textDisplay;

        public Text TextDisplay { get { return textDisplay; } }

        private Image myImage;
        private Button myButton;
        //	private bool hasChanged = false;

        void Awake()
        {
            myImage = GetComponent<Image>();
            myButton = GetComponent<Button>();

            ChangeStatus(true);
        }

        void OnEnable()
        {
            CheckVisibleAndChangeStatus();
        }

        void Start()
        {

            //c.onClick.AddListener(()=> {
            UnityEngine.Events.UnityAction action = delegate
            {
                //BtnClicked(index);
                MUP_Debug.DisplayContent(textDisplay.text);
            };

            GetComponent<Button>().onClick.AddListener(action);
        }

        /*public void BtnClicked(int param)
        {
            RealtorMenuManager.Instance.SelectBuild ( param );
        }*/

        void Update()
        {
            CheckVisibleAndChangeStatus();
        }

        public void SetLabel(string text)
        {
            textDisplay.text = text;

            //		hasChanged = true;
        }

        /*public void LateUpdate()
        {
            if (!hasChanged)
                return;

            Vector2 s = Vector2.zero;

            s.y = textDisplay.GetComponent<RectTransform> ().sizeDelta.y;

            this.GetComponent<RectTransform> ().sizeDelta = s;
        }*/

        void ChangeStatus(bool state)
        {
            myImage.enabled = state;
            myButton.enabled = state;
            textDisplay.gameObject.SetActive(state);
        }

        void CheckVisibleAndChangeStatus()
        {
            Rect screenRect = new Rect(0, 60, Screen.width, Screen.height);
            bool isInside = screenRect.Contains(this.transform.position);

            if (isInside && !myImage.enabled)
            {
                ChangeStatus(true);
            }
            else if (!isInside && myImage.enabled)
            {
                ChangeStatus(false);
            }
        }
    }
}
