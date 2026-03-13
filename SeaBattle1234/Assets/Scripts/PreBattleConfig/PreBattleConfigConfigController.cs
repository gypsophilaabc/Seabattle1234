using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PreBattleConfigController : MonoBehaviour
{
    [Header("Map Config")]
    public TMP_InputField inputRows;
    public TMP_InputField inputCols;

    [Header("Top Info")]
    public TMP_Text textCurrentPlayer;

    [Header("Selected Fleet Display")]
    public TMP_Text textSelectedFleet;
    public TMP_Text textFirepowerHint;
    public TMP_Text textDensityHint;
    public TMP_Text textOverallHint;

    [Header("Buttons")]
    public Button btnConfirmCurrentPlayer;
    public Button btnClearCurrentPlayer;
    public Button btnStartGame;
    public Button btnStartDefault;

    [Header("Ship Pool Columns")]
    public Transform columnLeft;
    public Transform columnRight;

    private ConfigStage stage = ConfigStage.Player0Config;

    private FleetSetupData player0Fleet = new FleetSetupData();
    private FleetSetupData player1Fleet = new FleetSetupData();

    private void Start()
    {
        InitDefaultUI();
        BuildShipPoolUI();
        RefreshAllUI();
    }

    private void InitDefaultUI()
    {
        if (inputRows != null) inputRows.text = "16";
        if (inputCols != null) inputCols.text = "20";
    }

    public void AddShipToCurrentPlayer(int typeId)
    {
        FleetSetupData currentFleet = GetCurrentFleet();
        AddShip(currentFleet, typeId);
        RefreshAllUI();
    }

    public void RemoveShipFromCurrentPlayer(int typeId)
    {
        FleetSetupData currentFleet = GetCurrentFleet();
        RemoveShip(currentFleet, typeId);
        RefreshAllUI();
    }

    public void OnClickConfirmCurrentPlayer()
    {
        if (stage == ConfigStage.Player0Config)
        {
            stage = ConfigStage.Player1Config;
            RefreshAllUI();
            return;
        }

        if (stage == ConfigStage.Player1Config)
        {
            stage = ConfigStage.BothConfirmed;
            RefreshAllUI();
            return;
        }
    }

    public void OnClickClearCurrentPlayer()
    {
        FleetSetupData currentFleet = GetCurrentFleet();
        currentFleet.selectedShips.Clear();
        RefreshAllUI();
    }

    public void OnClickStartDefault()
    {
        GameSetupRuntime.UseDefault();
        SceneManager.LoadScene("Scene_Placement");
    }

    public void OnClickStartGame()
    {
        GameSetupData setup = BuildCurrentSetup();
        GameSetupRuntime.CurrentSetup = setup;
        SceneManager.LoadScene("Scene_Placement");
    }

    private GameSetupData BuildCurrentSetup()
    {
        GameSetupData setup = new GameSetupData();
        setup.boardRows = ParseInput(inputRows, 16);
        setup.boardCols = ParseInput(inputCols, 20);
        setup.player0Fleet = CloneFleet(player0Fleet);
        setup.player1Fleet = CloneFleet(player1Fleet);
        setup.useDefaultSetup = false;
        return setup;
    }

    private int ParseInput(TMP_InputField field, int fallback)
    {
        if (field == null) return fallback;

        if (int.TryParse(field.text, out int value))
        {
            return Mathf.Max(1, value);
        }

        return fallback;
    }

    private FleetSetupData GetCurrentFleet()
    {
        if (stage == ConfigStage.Player1Config)
            return player1Fleet;

        return player0Fleet;
    }

    private FleetSetupData CloneFleet(FleetSetupData source)
    {
        FleetSetupData cloned = new FleetSetupData();

        if (source == null || source.selectedShips == null)
            return cloned;

        foreach (var ship in source.selectedShips)
        {
            cloned.selectedShips.Add(new ShipPickData
            {
                typeId = ship.typeId,
                count = ship.count
            });
        }

        return cloned;
    }

    private void AddShip(FleetSetupData fleet, int typeId)
    {
        if (fleet == null) return;
        if (typeId < 0 || typeId >= ShipCatalog.Types.Count) return;

        ShipPickData existing = fleet.selectedShips.Find(x => x.typeId == typeId);
        if (existing != null)
        {
            existing.count++;
        }
        else
        {
            fleet.selectedShips.Add(new ShipPickData
            {
                typeId = typeId,
                count = 1
            });
        }
    }

    private void RemoveShip(FleetSetupData fleet, int typeId)
    {
        if (fleet == null) return;

        ShipPickData existing = fleet.selectedShips.Find(x => x.typeId == typeId);
        if (existing == null) return;

        existing.count--;
        if (existing.count <= 0)
        {
            fleet.selectedShips.Remove(existing);
        }
    }

    private void RefreshAllUI()
    {
        RefreshCurrentPlayerText();
        RefreshSelectedFleetText();
        RefreshBalanceHints();
        RefreshButtons();
    }

    private void BuildShipPoolUI()
    {
        if (columnLeft == null || columnRight == null)
        {
            Debug.LogWarning("Ship pool columns are not assigned.");
            return;
        }

        ClearChildren(columnLeft);
        ClearChildren(columnRight);

        int totalTypes = ShipCatalog.Types.Count;
        int splitIndex = (totalTypes + 1) / 2;

        for (int i = 0; i < totalTypes; i++)
        {
            Transform parentColumn = (i < splitIndex) ? columnLeft : columnRight;
            CreateShipRow(parentColumn, i);
        }
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void CreateShipRow(Transform parent, int typeId)
    {
        if (parent == null) return;
        if (typeId < 0 || typeId >= ShipCatalog.Types.Count) return;

        ShipType shipType = ShipCatalog.Types[typeId];

        GameObject row = CreateUIObject("Row_" + shipType.name, parent);

        VerticalLayoutGroup rowLayout = row.AddComponent<VerticalLayoutGroup>();
        rowLayout.padding = new RectOffset(4, 4, 4, 4);
        rowLayout.spacing = 4;
        rowLayout.childAlignment = TextAnchor.UpperCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        LayoutElement rowLayoutElement = row.AddComponent<LayoutElement>();
        rowLayoutElement.preferredHeight = 58;
        rowLayoutElement.minHeight = 58;

        // 第一行：按钮区
        GameObject topRow = CreateUIObject("TopRow", row.transform);
        HorizontalLayoutGroup topRowLayout = topRow.AddComponent<HorizontalLayoutGroup>();
        topRowLayout.padding = new RectOffset(0, 0, 0, 0);
        topRowLayout.spacing = 8;
        topRowLayout.childAlignment = TextAnchor.MiddleCenter;
        topRowLayout.childControlWidth = true;
        topRowLayout.childControlHeight = true;
        topRowLayout.childForceExpandWidth = false;
        topRowLayout.childForceExpandHeight = false;

        LayoutElement topRowElement = topRow.AddComponent<LayoutElement>();
        topRowElement.preferredHeight = 30;
        topRowElement.minHeight = 30;

        GameObject addButton = CreateButton(topRow.transform, shipType.name, 170, 28);
        Button addBtnComp = addButton.GetComponent<Button>();
        addBtnComp.onClick.AddListener(() => AddShipToCurrentPlayer(typeId));

        GameObject removeButton = CreateButton(topRow.transform, "Remove", 70, 28);
        Button removeBtnComp = removeButton.GetComponent<Button>();
        removeBtnComp.onClick.AddListener(() => RemoveShipFromCurrentPlayer(typeId));

        // 第二行：火力配置文本
        GameObject infoTextObj = CreateUIText(row.transform, ShipWeaponCatalog.GetLoadoutText(typeId), 13);
        LayoutElement infoLayout = infoTextObj.AddComponent<LayoutElement>();
        infoLayout.preferredHeight = 18;
        infoLayout.minHeight = 18;
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private void AddImage(GameObject go, Color color)
    {
        Image image = go.AddComponent<Image>();
        image.color = color;
    }

    private GameObject CreateButton(Transform parent, string buttonText, float preferredWidth, float preferredHeight)
    {
        GameObject buttonObj = new GameObject(buttonText + "_Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObj.transform.SetParent(parent, false);

        Image image = buttonObj.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.92f);

        LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = preferredWidth;
        layoutElement.preferredHeight = preferredHeight;
        layoutElement.minWidth = preferredWidth;
        layoutElement.minHeight = preferredHeight;

        GameObject textObj = new GameObject("Text (TMP)", typeof(RectTransform));
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TMPro.TextMeshProUGUI tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmpText.overflowMode = TextOverflowModes.Ellipsis;
        tmpText.text = buttonText;
        tmpText.fontSize = 18;
        tmpText.alignment = TMPro.TextAlignmentOptions.Center;
        tmpText.color = Color.black;
        tmpText.enableWordWrapping = true;

        return buttonObj;
    }

    private void RefreshCurrentPlayerText()
    {
        if (textCurrentPlayer == null) return;

        switch (stage)
        {
            case ConfigStage.Player0Config:
                textCurrentPlayer.text = "Current Player: Player0";
                break;

            case ConfigStage.Player1Config:
                textCurrentPlayer.text = "Current Player: Player1";
                break;

            case ConfigStage.BothConfirmed:
                textCurrentPlayer.text = "Both Players Confirmed";
                break;
        }
    }

    private void RefreshSelectedFleetText()
    {
        if (textSelectedFleet == null) return;

        FleetSetupData currentFleet = GetCurrentFleet();
        if (currentFleet == null || currentFleet.selectedShips == null || currentFleet.selectedShips.Count == 0)
        {
            textSelectedFleet.text = "No ships selected.";
            return;
        }

        List<string> lines = new List<string>();

        foreach (var ship in currentFleet.selectedShips)
        {
            if (ship.typeId < 0 || ship.typeId >= ShipCatalog.Types.Count)
                continue;

            string shipName = ShipCatalog.Types[ship.typeId].name;
            lines.Add(shipName + " x" + ship.count);
        }

        if (lines.Count == 0)
        {
            textSelectedFleet.text = "No ships selected.";
        }
        else
        {
            textSelectedFleet.text = string.Join("\n", lines);
        }
    }

    private void RefreshBalanceHints()
    {
        int rows = ParseInput(inputRows, 16);
        int cols = ParseInput(inputCols, 20);

        FleetSetupData currentFleet = GetCurrentFleet();

        float firepowerDensity = EstimateFirepowerDensity(currentFleet, rows, cols);
        float shipDensity = EstimateShipDensity(currentFleet, rows, cols);

        if (textFirepowerHint != null)
            textFirepowerHint.text = GetFirepowerHint(firepowerDensity);

        if (textDensityHint != null)
            textDensityHint.text = GetDensityHint(shipDensity);

        if (textOverallHint != null)
            textOverallHint.text = GetOverallHint(firepowerDensity, shipDensity);
    }

    private void RefreshButtons()
    {
        if (btnStartGame != null)
        {
            btnStartGame.interactable = (stage == ConfigStage.BothConfirmed);
        }
    }

    private float EstimateFirepowerDensity(FleetSetupData fleet, int rows, int cols)
    {
        if (fleet == null || fleet.selectedShips == null)
            return 0f;

        float totalFirepower = 0f;

        foreach (var ship in fleet.selectedShips)
        {
            totalFirepower += GetShipFirepower(ship.typeId) * ship.count;
        }

        float boardArea = Mathf.Max(1, rows * cols);
        return totalFirepower / boardArea;
    }

    private float EstimateShipDensity(FleetSetupData fleet, int rows, int cols)
    {
        if (fleet == null || fleet.selectedShips == null)
            return 0f;

        float totalCells = 0f;

        foreach (var ship in fleet.selectedShips)
        {
            totalCells += GetShipArea(ship.typeId) * ship.count;
        }

        float boardArea = Mathf.Max(1, rows * cols);
        return totalCells / boardArea;
    }

    private float GetShipFirepower(int typeId)
    {
        if (typeId < 0 || typeId >= ShipCatalog.Types.Count)
            return 1f;

        ShipType ship = ShipCatalog.Types[typeId];

        // 先用面积做简化估算，后面接真实武器配置再替换
        return ship.h * ship.w;
    }

    private int GetShipArea(int typeId)
    {
        if (typeId < 0 || typeId >= ShipCatalog.Types.Count)
            return 1;

        ShipType ship = ShipCatalog.Types[typeId];
        return ship.h * ship.w;
    }

    private string GetFirepowerHint(float value)
    {
        if (value < 0.12f) return "Firepower: Too weak";
        if (value < 0.14f) return "Firepower: Slightly weak but acceptable";
        if (value <= 0.17f) return "Firepower: Ideal";
        if (value <= 0.20f) return "Firepower: Slightly strong but acceptable";
        return "Firepower: Too strong";
    }

    private string GetDensityHint(float value)
    {
        if (value < 0.18f) return "Ship Density: Too sparse";
        if (value < 0.22f) return "Ship Density: Slightly sparse";
        if (value <= 0.30f) return "Ship Density: Ideal";
        if (value <= 0.35f) return "Ship Density: Slightly dense";
        return "Ship Density: Too dense";
    }

    private string GetOverallHint(float firepower, float density)
    {
        bool firepowerOk = firepower >= 0.12f && firepower <= 0.20f;
        bool densityOk = density >= 0.18f && density <= 0.35f;

        if (firepowerOk && densityOk)
            return "Overall: Valid setup";

        return "Overall: Warning - setup may be unbalanced";
    }
    private GameObject CreateUIText(Transform parent, string text, float fontSize)
    {
        GameObject textObj = new GameObject("InfoText", typeof(RectTransform));
        textObj.transform.SetParent(parent, false);

        TMPro.TextMeshProUGUI tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = TMPro.TextAlignmentOptions.Left;
        tmpText.color = Color.white;
        tmpText.enableWordWrapping = false;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return textObj;
    }
}