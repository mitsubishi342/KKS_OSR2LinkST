using UnityEngine;
using BepInEx;
using HarmonyLib;
using System;
using System.IO.Ports;
using BepInEx.Configuration;
using System.Collections.Generic;
using Manager;
using KKAPI;
using KKABMX.Core;

namespace kks_osr2Link
{
    [BepInPlugin("org.bepinex.plugins.KKS_OSR2LinkST", "KKS_OSR2LinkST", "1.1.0")]
    [BepInProcess("CharaStudio")]
    [BepInProcess("KoikatsuSunshine")]

    public partial class kks_osr2Link : BaseUnityPlugin
    {
        private Rect windowRect = new Rect((Screen.width - 350) / 2, (Screen.height - 600) / 2, 350, 600);
        private GameObject myObjA;
        private string Tcode;
        private bool syncCylinBtn = false;
        private bool syncChrBtn = false;
        private bool spacekey = false;
        private bool autoMoveSW = false;
        private bool inverseSW = false;
        private bool toggleRNDRol = false;
        private bool toggleRNDStrk = false;
        private bool wkey = false;
        private int selGridIntB = 0;
        private int nowChar = 0;
        private string[] selStringsB = { "L/R", "F/B", "spinL", "spinR", "Twist", "Infinit", "Infinit2", "Omega" };
        private string[] selStringsC = { "OSR2 CONNECT", "DISCONNECT" };
        private string[] charListC = { "" };
        private string[] parentBtn = { "JointIK", "Reset" };
        private float speedA;
        private float speedB;
        private float rollweight = 0.01f;
        private float swing;
        private float rollX;
        private float rollZ;
        private float swingPC;
        private float rollXPC;
        private float rollZPC;
        private float minValue = 100;
        private float maxValue = 800;
        private float minmax;
        private float nowValue;
        private float nowValueB;
        private float nowValueC;
        private float sinwave;
        private float coswave;
        private float sinwaveB;
        private float coswaveB;
        private float timeBR;
        private float timeAR;
        private float tt;
        private float rr;
        private float count;
        private Texture2D winBackground;
        private Texture2D txtBackground;
        private GUIStyle winStyle;
        private GUIStyle txtStyle;
        private bool linK;
        private string stringToEdit = "Disconnect";
        private Vector2 _scroll;
        private int _selected;
        private int _selectedChr;
        private ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Show { get; set; }
        private ConfigEntry<int>[] _Port { get; set; }
        private bool firstLoad = true;
        private StudioScene studioScene;
        private HScene hScene;
        private SerialPort serial = new SerialPort() { BaudRate = 115200 };

        public kks_osr2Link()
        {
            Show = Config.AddSetting("Hotkeys", "Show window", new BepInEx.Configuration.KeyboardShortcut(KeyCode.G));
        }

        private void Start()
        {
            InitializeGUI();
        }

        private void Update()
        {
            studioScene = studioScene ?? FindObjectOfType<StudioScene>();
            hScene = hScene ?? FindObjectOfType<HScene>();

            if (firstLoad)
            {
                InitializeGameObjects();
                firstLoad = false;
            }

            HandleInput();
            HandleSync();
        }

        private void OnGUI()
        {
            if (!spacekey && wkey)
            {
                windowRect = GUILayout.Window(6123, windowRect, MovableWindow, "--- Studio OSR2 Link ---");
                KKAPI.Utilities.IMGUIUtils.EatInputInRect(windowRect);
            }
        }

        private void MovableWindow(int windowId)
        {
            GUILayout.BeginHorizontal();
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Width(100));
            _selected = GUILayout.SelectionGrid(_selected, SerialPort.GetPortNames(), 1);
            GUILayout.EndScrollView();
            GUILayout.Label(SerialPort.GetPortNames()[_selected]);

            if (GUILayout.Button("CONNECT") && !linK)
            {
                ConnectSerialPort();
            }

            if (GUILayout.Button("DISCONNECT") && linK)
            {
                DisconnectSerialPort();
            }

