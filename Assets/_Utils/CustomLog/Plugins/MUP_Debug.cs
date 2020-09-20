/**
 * MUP_Debug
 * Autor: Eder Moreira
 * Organization: TI & MUP Studios Ltda (www.mupstudios.com)
 * Pakage Version: 1.19R
 * Unity Version: 5.4.0b22
 * Update 1.19
 *  - Application.LoadLevel (Obsolete) changed for SceneManage.LoadScene
 *  - OnLevelWasLoaded (decrepted) changed for OnLevelWasLoadedHandler and executed on Awake
 *  - UnityEngine.EventSystems.TouchInputModule removed from EventSystem
 * Update 1.18
 *  - ExecuteInEditMode removido
 *  - Interface adaptada para começar escondida no editor
 *  - Distância de deslocamento dos menus alterada
 * Update 1.16
 *  - ExecuteInEditMode adicionado
 * Update 1.15
 *  - Possibilidade de ocultar a barra de debug restando apenas um botão para amplia-la.
 * Update 1.1
 *  - Criar EventSystem caso ele não seja encontrado
 *  - Opção de tornar persistente o canvas
 *  - Atualização da profundidade do canvas, agora ele irá aparecer na frente de outras UIs
 *  - Ajuste da barra inferior para ocupar toda a parte inferior da tela, ajuste da caixa de texto
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;

namespace Mup.CustomLog
{
    [ScriptOrder(-31000)]
    public class MUP_Debug : MonoBehaviour
    {

        static private MUP_Debug _instance;

        static private MUP_Debug Instance
        {
            get
            {
                return _instance;
            }
        }

        static private readonly string LOG_TYPE = "LOG";
        static private readonly string ERROR_TYPE = "ERROR";
        static private readonly string EXCEPTION_TYPE = "EXCEPTION";
        static private readonly string WARNING_TYPE = "WARNING";

        private static int count = 0;
        private static int newMessages = -1;

        [SerializeField]
        private Text minMaxButtonLabel;

        [SerializeField]
        private GameObject panel;

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private GameObject cellPrefab;

        [SerializeField]
        private GameObject eventSystemPrefab;

        [SerializeField]
        private Transform hideButton;

        [SerializeField]
        private Transform debugBar;

        [SerializeField]
        private Text mainLabel;

        [SerializeField]
        private Text simpleDisplay;

        [SerializeField]
        private bool dontDestroyOnLoad = true;

        private List<GameObject> list = new List<GameObject>();
        private List<GameObject> deactivedList = new List<GameObject>();

        private float lastTime = 0;
        
        void Awake()
        {
            if (!Debug.isDebugBuild || (_instance != null && _instance != this))
            {
                Destroy(this.gameObject);
                return;
            }

            Application.logMessageReceived += HandleLog;

            _instance = this;
            count = 0;
            newMessages = -1;
            mainLabel.text = "";
            simpleDisplay.text = "";

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);

            OnLevelWasLoadedHandler();

            Hide();

            hideButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            debugBar.GetComponent<RectTransform>().anchoredPosition += Vector2.up * 93;

        }



        void Start()
        {
            StartCoroutine(GarbageCollector());
        }

        void OnLevelWasLoadedHandler()
        {

            if (!GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>())
            {
                CreateEventSystem();
            }
        }

        private void CreateEventSystem()
        {
            GameObject g = new GameObject("EventSystem");

            g.AddComponent<UnityEngine.EventSystems.EventSystem>();
            g.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }


        static public void Log(string message, string stackTrack)
        {
            if (!_instance)
                return;

            SetMessage(message, LOG_TYPE, "white", stackTrack);
        }

        static public void LogError(string message, string stackTrack)
        {
            if (!_instance)
                return;

            SetMessage(message, ERROR_TYPE, "red", stackTrack);
        }

        static public void LogWarning(string message, string stackTrack)
        {
            if (!_instance)
                return;

            SetMessage(message, WARNING_TYPE, "yellow", stackTrack);
        }

        static public void LogException(string message, string stackTrack)
        {
            if (!_instance)
                return;

            SetMessage(message, EXCEPTION_TYPE, "red", stackTrack);
        }

        static private void SetMessage(string message, string messageType, string color, string stackTrack)
        {
            bool updateScroll = (Instance.scrollRect.normalizedPosition.y < 0.05f);
            //get the function name who calls the log message
            //string trace = StackTraceUtility.ExtractStackTrace ();
            //string[] s = trace.Split ("\n"[0]);

            Instance.simpleDisplay.text = "<color=" + color + ">" + message + "</color>";

            count = count + 1;
            string t = "<color=" + color + "><b>[" + messageType + ":" + count + "]</b> {" + message + "}\n" + stackTrack + "</color>";

            /*for (int i = 2; i < s.Length; i++) {
                t += ("\n"+s [i]);
            }*/
            //t += "</color>";
            Instance.CreateCell(t);

            if (newMessages != -1)
            {
                newMessages = newMessages + 1;

                if (newMessages > 0)
                {
                    if (newMessages < 99)
                        Instance.minMaxButtonLabel.text = "Debug (" + newMessages + ")";
                    else
                        Instance.minMaxButtonLabel.text = "Debug (99+)";
                }
            }

            if (updateScroll)
            {
                Instance.scrollRect.normalizedPosition = Vector2.zero;
            }
        }

        private void CreateCell(string text)
        {
            GameObject g;

            if (deactivedList.Count == 0)
            {
                g = (GameObject)Instantiate(cellPrefab);
            }
            else
            {
                g = deactivedList[0];
                deactivedList.RemoveAt(0);
                g.SetActive(true);

            }

            g.SendMessage("ChangeStatus", true);

            g.transform.SetParent(scrollRect.content);

            g.transform.localPosition = Vector3.down * (list.Count * 60);
            g.transform.localScale = Vector3.one;
            g.GetComponent<RectTransform>().sizeDelta = Vector2.up * 60;
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, 60 * (list.Count + 1));
            g.GetComponent<MUP_DebugCell>().SetLabel(text);


            list.Add(g);
        }

        static public void Clear()
        {
            Instance.Erase();
        }

        static public void DisplayContent(string text)
        {
            Instance.mainLabel.text = text;
        }

        public void Erase()
        {
            foreach (GameObject g in list)
            {
                g.SetActive(false);
                deactivedList.Add(g);
                g.transform.SetParent(null);
            }

            list = new List<GameObject>();
            mainLabel.text = "";
            simpleDisplay.text = "";

            if (!panel.activeSelf)
            {
                newMessages = 0;
                minMaxButtonLabel.text = "Debug";
            }

            scrollRect.content.sizeDelta = Vector2.zero;
            Instance.scrollRect.normalizedPosition = Vector2.one;
        }

        public void SwitchState()
        {
            if (!panel.activeSelf)
                Open();
            else
                Close();
        }

        public void Close()
        {
            minMaxButtonLabel.text = "Debug";
            panel.SetActive(false);

            newMessages = 0;
        }
        public void Open()
        {
            minMaxButtonLabel.text = "Fechar";

            scrollRect.normalizedPosition = Vector2.zero;

            panel.SetActive(true);

            newMessages = -1;
        }

        private IEnumerator GarbageCollector()
        {
            while (true)
            {
                if (deactivedList.Count > 0 && Time.timeSinceLevelLoad - lastTime > 10)
                {
                    Destroy(deactivedList[0]);
                    deactivedList.RemoveAt(0);
                    lastTime = Time.timeSinceLevelLoad;
                }
                yield return new WaitForEndOfFrame();
            }
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }
        void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Log)
                Log(logString, stackTrace);
            else if (type == LogType.Warning)
                LogWarning(logString, stackTrace);
            else if (type == LogType.Error)
                LogError(logString, stackTrace);
            else if (type == LogType.Exception)
                LogException(logString, stackTrace);
            else
                Log(logString, stackTrace);
        }

        public void Show()
        {
            debugBar.gameObject.SetActive(true);
            debugBar.GetComponent<RectTransform>().anchoredPosition += Vector2.up * 93;
            hideButton.GetComponent<RectTransform>().anchoredPosition += Vector2.up * 93;
            hideButton.GetComponentInChildren<Text>().text = ">>";
        }

        public void Hide()
        {
            Close();

            debugBar.gameObject.SetActive(false);
            debugBar.GetComponent<RectTransform>().anchoredPosition -= Vector2.up * 93;
            hideButton.GetComponent<RectTransform>().anchoredPosition -= Vector2.up * 93;
            hideButton.GetComponentInChildren<Text>().text = "<<";
        }

        public void ChangeBarState()
        {
            if (debugBar.gameObject.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }
}