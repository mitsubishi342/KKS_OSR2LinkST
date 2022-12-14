using UnityEngine;
using BepInEx;
using HarmonyLib;
using System;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using System.Collections.Generic;
using Manager;
using KKAPI;
namespace kks_osr2Link
{
    [BepInPlugin("org.bepinex.plugins.KKS_OSR2LinkST", "KKS_OSR2LinkST", "1.1.0")]
    [BepInProcess("CharaStudio")]

    public class kks_osr2Link : BaseUnityPlugin
    {
        Rect windowRect = new Rect(((Screen.width / 2) - (350 / 2)), ((Screen.height / 2) - (600 / 2)), 350, 600);
        GameObject myObjA;
        //GameObject myObjB;
        String Tcode;
        SerialPort serial;
        bool syncCylinBtn = false;
        bool syncChrBtn = false;
        bool spacekey = false;
        bool autoMoveSW = false;
        bool inverseSW = false;
        bool toggleRNDRol = false;
        bool toggleRNDStrk = false;
        bool wkey = false;
        int selGridIntB = 0;
        int nowChar = 0;
        string[] selStringsB = { "L/R", "F/B", "spinL", "spinR", "Twist", "Infinit", "Infinit2", "Omega" };
        string[] selStringsC = { "OSR2 CONNECT", "DISCONNECT" };
        string[] charListC = { "" };
        string[] parentBtn = { "JointIK", "Reset" };
        float speedA;
        float speedB;
        float rollweight = 0.01f;
        float swing;
        float rollX;
        float rollZ;
        float swingPC;
        float rollXPC;
        float rollZPC;
        float minValue = 100;
        float maxValue = 800;
        float minmax;
        float nowValue;
        float nowValueB;
        float nowValueC;
        float sinwave;
        float coswave;
        float sinwaveB;
        float coswaveB;
        float timeBR;
        float timeAR;
        float tt;
        float rr;
        float count;
        Texture2D winBackground;
        Texture2D txtBackground;
        GUIStyle winStyle;
        GUIStyle txtStyle;
        bool linK;
        String stringToEdit = "Disconnect";
        Vector2 _scroll;
        int _selected;
        //Vector2 _scrollChr;
        int _selectedChr;
        private ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Show { get; set; }
        private ConfigEntry<int>[] _Port { get; set; }
        //int fCount;
        int fRange = 25;
        List<ChaControl> charList = new List<ChaControl> { };
        bool firstLoad = true;
        public kks_osr2Link()
        {
            Show = Config.AddSetting("Hotkeys", "Show window", new BepInEx.Configuration.KeyboardShortcut(KeyCode.G));
        }

        void Start()
        {
        }