            GUILayout.EndHorizontal();
            HandleSyncOptions();
            GUILayout.Label("[ Stroke section ]", winStyle);
            HandleStrokeOptions();
            GUILayout.Label("[ Swing section ]", winStyle);
            HandleSwingOptions();
            stringToEdit = GUILayout.TextArea(stringToEdit, 300, txtStyle);
            GUI.Button(new Rect(0, 25, 345, 595), "", winStyle);
            KKAPI.Utilities.IMGUIUtils.DragResizeEatWindow(6123, windowRect);
        }

        private void HandleInput()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                spacekey = !spacekey;
            }

            if (Show.Value.IsUp())
            {
                wkey = !wkey;
            }
        }

        private void HandleSync()
        {
            if (syncCylinBtn && myObjA == null)
            {
                myObjA = GameObject.Find("p_koi_stu_cylinder01_02");
            }

            if (autoMoveSW && (syncCylinBtn || syncChrBtn))
            {
                CylinderMovefunc();
            }
        }

        private void InitializeGUI()
        {
            winBackground = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            winBackground.SetPixel(0, 0, new Color(1, 1, 1, 0.25f));
            winBackground.Apply();
            txtBackground = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            txtBackground.SetPixel(0, 0, new Color(0, 0, 0, 0.25f));
            txtBackground.Apply();
            winStyle = new GUIStyle(GUIStyle.none)
            {
                fontSize = 18,
                normal = { textColor = Color.white, background = winBackground },
                alignment = TextAnchor.MiddleCenter
            };
            txtStyle = new GUIStyle(GUIStyle.none)
            {
                fontSize = 20,
                normal = { textColor = Color.white, background = txtBackground },
                alignment = TextAnchor.MiddleCenter
            };
        }

        private void InitializeGameObjects()
        {
            new GameObject("CornX").transform.parent = GameObject.Find("CommonSpace").transform;
            new GameObject("CylinX").transform.parent = GameObject.Find("CommonSpace").transform;
        }

        private void ConnectSerialPort()
        {
            try
            {
                serial.PortName = SerialPort.GetPortNames()[_selected];
                serial.Open();
                serial.ReadTimeout = 10;
                serial.WriteLine("L0500 L1500 R1500");
                stringToEdit = "Connect.";
                UpdateTextColor(new Color(0, 1, 0, 0.25f));
                linK = true;
            }
            catch
            {
                UpdateTextColor(new Color(1, 0, 0, 0.25f));
                stringToEdit = "Error...";
                serial.Close();
                linK = false;
            }
        }

        private void DisconnectSerialPort()
        {
            serial.Close();
            stringToEdit = "Disconnect...";
            UpdateTextColor(new Color(0, 0, 0, 0.5f));
            linK = false;
        }

        private void UpdateTextColor(Color color)
        {
            txtBackground.SetPixel(0, 0, color);
            txtBackground.Apply();
        }

        private void HandleSyncOptions()
        {
            GUILayout.BeginHorizontal();
            if (!syncChrBtn && studioScene)
            {
                syncCylinBtn = GUILayout.Toggle(syncCylinBtn, "Sync cylinder");
            }
            if (!syncCylinBtn && studioScene)
            {
                syncChrBtn = GUILayout.Toggle(syncChrBtn, "Sync character");
            }
            GUILayout.EndHorizontal();

            if (syncChrBtn && studioScene)
            {
                _selectedChr = GUILayout.SelectionGrid(_selectedChr, string.Join(",", GetFemaleAll()).Split(','), 2);
                if (_selectedChr > 9) _selectedChr = 9;
                if (CharChange(_selectedChr, nowChar)) nowChar = _selectedChr;
            }

            if (syncCylinBtn || syncChrBtn)
            {
                GUILayout.BeginHorizontal();
                if (studioScene) autoMoveSW = GUILayout.Toggle(autoMoveSW, "Auto Move");
                inverseSW = GUILayout.Toggle(inverseSW, "Inverse Z");
                GUILayout.EndHorizontal();
            }
        }

        private void HandleStrokeOptions()
        {
            if (syncCylinBtn || syncChrBtn)
            {
                if (studioScene)
                {
                    toggleRNDStrk = GUILayout.Toggle(toggleRNDStrk, "Random Stroke");
                    GUILayout.Label("Speed");
                    speedA = GUILayout.HorizontalSlider(speedA, 0, 15);
                }
            }
            GUILayout.Label("Range");
            MyGUI.MinMaxSlider(ref minValue, ref maxValue, 0, 1000);
        }

        private void HandleSwingOptions()
        {
            if (studioScene)
            {
                if (syncCylinBtn || syncChrBtn)
                {
                    toggleRNDRol = GUILayout.Toggle(toggleRNDRol, "Random Swing");
                    GUILayout.Label("Speed");
                    speedB = GUILayout.HorizontalSlider(speedB, 0, 15);
                }
            }
            GUILayout.Label("weight");
            rollweight = GUILayout.HorizontalSlider(rollweight, 0, 1);
            if (studioScene)
            {
                selGridIntB = GUILayout.SelectionGrid(selGridIntB, selStringsB, 4);
            }
        }

        private void CylinderMovefunc()
        {
            int fRange = syncCylinBtn ? 25 : 20;
            timeAR += speedA * minmax * 0.01f;
            timeBR += speedB * 0.01f;
            minmax = Mathf.Clamp(((minValue + (1000 - maxValue)) / 300), 1, 9);
            nowValue = toggleRNDStrk ? RandomStroke(timeAR) : Mathf.Sin(timeAR);
            nowValueB = MathfConv.ChangeRange(-1, 1, nowValue, minValue, maxValue);
            nowValueC = nowValueB;
            sinwave = Mathf.Sin(timeBR) * rollweight;
            coswave = Mathf.Cos(timeBR) * rollweight;
            sinwaveB = MathfConv.ChangeRange(-1, 1, sinwave, -fRange, fRange);
            coswaveB = MathfConv.ChangeRange(-1, 1, coswave, -fRange, fRange);

            if (toggleRNDRol && Time.time % UnityEngine.Random.Range(2, 4) < 0.02f)
            {
                selGridIntB = UnityEngine.Random.Range(4, 7);
            }

            switch (selGridIntB)
            {
                case 0:
                    RollLeftRight(sinwaveB);
                    break;
                case 1:
                    RollFrontBack(sinwaveB);
                    break;
                case 2:
                    RollSpinLeft(coswaveB, sinwaveB);
                    break;
                case 3:
                    RollSpinRight(sinwaveB, coswaveB);
                    break;
                case 4:
                    RollTwist();
                    break;
                case 5:
                    RollInfinit1();
                    break;
                case 6:
                    RollInfinit2();
                    break;
                case 7:
                    RollOmega();
                    break;
            }

            MoveObject();
            Osr2senddata();
        }

        private float RandomStroke(float time)
        {
            rr = Mathf.Sin(time) / 12;
            tt += rr + Mathf.Sin(Time.time * 2) * 0.01f;
            return Mathf.Sin(tt);
        }

        private void RollLeftRight(float sinwaveB)
        {
            SendObj(0, 0, sinwaveB);
            rollXPC = Mathf.Max(0, rollXPC - 0.01f);
            rollZPC = sinwave;
        }

        private void RollFrontBack(float sinwaveB)
        {
            SendObj(sinwaveB, 0, 0);
            rollXPC = -sinwave;
            rollZPC = Mathf.Max(0, rollZPC - 0.01f);
        }

        private void RollSpinLeft(float coswaveB, float sinwaveB)
        {
            SendObj(coswaveB, 0, sinwaveB);
            rollXPC = sinwave;
            rollZPC = coswave;
        }

        private void RollSpinRight(float sinwaveB, float coswaveB)
        {
            SendObj(sinwaveB, 0, coswaveB);
            rollXPC = coswave;
            rollZPC = sinwave;
        }

        private void RollTwist()
        {
            count += (speedB / 7) * 0.01f;
            rollZPC = Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight;
            rollXPC = -(Mathf.Cos(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight);
            SendObj(-rollXPC * 20, 0, rollZPC * 20);
        }

        private void RollInfinit1()
        {
            count += (speedB / 7) * 0.01f;
            rollZPC = -(Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight);
            rollXPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 2) * rollweight);
            SendObj(-rollXPC * 20, 0, rollZPC * 20);
        }

        private void RollInfinit2()
        {
            count += (speedB / 7) * 0.01f;
            rollZPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight);
            rollXPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 2) * rollweight);
            SendObj(rollZPC * 20, 0, rollXPC * 20);
        }

        private void RollOmega()
        {
            count += (speedB / 7) * 0.01f;
            rollZPC = -(Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 2) * rollweight);
            rollXPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 3) * rollweight);
            SendObj(-rollXPC * 20, 0, rollZPC * 20);
        }

        private void MoveObject()
        {
            if (syncChrBtn)
            {
                myObjA.transform.localPosition = new Vector3(0, nowValueC / 5500, 0);
            }
            if (syncCylinBtn)
            {
                myObjA.transform.localPosition = new Vector3(0, nowValueC / 8000, 0);
            }

            swingPC = Mathf.Clamp(nowValueC, 100, 900);
            swing = swingPC;
            rollX = MathfConv.ChangeRange(-1, 1, rollXPC, 100, 900);
            rollZ = MathfConv.ChangeRange(-1, 1, rollZPC, 100, 900);
        }

        private void Osr2senddata()
        {
            if (linK)
            {
                try
                {
                    if (inverseSW)
                    {
                        Tcode = $"L0{(int)swing} L1{1000 - (int)rollX} R1{1000 - (int)rollZ}";
                    }
                    else
                    {
                        Tcode = $"L0{(int)swing} L1{(int)rollX} R1{(int)rollZ}";
                    }
                    serial.WriteLine(Tcode);
                }
                catch
                {
                    serial.Close();
                    stringToEdit = "WriteError! Disconnect...";
                    UpdateTextColor(new Color(0, 0, 0, 0.5f));
                }
            }
        }

        private bool CharChange(int charno, int nowchar)
        {
            var cha = $"CommonSpace/chaF_00{charno + 1}";
            if (GameObject.Find($"CommonSpace/chaF_00{nowchar + 1}")?.transform.Find("CornX/CylinX/cf_t_hips(work)") != null)
            {
                var cylinX = GameObject.Find("CylinX").transform;
                cylinX.localRotation = Quaternion.identity;
                cylinX.localPosition = Vector3.zero;
                GameObject.Find($"CommonSpace/chaF_00{nowchar + 1}/CornX/CylinX/cf_t_hips(work)").transform.SetParent(GameObject.Find($"CommonSpace/chaF_00{nowchar + 1}").transform);
                cylinX.SetParent(GameObject.Find("CommonSpace").transform);
                GameObject.Find("CornX").transform.SetParent(GameObject.Find("CommonSpace").transform);
            }

            if (GameObject.Find(cha)?.transform.Find("cf_t_hips(work)") != null)
            {
                var cornX = GameObject.Find("CornX").transform;
                var chaTransform = GameObject.Find($"{cha}/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_d_kokan/cf_j_kokan").transform;
                cornX.position = chaTransform.position;
                cornX.eulerAngles = chaTransform.eulerAngles;
                cornX.SetParent(GameObject.Find(cha).transform);
                GameObject.Find($"{cha}/cf_t_hips(work)").transform.SetParent(GameObject.Find("CylinX").transform);
                GameObject.Find("CylinX").transform.SetParent(GameObject.Find("CornX").transform);

                myObjA = GameObject.Find("CylinX");
                return true;
            }
            return false;
        }

        private void SendObj(float x, float y, float z)
        {
            try
            {
                myObjA.transform.localEulerAngles = new Vector3(x, y, z);
            }
            catch
            {
                if (syncChrBtn && CharChange(_selectedChr, nowChar))
                {
                    nowChar = _selectedChr;
                }
                else if (syncCylinBtn)
                {
                    myObjA = GameObject.Find("p_koi_stu_cylinder01_02");
                }
            }
        }

        private void OnDestroy()
        {
            serial.Close();
        }

        public static List<ChaControl> GetFemaleAll()
        {
            List<ChaControl> list = new List<ChaControl>();
            var charaList = Character.GetCharaList(1);
            if (charaList == null || charaList.Count == 0) return list;

            foreach (var cha in charaList)
            {
                if (cha.hiPoly)
                {
                    list.Add(cha);
                }
            }
            return list;
        }
    }
}
