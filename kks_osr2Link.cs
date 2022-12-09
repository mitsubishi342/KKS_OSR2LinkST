using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using HarmonyLib;
using System;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using KKAPI;
using BepInEx.Configuration;

namespace kks_osr2Link
{
    [BepInPlugin("org.bepinex.plugins.KKS_OSR2LinkST", "KKS_OSR2LinkST", "1.0.0")]
    [BepInProcess("CharaStudio")]

    public class kks_osr2Link : BaseUnityPlugin
    {
        Rect windowRect = new Rect(((Screen.width / 2) - (350 / 2)), ((Screen.height / 2) - (470 / 2)), 350, 470);
        GameObject Myobj;
        String Tcode;
        SerialPort serial;
        bool syncCylinBtn = false;
        bool spacekey = false;
        bool autoMoveSW = false;
        bool inverseSW = false;
        bool toggleRNDRol = false;
        bool toggleRNDStrk = false;
        bool wkey = false;
        int selGridIntB = 0;
        //int selGridIntA = 0;
        string[] selStringsB = { "L/R", "F/B", "spinL", "spinR", "Twist", "Infinit", "Infinit2", "Omega" };
        string[] selStringsC = { "OSR2 CONNECT", "DISCONNECT" };
        //string[] selStringsA = { "Twist", "Infinit", "Infinit2", "Omega" };
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
        private ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Show { get; set; }
        private ConfigEntry<int>[] _Port { get; set; }

        public kks_osr2Link()
        {
            Show = Config.AddSetting("Hotkeys", "Show window", new BepInEx.Configuration.KeyboardShortcut(KeyCode.G));

        }

        void Start()

        {
            //Application.targetFrameRate = 15; //60FPSに設定
        }