        void Update()
        {

            //Logger.LogInfo("nowValue" + ((currentFemaleData != null) ? currentFemaleData.fullName : null) ?? string.Empty);
            if (firstLoad)
            {
                new GameObject("CornX").transform.parent = GameObject.Find("CommonSpace").transform;
                new GameObject("CylinX").transform.parent = GameObject.Find("CommonSpace").transform;
                firstLoad = false;
            }

            if (syncCylinBtn)
            {
                if (GameObject.Find("p_koi_stu_cylinder01_02") != null)
                {
                    myObjA = GameObject.Find("p_koi_stu_cylinder01_02");
                    if (autoMoveSW)
                    {
                        CylinderMovefunc();
                    }
                }
            }
            if (syncChrBtn)
            {
                if (GameObject.Find("chaF_00" + (_selectedChr + 1)) != null)
                {
                    if (autoMoveSW)
                    {
                        CylinderMovefunc();
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (spacekey == false)
                {
                    spacekey = true;
                }
                else
                {
                    spacekey = false;
                }
            }
            if (Show.Value.IsUp())
            {
                if (wkey == false)
                {
                    wkey = true;
                }
                else
                {
                    wkey = false;
                }
            }
        }

        void OnGUI()
        {
            if (spacekey == false && wkey)
            {
                windowRect.position = GUILayout.Window(6123, windowRect, MovableWindow, "--- Studio OSR2 Link ---").position;
                KKAPI.Utilities.IMGUIUtils.EatInputInRect(windowRect);
                var centeredStyle = GUI.skin.textField;
                centeredStyle.alignment = TextAnchor.UpperCenter;
            }
        }
        private void MovableWindow(int windowId)
        {
            GUILayout.BeginHorizontal();
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Width(100));
            _selected = GUILayout.SelectionGrid(_selected, SerialPort.GetPortNames(), 1);
            GUILayout.EndScrollView();
            if (GUILayout.Button("CONNECT") && linK == false)
            {
                try
                {
                    serial = new SerialPort("COM" + _selected.ToString(), 115200);
                    serial.Open();
                    serial.ReadTimeout = 10;
                    serial.WriteLine("L0500 L1500 R1500");
                    stringToEdit = "Connect.";
                    txtBackground.SetPixel(0, 0, new Color(0, 1, 0, 0.25f));
                    txtBackground.Apply();
                    linK = true;
                }
                catch
                {
                    txtBackground.SetPixel(0, 0, new Color(1, 0, 0, 0.25f));
                    txtBackground.Apply();
                    stringToEdit = "Error...";
                    serial.Close();
                    linK = false;
                }
            }
            if (GUILayout.Button("DISCONNECT"))
            {
                if (linK)
                {
                    serial.Close();
                    stringToEdit = "Disconnect...";
                    txtBackground.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
                    txtBackground.Apply();
                    linK = false;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (syncChrBtn == false)
            {
                syncCylinBtn = GUILayout.Toggle(syncCylinBtn, "Sync cylinder");
            }
            if (syncCylinBtn == false)
            {
                syncChrBtn = GUILayout.Toggle(syncChrBtn, "Sync charactor");
            }
            GUILayout.EndHorizontal();
            if (syncChrBtn)
            {
                _selectedChr = GUILayout.SelectionGrid(_selectedChr, string.Join(",", GetFemaleAll()).Split(','), 2);
                if (_selectedChr > 9)
                {
                    _selectedChr = 9;
                }
                if (CharChange(_selectedChr, nowChar))
                {
                    nowChar = _selectedChr;
                }
            }
            if (syncCylinBtn || syncChrBtn)
            {
                GUILayout.BeginHorizontal();
                autoMoveSW = GUILayout.Toggle(autoMoveSW, "Auto Move");
                inverseSW = GUILayout.Toggle(inverseSW, "Inverse Z");
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("[ Stroke section ]", winStyle);
            if (syncCylinBtn || syncChrBtn)
            {
                toggleRNDStrk = GUILayout.Toggle(toggleRNDStrk, "Random Stroke");
                GUILayout.Label("Speed");
                speedA = GUILayout.HorizontalSlider(speedA, 0, 15);
            }
            GUILayout.Label("Range");
            MyGUI.MinMaxSlider(ref minValue, ref maxValue, 0, 1000);
            GUILayout.Label("[ Swing section ]", winStyle);
            if (syncCylinBtn || syncChrBtn)
            {
                toggleRNDRol = GUILayout.Toggle(toggleRNDRol, "Random Swing");
                GUILayout.Label("Speed");
                speedB = GUILayout.HorizontalSlider(speedB, 0, 15);
            }
            GUILayout.Label("weight");
            rollweight = GUILayout.HorizontalSlider(rollweight, 0, 1);
            selGridIntB = GUILayout.SelectionGrid(selGridIntB, selStringsB, 4);
            stringToEdit = GUILayout.TextArea(stringToEdit, 300, txtStyle);
            GUI.Button(new Rect(0, 25, 345, 595), "", winStyle);
            KKAPI.Utilities.IMGUIUtils.DragResizeEatWindow(6123, windowRect);
        }

        void CylinderMovefunc()
        {
            if (syncCylinBtn)
            {
                fRange = 25;
            }
            if (syncChrBtn)
            {
                fRange = 20;
            }
            timeAR = timeAR + speedA * minmax * 0.01f;
            timeBR = timeBR + speedB * 0.01f;
            minmax = ((minValue + (1000 - maxValue)) / 300);
            minmax = Mathf.Clamp(minmax, 1, 9);
            nowValue = Wavegen("sin", timeAR, 1);//stroke
            if (toggleRNDStrk)
            {
                rr = Mathf.Sin(timeAR) / 12;
                tt = tt + rr + Mathf.Sin(Time.time * 2) * 0.01f;
                nowValue = Mathf.Sin(tt);
            }
            nowValueB = MathfConv.ChangeRange(-1, 1, nowValue, minValue, maxValue);
            nowValueC = nowValueB;
            sinwave = Wavegen("sin", timeBR, rollweight);
            coswave = Wavegen("cos", timeBR, rollweight);
            sinwaveB = MathfConv.ChangeRange(-1, 1, sinwave, -fRange, fRange); //roll
            coswaveB = MathfConv.ChangeRange(-1, 1, coswave, -fRange, fRange);
            if (toggleRNDRol)
            {
                if (Time.time % UnityEngine.Random.Range(2, 4) < 0.02f)
                {
                    selGridIntB = UnityEngine.Random.Range(4, 7);
                }
            }
            switch (selGridIntB)//ロール動作切り替え
            {
                case 0:
                    SendObj(0, 0, sinwaveB);
                    rollXPC = rollXPC - 0.01f;
                    if (rollXPC < 0) { rollXPC = 0; }
                    rollZPC = sinwave;
                    break;
                case 1:
                    SendObj(sinwaveB, 0, 0);
                    rollXPC = -sinwave;
                    rollZPC = rollZPC - 0.01f;
                    if (rollZPC < 0) { rollZPC = 0; }
                    break;
                case 2:
                    SendObj(coswaveB, 0, sinwaveB);
                    rollXPC = sinwave;
                    rollZPC = coswave;
                    break;
                case 3:
                    SendObj(sinwaveB, 0, coswaveB);
                    rollXPC = coswave;
                    rollZPC = sinwave;
                    break;
                case 4:
                    count = count + (speedB / 7) * 0.01f;
                    rollZPC = Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight;
                    rollXPC = -(Mathf.Cos(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight);
                    SendObj(-rollXPC * fRange, 0, rollZPC * fRange);
                    break;
                case 5:
                    count = count + (speedB / 7) * 0.01f;
                    rollZPC = -(Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight);
                    rollXPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 2) * rollweight);
                    SendObj(-rollXPC * fRange, 0, rollZPC * fRange);
                    break;
                case 6:
                    count = count + (speedB / 7) * 0.01f;
                    rollZPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight);
                    rollXPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 2) * rollweight);
                    SendObj(rollZPC * fRange, 0, rollXPC * fRange);
                    break;
                case 7:
                    count = count + (speedB / 7) * 0.01f;
                    rollZPC = -(Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 2) * rollweight);
                    rollXPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 3) * rollweight);
                    SendObj(-rollXPC * fRange, 0, rollZPC * fRange);
                    break;
            }
            if (syncChrBtn)
            {
                myObjA.transform.localPosition = new Vector3(0, nowValueC / 7000, 0);// push Obj
            }
            if (syncCylinBtn)
            {
                myObjA.transform.localPosition = new Vector3(0, nowValueC / 8000, 0);// push Obj
            }

            swingPC = nowValueC;
            if (swingPC > 900) { swingPC = 900; }
            if (swingPC < 100) { swingPC = 100; }
            swing = swingPC;
            rollX = MathfConv.ChangeRange(-1, 1, rollXPC, 100, 900);
            rollZ = MathfConv.ChangeRange(-1, 1, rollZPC, 100, 900);
            Osr2senddata();
        }

        void Osr2senddata()
        {
            if (linK)
            {
                try
                {
                    if (inverseSW)
                    {
                        Tcode = "L0" + Convert.ToString((int)swing)
                                + " L1" + Convert.ToString(1000 - (int)rollX)
                                + " R1" + Convert.ToString(1000 - (int)rollZ);
                        serial.WriteLine(Tcode);
                    }
                    else
                    {
                        Tcode = "L0" + Convert.ToString((int)swing)
                                + " L1" + Convert.ToString((int)rollX)
                                + " R1" + Convert.ToString((int)rollZ);
                        serial.WriteLine(Tcode);
                    }
                }
                catch
                {
                    serial.Close();
                    stringToEdit = "WriteError! Disconnect...";
                    txtBackground.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
                    txtBackground.Apply();
                }
            }
        }

        private class MathfConv
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ChangeRange(float min, float max, float value,
            float newMin, float newMax)
            {
                return Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(min, max, value));
            }
        }
        private void Awake()
        {
            winBackground = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            winBackground.SetPixel(0, 0, new Color(1, 1, 1, 0.25f));
            winBackground.Apply(); // not sure if this is necessary
            txtBackground = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            txtBackground.SetPixel(0, 0, new Color(0, 0, 0, 0.25f));
            txtBackground.Apply(); // not sure if this is necessary
            winStyle = new GUIStyle(GUIStyle.none);
            winStyle.fontSize = 18;
            winStyle.normal.textColor = Color.white;
            winStyle.normal.background = winBackground;
            winStyle.alignment = TextAnchor.MiddleCenter;
            txtStyle = new GUIStyle(GUIStyle.none);
            txtStyle.fontSize = 20;
            txtStyle.normal.textColor = Color.white;
            txtStyle.normal.background = txtBackground;
            txtStyle.alignment = TextAnchor.MiddleCenter;
        }

        private static class MyGUI
        {
            private static GUIStyle backgroundStyle = "Box";
            private static GUIStyle thumbStyle = "Box";
            private static GUIStyle minThumbStyle = "verticalScrollbarThumb";
            private static GUIStyle maxThumbStyle = "verticalScrollbarThumb";

            private static void DoMinMaxSlider(Rect position, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
            {
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                int minThumbControlID = GUIUtility.GetControlID(FocusType.Passive);
                int maxThumbControlID = GUIUtility.GetControlID(FocusType.Passive);
                if (Event.current.type == EventType.Layout)
                    return;
                minValue = Mathf.Clamp(minValue, minLimit, maxLimit);
                maxValue = Mathf.Max(minValue, Mathf.Clamp(maxValue, minLimit, maxLimit));
                float range = Mathf.Abs(maxLimit - minLimit);
                float normalizedMinValue = (minValue - minLimit) / range;
                float normalizedMaxValue = (maxValue - minLimit) / range;
                float minValueX = position.x + normalizedMinValue * position.width;
                float maxValueX = position.x + normalizedMaxValue * position.width;
                Rect minThumbPosition = new Rect(minValueX - 5, position.y, 10, position.height);
                Rect maxThumbPosition = new Rect(maxValueX, position.y, 10, position.height);
                float normalizedMousePosition;
                switch (Event.current.GetTypeForControl(controlID))
                {
                    case EventType.MouseDown:
                        if (minThumbPosition.Contains(Event.current.mousePosition))
                        {
                            GUIUtility.hotControl = minThumbControlID;
                            Event.current.Use();
                        }
                        else if (maxThumbPosition.Contains(Event.current.mousePosition))
                        {
                            GUIUtility.hotControl = maxThumbControlID;
                            Event.current.Use();
                        }
                        break;
                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == minThumbControlID || GUIUtility.hotControl == maxThumbControlID || GUIUtility.hotControl == controlID)
                            GUIUtility.hotControl = 0;
                        break;
                    case EventType.MouseDrag:
                        normalizedMousePosition = (Event.current.mousePosition.x - position.x) / position.width;
                        if (GUIUtility.hotControl == minThumbControlID)
                        {
                            float newMinValue = Mathf.Clamp(normalizedMousePosition * range + minLimit, minLimit, maxValue);
                            if (newMinValue != minValue)
                            {
                                minValue = newMinValue;
                                GUI.changed = true;
                            }
                            Event.current.Use();
                        }
                        else if (GUIUtility.hotControl == maxThumbControlID)
                        {
                            float newMaxValue = Mathf.Clamp(normalizedMousePosition * range + minLimit, minValue, maxLimit);
                            if (newMaxValue != maxValue)
                            {
                                maxValue = newMaxValue;
                                GUI.changed = true;
                            }
                            Event.current.Use();
                        }
                        break;
                    case EventType.Repaint:
                        backgroundStyle.Draw(new Rect(position.x, position.y + 5, position.width, position.height - 10), GUIContent.none, controlID);
                        thumbStyle.Draw(new Rect(minValueX, position.y + 9, maxValueX - minValueX, 5), GUIContent.none, false, false, false, false);
                        minThumbStyle.Draw(minThumbPosition, GUIContent.none, minThumbControlID);
                        maxThumbStyle.Draw(maxThumbPosition, GUIContent.none, maxThumbControlID);
                        break;
                }
            }
            public static void MinMaxSlider(Rect position, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
            {
                DoMinMaxSlider(position, ref minValue, ref maxValue, minLimit, maxLimit);
            }
            public static void MinMaxSlider(ref float minValue, ref float maxValue, float minLimit, float maxLimit)
            {
                Rect position = GUILayoutUtility.GetRect(GUIContent.none, backgroundStyle);
                DoMinMaxSlider(position, ref minValue, ref maxValue, minLimit, maxLimit);
            }
        }

        private float Wavegen(string sincos, float wavelength, float amplitude)
        {
            var wave = 0f;
            if (sincos == "sin")
            {
                wave = Mathf.Sin(wavelength) * amplitude;
            }
            if (sincos == "cos")
            {
                wave = Mathf.Cos(wavelength) * amplitude;
            }
            return wave;
        }

        private bool CharChange(int charno, int nowchar)
        {
            var cha = "CommonSpace/chaF_00" + (charno + 1);
            if (GameObject.Find("CommonSpace/chaF_00" + (nowchar + 1)) != null && GameObject.Find("CommonSpace/chaF_00" + (nowchar + 1) + "/CornX/CylinX/cf_t_hips(work)") != null)
            {
                GameObject.Find("CylinX").transform.localRotation = default;
                GameObject.Find("CylinX").transform.localPosition = default;
                GameObject.Find("CommonSpace/chaF_00" + (nowchar + 1) + "/CornX/CylinX/cf_t_hips(work)").transform.SetParent(GameObject.Find("CommonSpace/chaF_00" + (nowchar + 1)).transform);
                GameObject.Find("CylinX").transform.SetParent(GameObject.Find("CommonSpace").transform);
                GameObject.Find("CornX").transform.SetParent(GameObject.Find("CommonSpace").transform);
            }

            if (GameObject.Find(cha) != null && GameObject.Find(cha + "/cf_t_hips(work)") != null)
            {
                GameObject.Find("CylinX").transform.SetParent(GameObject.Find("CornX").transform);
                GameObject.Find("CornX").transform.position = GameObject.Find(cha + "/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_d_kokan/cf_j_kokan").transform.position;
                GameObject.Find("CornX").transform.eulerAngles = GameObject.Find(cha + "/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_d_kokan/cf_j_kokan").transform.eulerAngles;
                GameObject.Find("CornX").transform.SetParent(GameObject.Find(cha).transform);
                GameObject.Find(cha + "/cf_t_hips(work)").transform.SetParent(GameObject.Find("CylinX").transform);

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
                if (syncChrBtn)
                {
                    if (GameObject.Find("CornX") == null)
                    { new GameObject("CornX").transform.parent = GameObject.Find("CommonSpace").transform; }
                    if (GameObject.Find("CylinX") != null)
                    { new GameObject("CylinX").transform.parent = GameObject.Find("CommonSpace").transform; }

                    if (CharChange(_selectedChr, nowChar))
                    {
                        nowChar = _selectedChr;
                    }
                }
                if (syncCylinBtn)
                {
                    if (GameObject.Find("p_koi_stu_cylinder01_02") != null)
                    {
                        myObjA = GameObject.Find("p_koi_stu_cylinder01_02");
                    }
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
            List<ChaControl> charaList = Character.GetCharaList(1);
            bool flag = charaList == null || charaList.Count == 0;
            List<ChaControl> result;
            if (flag)
            {
                result = list;
            }
            else
            {
                for (int i = 0; i <= charaList.Count - 1; i++)
                {
                    bool hiPoly = charaList[i].hiPoly;
                    if (hiPoly)
                    {
                        list.Add(charaList[i]);
                    }
                }
                result = list;
            }
            return result;
        }
    }
}