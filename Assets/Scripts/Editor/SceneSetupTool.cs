#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

public static class SceneSetupTool
{
    [MenuItem("Tools/Build Slot Game Scene")]
    public static string BuildScene()
    {
        string dir = "Assets/Game/Slot Machine/Slot Machine/";

        // 1. Load Sprites
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "bg_gradient.png");
        Sprite machineFront = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "slot-machine4.png");
        Sprite machineBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "slot-machine5.png");
        Sprite leverUp = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "slot-machine2.png");
        Sprite leverDown = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "slot-machine3.png");
        Sprite middleBox = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "slot_machine_Middle_box.png");
        Sprite popupSprite = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "popup.png");

        Sprite symSeven = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "slot-symbol1.png");
        Sprite symCherry = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "slot-symbol2.png");
        Sprite symBell = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "slot-symbol3.png");
        Sprite symBar = AssetDatabase.LoadAssetAtPath<Sprite>(dir + "slot-symbol4.png");

        // Find Canvas in scene
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return "Canvas not found!";

        // Remove old UI children if any
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
        }

        // 1. Background
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(canvas.transform, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.sprite = bgSprite;
        bgImg.raycastTarget = false;

        // 2. SlotMachineRoot
        GameObject machineRoot = new GameObject("SlotMachineRoot", typeof(RectTransform));
        machineRoot.transform.SetParent(canvas.transform, false);
        RectTransform machineRect = machineRoot.GetComponent<RectTransform>();
        machineRect.anchorMin = new Vector2(0.5f, 0.5f);
        machineRect.anchorMax = new Vector2(0.5f, 0.5f);
        machineRect.pivot = new Vector2(0.5f, 0.5f);
        machineRect.anchoredPosition = new Vector2(0f, 95f);
        machineRect.sizeDelta = new Vector2(816f, 624f);
        machineRect.localScale = new Vector3(1.25f, 1.25f, 1f);

        // 2a. Reel Backdrop
        GameObject backdropObj = new GameObject("ReelBackdrop", typeof(RectTransform), typeof(Image));
        backdropObj.transform.SetParent(machineRoot.transform, false);
        RectTransform bdRect = backdropObj.GetComponent<RectTransform>();
        bdRect.sizeDelta = new Vector2(816f, 624f);
        Image bdImg = backdropObj.GetComponent<Image>();
        bdImg.sprite = machineBackdrop;
        bdImg.raycastTarget = false;

        // 2b. Reels Area
        GameObject reelsArea = new GameObject("ReelsArea", typeof(RectTransform));
        reelsArea.transform.SetParent(machineRoot.transform, false);
        RectTransform raRect = reelsArea.GetComponent<RectTransform>();
        raRect.sizeDelta = new Vector2(816f, 624f);

        // 3 Reels setup
        float[] reelX = new float[] { -125.5f, 4.5f, 134.5f };
        float reelY = -38.5f;
        ReelController[] reelControllers = new ReelController[3];

        for (int r = 0; r < 3; r++)
        {
            GameObject reelObj = new GameObject("Reel_" + r, typeof(RectTransform), typeof(RectMask2D), typeof(ReelController));
            reelObj.transform.SetParent(reelsArea.transform, false);
            RectTransform rRect = reelObj.GetComponent<RectTransform>();
            rRect.sizeDelta = new Vector2(108f, 210f);
            rRect.anchoredPosition = new Vector2(reelX[r], reelY);

            ReelController rc = reelObj.GetComponent<ReelController>();
            reelControllers[r] = rc;

            // 5 Symbol Images
            var imgList = new List<Image>();
            float startY = -2f * 96f; // -192
            for (int s = 0; s < 5; s++)
            {
                GameObject symObj = new GameObject("Symbol_" + s, typeof(RectTransform), typeof(Image));
                symObj.transform.SetParent(reelObj.transform, false);
                RectTransform sRect = symObj.GetComponent<RectTransform>();
                sRect.sizeDelta = new Vector2(96f, 96f);
                sRect.anchoredPosition = new Vector2(0f, startY + (s * 96f));
                Image sImg = symObj.GetComponent<Image>();
                sImg.sprite = symSeven;
                sImg.raycastTarget = false;
                imgList.Add(sImg);
            }

            // Set serialized symbolImages on ReelController via SerializedObject
            SerializedObject so = new SerializedObject(rc);
            SerializedProperty imagesProp = so.FindProperty("symbolImages");
            imagesProp.ClearArray();
            imagesProp.arraySize = imgList.Count;
            for (int i = 0; i < imgList.Count; i++)
            {
                imagesProp.GetArrayElementAtIndex(i).objectReferenceValue = imgList[i];
            }
            so.ApplyModifiedProperties();
        }

        // 2c. Cabinet Front
        GameObject cabinetObj = new GameObject("FrontCabinet", typeof(RectTransform), typeof(Image));
        cabinetObj.transform.SetParent(machineRoot.transform, false);
        RectTransform cabRect = cabinetObj.GetComponent<RectTransform>();
        cabRect.sizeDelta = new Vector2(816f, 624f);
        Image cabImg = cabinetObj.GetComponent<Image>();
        cabImg.sprite = machineFront;
        cabImg.raycastTarget = false;

        // 2d. Lever Image
        GameObject leverObj = new GameObject("Lever", typeof(RectTransform), typeof(Image));
        leverObj.transform.SetParent(machineRoot.transform, false);
        RectTransform leverRect = leverObj.GetComponent<RectTransform>();
        leverRect.sizeDelta = new Vector2(816f, 624f);
        Image leverImg = leverObj.GetComponent<Image>();
        leverImg.sprite = leverUp;
        leverImg.raycastTarget = false; // Do not block entire machine with transparent rect

        // Lever Clickable Handle (only covers the actual pull handle on the right)
        GameObject leverHandleObj = new GameObject("LeverHandle", typeof(RectTransform), typeof(Image), typeof(Button));
        leverHandleObj.transform.SetParent(leverObj.transform, false);
        RectTransform lhRect = leverHandleObj.GetComponent<RectTransform>();
        lhRect.sizeDelta = new Vector2(100f, 260f);
        lhRect.anchoredPosition = new Vector2(345f, 50f);
        Image lhImg = leverHandleObj.GetComponent<Image>();
        lhImg.color = Color.clear; // Invisible hit area over knob
        lhImg.raycastTarget = true;
        Button leverBtn = leverHandleObj.GetComponent<Button>();

        // 3. Status & Win Display Frame (in lower cabinet panel)
        GameObject displayBoxObj = new GameObject("DisplayFrame", typeof(RectTransform), typeof(Image));
        displayBoxObj.transform.SetParent(machineRoot.transform, false);
        RectTransform dbRect = displayBoxObj.GetComponent<RectTransform>();
        dbRect.anchoredPosition = new Vector2(6f, -225f);
        dbRect.sizeDelta = new Vector2(490f, 95f);
        Image dbImg = displayBoxObj.GetComponent<Image>();
        dbImg.sprite = middleBox;
        dbImg.raycastTarget = false;

        // Status Text inside frame
        GameObject statusTextObj = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
        statusTextObj.transform.SetParent(displayBoxObj.transform, false);
        RectTransform stRect = statusTextObj.GetComponent<RectTransform>();
        stRect.sizeDelta = new Vector2(460f, 38f);
        stRect.anchoredPosition = new Vector2(0f, 14f);
        TextMeshProUGUI statusTmp = statusTextObj.GetComponent<TextMeshProUGUI>();
        statusTmp.text = "PRESS SPIN TO PLAY";
        statusTmp.fontSize = 24;
        statusTmp.fontStyle = FontStyles.Bold;
        statusTmp.alignment = TextAlignmentOptions.Center;
        statusTmp.color = new Color(1f, 0.95f, 0.5f);

        // Win Amount Text inside frame
        GameObject winTextObj = new GameObject("WinAmountText", typeof(RectTransform), typeof(TextMeshProUGUI));
        winTextObj.transform.SetParent(displayBoxObj.transform, false);
        RectTransform wtRect = winTextObj.GetComponent<RectTransform>();
        wtRect.sizeDelta = new Vector2(460f, 32f);
        wtRect.anchoredPosition = new Vector2(0f, -18f);
        TextMeshProUGUI winTmp = winTextObj.GetComponent<TextMeshProUGUI>();
        winTmp.text = "WIN: 0";
        winTmp.fontSize = 20;
        winTmp.fontStyle = FontStyles.Bold;
        winTmp.alignment = TextAlignmentOptions.Center;
        winTmp.color = Color.white;

        // 4. UI HUD Controls Panel (below the machine)
        GameObject hudObj = new GameObject("UI_HUD", typeof(RectTransform));
        hudObj.transform.SetParent(canvas.transform, false);
        RectTransform hudRect = hudObj.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0.5f, 0f);
        hudRect.anchorMax = new Vector2(0.5f, 0f);
        hudRect.pivot = new Vector2(0.5f, 0f);
        hudRect.anchoredPosition = new Vector2(0f, 30f);
        hudRect.sizeDelta = new Vector2(1300f, 130f);

        // 4a. Wallet Balance Text
        GameObject balTextObj = new GameObject("BalanceText", typeof(RectTransform), typeof(TextMeshProUGUI));
        balTextObj.transform.SetParent(hudObj.transform, false);
        RectTransform btRect = balTextObj.GetComponent<RectTransform>();
        btRect.anchoredPosition = new Vector2(-420f, 75f);
        btRect.sizeDelta = new Vector2(320f, 45f);
        TextMeshProUGUI balTmp = balTextObj.GetComponent<TextMeshProUGUI>();
        balTmp.text = "BALANCE: 1000";
        balTmp.fontSize = 32;
        balTmp.fontStyle = FontStyles.Bold;
        balTmp.color = Color.white;

        // 4b. Bet Amount Text
        GameObject betTextObj = new GameObject("BetText", typeof(RectTransform), typeof(TextMeshProUGUI));
        betTextObj.transform.SetParent(hudObj.transform, false);
        RectTransform bttRect = betTextObj.GetComponent<RectTransform>();
        bttRect.anchoredPosition = new Vector2(-420f, 25f);
        bttRect.sizeDelta = new Vector2(320f, 45f);
        TextMeshProUGUI betTmp = betTextObj.GetComponent<TextMeshProUGUI>();
        betTmp.text = "BET: 10";
        betTmp.fontSize = 32;
        betTmp.fontStyle = FontStyles.Bold;
        betTmp.color = new Color(1f, 0.85f, 0.2f);

        // 4c. Bet Buttons (10G, 50G, 100G)
        int[] betValues = new int[] { 10, 50, 100 };
        Button[] betButtons = new Button[3];
        for (int b = 0; b < 3; b++)
        {
            GameObject bObj = new GameObject("BetBtn_" + betValues[b], typeof(RectTransform), typeof(Image), typeof(Button));
            bObj.transform.SetParent(hudObj.transform, false);
            RectTransform bRect = bObj.GetComponent<RectTransform>();
            bRect.sizeDelta = new Vector2(110f, 55f);
            bRect.anchoredPosition = new Vector2(-110f + (b * 130f), 50f);
            Image bImg = bObj.GetComponent<Image>();
            bImg.sprite = middleBox;
            Button btnComp = bObj.GetComponent<Button>();
            btnComp.transition = Selectable.Transition.ColorTint;
            var cb = btnComp.colors;
            cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btnComp.colors = cb;
            betButtons[b] = btnComp;

            // Button label
            GameObject lblObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(bObj.transform, false);
            RectTransform lblRect = lblObj.GetComponent<RectTransform>();
            lblRect.sizeDelta = bRect.sizeDelta;
            TextMeshProUGUI lblTmp = lblObj.GetComponent<TextMeshProUGUI>();
            lblTmp.text = betValues[b] + "G";
            lblTmp.fontSize = 24;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.alignment = TextAlignmentOptions.Center;
            lblTmp.color = new Color(1f, 0.9f, 0.4f);
        }

        // 4d. SPIN Button
        GameObject spinBtnObj = new GameObject("SpinButton", typeof(RectTransform), typeof(Image), typeof(Button));
        spinBtnObj.transform.SetParent(hudObj.transform, false);
        RectTransform spinRect = spinBtnObj.GetComponent<RectTransform>();
        spinRect.sizeDelta = new Vector2(230f, 75f);
        spinRect.anchoredPosition = new Vector2(430f, 50f);
        Image spinImg = spinBtnObj.GetComponent<Image>();
        spinImg.sprite = middleBox;
        Button spinBtn = spinBtnObj.GetComponent<Button>();
        spinBtn.transition = Selectable.Transition.ColorTint;
        var scb = spinBtn.colors;
        scb.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        scb.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        scb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        spinBtn.colors = scb;

        GameObject spinLblObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        spinLblObj.transform.SetParent(spinBtnObj.transform, false);
        RectTransform slRect = spinLblObj.GetComponent<RectTransform>();
        slRect.sizeDelta = spinRect.sizeDelta;
        TextMeshProUGUI spinTmp = spinLblObj.GetComponent<TextMeshProUGUI>();
        spinTmp.text = "SPIN";
        spinTmp.fontSize = 42;
        spinTmp.fontStyle = FontStyles.Bold;
        spinTmp.alignment = TextAlignmentOptions.Center;
        spinTmp.color = new Color(1f, 0.92f, 0.3f); // Bright gold text

        // 5. Win Popup Modal
        GameObject popupObj = new GameObject("WinPopup", typeof(RectTransform), typeof(Image));
        popupObj.transform.SetParent(canvas.transform, false);
        RectTransform popRect = popupObj.GetComponent<RectTransform>();
        popRect.sizeDelta = new Vector2(850f, 440f);
        popRect.anchoredPosition = Vector2.zero;
        Image popImg = popupObj.GetComponent<Image>();
        popImg.sprite = popupSprite;

        GameObject popMsgObj = new GameObject("MessageText", typeof(RectTransform), typeof(TextMeshProUGUI));
        popMsgObj.transform.SetParent(popupObj.transform, false);
        RectTransform pmRect = popMsgObj.GetComponent<RectTransform>();
        pmRect.sizeDelta = new Vector2(750f, 220f);
        pmRect.anchoredPosition = new Vector2(0f, 40f);
        TextMeshProUGUI popTmp = popMsgObj.GetComponent<TextMeshProUGUI>();
        popTmp.text = "CONGRATULATIONS!\n\nYOU WON!";
        popTmp.fontSize = 32;
        popTmp.fontStyle = FontStyles.Bold;
        popTmp.alignment = TextAlignmentOptions.Center;
        popTmp.color = new Color(1f, 0.9f, 0.4f);

        // Popup Collect Button
        GameObject collectBtnObj = new GameObject("CollectButton", typeof(RectTransform), typeof(Image), typeof(Button));
        collectBtnObj.transform.SetParent(popupObj.transform, false);
        RectTransform cRect = collectBtnObj.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(200f, 70f);
        cRect.anchoredPosition = new Vector2(0f, -120f);
        Image cImg = collectBtnObj.GetComponent<Image>();
        cImg.sprite = middleBox;
        Button collectBtn = collectBtnObj.GetComponent<Button>();

        GameObject cLblObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        cLblObj.transform.SetParent(collectBtnObj.transform, false);
        RectTransform clRect = cLblObj.GetComponent<RectTransform>();
        clRect.sizeDelta = cRect.sizeDelta;
        TextMeshProUGUI cTmp = cLblObj.GetComponent<TextMeshProUGUI>();
        cTmp.text = "COLLECT";
        cTmp.fontSize = 26;
        cTmp.fontStyle = FontStyles.Bold;
        cTmp.alignment = TextAlignmentOptions.Center;
        cTmp.color = new Color(1f, 0.9f, 0.4f);

        popupObj.SetActive(false);

        // 6. Setup Game Controllers
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
        }
        GameManager gm = gmObj.GetComponent<GameManager>() ?? gmObj.AddComponent<GameManager>();
        WalletManager wm = gmObj.GetComponent<WalletManager>() ?? gmObj.AddComponent<WalletManager>();
        WinEvaluator we = gmObj.GetComponent<WinEvaluator>() ?? gmObj.AddComponent<WinEvaluator>();
        SlotMachineController smc = gmObj.GetComponent<SlotMachineController>() ?? gmObj.AddComponent<SlotMachineController>();
        AudioManager am = gmObj.GetComponent<AudioManager>() ?? gmObj.AddComponent<AudioManager>();

        // Wire AudioManager
        AudioSource[] audioSources = gmObj.GetComponents<AudioSource>();
        AudioSource sfxSource = audioSources.Length > 0 ? audioSources[0] : gmObj.AddComponent<AudioSource>();
        AudioSource loopSource = audioSources.Length > 1 ? audioSources[1] : gmObj.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        loopSource.playOnAwake = false;
        loopSource.spatialBlend = 0f;
        loopSource.loop = true;

        string audioDir = "Assets/Game/Audio/RawSignalCasinoLite/WAV/";
        SerializedObject amSO = new SerializedObject(am);
        amSO.FindProperty("spinButtonClickSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(audioDir + "slot_spin_button_01.wav");
        amSO.FindProperty("leverPullSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(audioDir + "slot_lever_pull_01.wav");
        amSO.FindProperty("reelSpinLoopSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(audioDir + "dice_roll_table_01.wav");
        amSO.FindProperty("reelStopSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(audioDir + "slot_reel_stop_01.wav");
        amSO.FindProperty("winSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(audioDir + "slot_win_small_01.wav");
        amSO.FindProperty("jackpotSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(audioDir + "slot_win_big_01.wav");
        amSO.FindProperty("betChangeSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(audioDir + "ui_bet_up_01.wav");
        amSO.FindProperty("insufficientBalanceSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(audioDir + "chip_single_place_01.wav");
        amSO.FindProperty("sfxSource").objectReferenceValue = sfxSource;
        amSO.FindProperty("loopSource").objectReferenceValue = loopSource;
        amSO.FindProperty("sfxVolume").floatValue = 1.0f;
        amSO.ApplyModifiedProperties();

        // Wire SlotMachineController
        SerializedObject smcSO = new SerializedObject(smc);
        SerializedProperty reelsProp = smcSO.FindProperty("reels");
        reelsProp.ClearArray();
        reelsProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            reelsProp.GetArrayElementAtIndex(i).objectReferenceValue = reelControllers[i];
        }

        // Available Symbols (Seven, Cherry, Bell, BAR)
        SerializedProperty symsProp = smcSO.FindProperty("availableSymbols");
        symsProp.ClearArray();
        symsProp.arraySize = 4;

        string[] sNames = new string[] { "Seven", "Cherry", "Bell", "BAR" };
        Sprite[] sSprites = new Sprite[] { symSeven, symCherry, symBell, symBar };
        int[] sMultipliers = new int[] { 100, 10, 25, 50 };

        for (int i = 0; i < 4; i++)
        {
            var elem = symsProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("symbolName").stringValue = sNames[i];
            elem.FindPropertyRelative("symbolSprite").objectReferenceValue = sSprites[i];
            elem.FindPropertyRelative("payoutMultiplier").intValue = sMultipliers[i];
        }

        smcSO.FindProperty("winEvaluator").objectReferenceValue = we;
        smcSO.FindProperty("walletManager").objectReferenceValue = wm;
        smcSO.FindProperty("spinButton").objectReferenceValue = spinBtn;
        smcSO.FindProperty("statusText").objectReferenceValue = statusTmp;
        smcSO.FindProperty("winAmountText").objectReferenceValue = winTmp;
        smcSO.FindProperty("balanceText").objectReferenceValue = balTmp;
        smcSO.FindProperty("betText").objectReferenceValue = betTmp;
        smcSO.FindProperty("leverImage").objectReferenceValue = leverImg;
        smcSO.FindProperty("leverUpSprite").objectReferenceValue = leverUp;
        smcSO.FindProperty("leverDownSprite").objectReferenceValue = leverDown;
        smcSO.FindProperty("winPopupObject").objectReferenceValue = popupObj;
        smcSO.FindProperty("winPopupText").objectReferenceValue = popTmp;
        smcSO.FindProperty("debugForceWin").boolValue = false;
        smcSO.ApplyModifiedProperties();

        // Wire GameManager
        SerializedObject gmSO = new SerializedObject(gm);
        gmSO.FindProperty("slotMachineController").objectReferenceValue = smc;
        gmSO.FindProperty("walletManager").objectReferenceValue = wm;
        gmSO.ApplyModifiedProperties();

        // Hook up button onClick events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(spinBtn.onClick, smc.OnSpinButtonPressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(leverBtn.onClick, smc.OnSpinButtonPressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(collectBtn.onClick, smc.CloseWinPopup);

        // Bet buttons persistent listeners
        for (int b = 0; b < 3; b++)
        {
            int betVal = betValues[b];
            var targetMethod = typeof(SlotMachineController).GetMethod("SelectBet", new System.Type[] { typeof(int) });
            var action = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction<int>), smc, targetMethod) as UnityEngine.Events.UnityAction<int>;
            UnityEditor.Events.UnityEventTools.AddIntPersistentListener(betButtons[b].onClick, action, betVal);
        }

        // Save scene
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorSceneManager.SaveScene(canvas.gameObject.scene);

        return "Slot Machine UI rebuilt and scene saved successfully!";
    }
}
#endif