        void Update()
        {

            if (syncCylinBtn)
            {
                if (GameObject.Find("p_koi_stu_cylinder01_02") != null)
                {
                    Myobj = GameObject.Find("p_koi_stu_cylinder01_02"); //シリンダーに接続
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

            syncCylinBtn = GUILayout.Toggle(syncCylinBtn, "Sync cylinder");
            if (syncCylinBtn)
            {
                GUILayout.BeginHorizontal();
                autoMoveSW = GUILayout.Toggle(autoMoveSW, "Auto Move");
                inverseSW = GUILayout.Toggle(inverseSW, "Inverse Z");

                GUILayout.EndHorizontal();

            }
            GUILayout.Label("[ Stroke section ]", winStyle);
            if (syncCylinBtn)
            {
                toggleRNDStrk = GUILayout.Toggle(toggleRNDStrk, "Random Stroke");
                GUILayout.Label("Speed");
                speedA = GUILayout.HorizontalSlider(speedA, 0, 15);
            }
            GUILayout.Label("Range");
            MyGUI.MinMaxSlider(ref minValue, ref maxValue, 0, 1000);
            GUILayout.Label("[ Swing section ]", winStyle);
            if (syncCylinBtn)
            {
                toggleRNDRol = GUILayout.Toggle(toggleRNDRol, "Random Swing");
                GUILayout.Label("Speed");
                speedB = GUILayout.HorizontalSlider(speedB, 0, 15);
            }
            GUILayout.Label("weight");
            rollweight = GUILayout.HorizontalSlider(rollweight, 0, 1);
            selGridIntB = GUILayout.SelectionGrid(selGridIntB, selStringsB, 4);
            //selGridIntA = GUILayout.SelectionGrid(selGridIntA, selStringsA, 5);

            stringToEdit = GUILayout.TextArea(stringToEdit, 300, txtStyle);

            GUI.Button(new Rect(0, 25, 345, 465), " ", winStyle);
            KKAPI.Utilities.IMGUIUtils.DragResizeEatWindow(6123, windowRect);

        }

        void CylinderMovefunc()
        {

            timeAR = timeAR + speedA * minmax * 0.01f;
            timeBR = timeBR + speedB * 0.01f;

            minmax = ((minValue + (1000 - maxValue)) / 300);
            minmax = Mathf.Clamp(minmax, 1, 9);
            nowValue = Wavegen("sin", timeAR, 1);//ストローク波

            if (toggleRNDStrk)
            {
                //nowValue = WaveRndgen(4, timeAR, 12);
                rr = Mathf.Sin(timeAR) / 12;
                tt = tt + rr + Mathf.Sin(Time.time * 2) * 0.01f;

                nowValue = Mathf.Sin(tt);
            }
            nowValueB = MathfConv.ChangeRange(-1, 1, nowValue, minValue, maxValue);
            nowValueC = nowValueB;

            sinwave = Wavegen("sin", timeBR, rollweight);
            coswave = Wavegen("cos", timeBR, rollweight);
            sinwaveB = MathfConv.ChangeRange(-1, 1, sinwave, -25, 25); //ロール波
            coswaveB = MathfConv.ChangeRange(-1, 1, coswave, -25, 25);
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
                    Myobj.transform.localEulerAngles = new Vector3(0, 0, sinwaveB);
                    rollXPC = rollXPC - 0.01f;
                    if (rollXPC < 0) { rollXPC = 0; }
                    rollZPC = sinwave;
                    break;
                case 1:
                    Myobj.transform.localEulerAngles = new Vector3(sinwaveB, 0, 0);
                    rollXPC = -sinwave;
                    rollZPC = rollZPC - 0.01f;
                    if (rollZPC < 0) { rollZPC = 0; }
                    break;
                case 2:
                    Myobj.transform.localEulerAngles = new Vector3(coswaveB, 0, sinwaveB);
                    rollXPC = sinwave;
                    rollZPC = coswave;
                    break;
                case 3:
                    Myobj.transform.localEulerAngles = new Vector3(sinwaveB, 0, coswaveB);
                    rollXPC = coswave;
                    rollZPC = sinwave;
                    break;
                case 4:
                    count = count + (speedB / 7) * 0.01f;
                    rollZPC = Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight;//角度
                    rollXPC = -(Mathf.Cos(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight);
                    Myobj.transform.localEulerAngles = new Vector3(-rollXPC * 25, 0, rollZPC * 25);
                    break;
                case 5:
                    count = count + (speedB / 7) * 0.01f;
                    rollZPC = -(Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight);//角度
                    rollXPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 2) * rollweight);
                    Myobj.transform.localEulerAngles = new Vector3(-rollXPC * 25, 0, rollZPC * 25);
                    break;
                case 6:
                    count = count + (speedB / 7) * 0.01f;
                    rollZPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f) * rollweight);//角度
                    rollXPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 2) * rollweight);
                    Myobj.transform.localEulerAngles = new Vector3(rollZPC * 25, 0, rollXPC * 25);
                    break;
                case 7:
                    count = count + (speedB / 7) * 0.01f;
                    rollZPC = -(Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 2) * rollweight);//角度
                    rollXPC = (Mathf.Sin(2 * Mathf.PI * Mathf.Sin(count) * 1f * 3) * rollweight);
                    Myobj.transform.localEulerAngles = new Vector3(-rollXPC * 25, 0, rollZPC * 25);
                    break;

            }


            Myobj.transform.localPosition = new Vector3(0, nowValueC / 8000, 0);// オブジェクトに動作流し込み
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

                // Do not proceed for layout event.
                if (Event.current.type == EventType.Layout)
                    return;

                // Clamp current state of values.
                minValue = Mathf.Clamp(minValue, minLimit, maxLimit);
                maxValue = Mathf.Max(minValue, Mathf.Clamp(maxValue, minLimit, maxLimit));

                // Calculate normalized version of values.
                float range = Mathf.Abs(maxLimit - minLimit);
                float normalizedMinValue = (minValue - minLimit) / range;
                float normalizedMaxValue = (maxValue - minLimit) / range;

                // Calculate visual version of values.
                float minValueX = position.x + normalizedMinValue * position.width;
                float maxValueX = position.x + normalizedMaxValue * position.width;

                Rect minThumbPosition = new Rect(minValueX - 5, position.y, 10, position.height);
                Rect maxThumbPosition = new Rect(maxValueX, position.y, 10, position.height);

                float normalizedMousePosition;

                switch (Event.current.GetTypeForControl(controlID))
                {
                    case EventType.MouseDown:
                        // Mouse pressed down on minimum thumb position?
                        if (minThumbPosition.Contains(Event.current.mousePosition))
                        {
                            GUIUtility.hotControl = minThumbControlID;
                            Event.current.Use();
                        }
                        // Mouse pressed down on maximum thumb position?
                        else if (maxThumbPosition.Contains(Event.current.mousePosition))
                        {
                            GUIUtility.hotControl = maxThumbControlID;
                            Event.current.Use();
                        }
                        break;

                    case EventType.MouseUp:
                        // Extremely important!!
                        if (GUIUtility.hotControl == minThumbControlID || GUIUtility.hotControl == maxThumbControlID || GUIUtility.hotControl == controlID)
                            GUIUtility.hotControl = 0;
                        break;

                    case EventType.MouseDrag:
                        normalizedMousePosition = (Event.current.mousePosition.x - position.x) / position.width;

                        // Process mouse movement?
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
                        // Draw background of slider control.
                        backgroundStyle.Draw(new Rect(position.x, position.y + 5, position.width, position.height - 10), GUIContent.none, controlID);
                        // Draw background of thumb range.
                        thumbStyle.Draw(new Rect(minValueX, position.y + 9, maxValueX - minValueX, 5), GUIContent.none, false, false, false, false);
                        // Draw minimum thumb button.
                        minThumbStyle.Draw(minThumbPosition, GUIContent.none, minThumbControlID);
                        // Draw maximum thumb button.
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
        private void OnDestroy()
        {
            serial.Close();
        }
    }

}