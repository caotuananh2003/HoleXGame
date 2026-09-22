#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool để sắp xếp obstacle theo các pattern hình học.
/// Mở qua menu: Tools → Obstacle Pattern Placer
/// </summary>
public class ObstaclePatternPlacer : EditorWindow
{
    // ── Enums ─────────────────────────────────────────────────────────────────

    private enum PatternType
    {
        FilledRectangle,
        HollowRectangle,
        FilledCircle,
        HollowCircle,
        FilledTriangle,
        HollowTriangle,
        Pyramid,
        Cube,
    }

    // ── Fields ────────────────────────────────────────────────────────────────

    // Prefab
    private GameObject _prefab;

    // Pattern
    private PatternType _patternType = PatternType.FilledRectangle;
    private float       _sideLength  = 5f;   // Triangle
    private float       _rectCols    = 5f;   // Rectangle — số object theo trục X
    private float       _rectRows    = 3f;   // Rectangle — số object theo trục Z
    private float       _radius      = 3f;   // Circle, Pyramid
    private float       _spacing     = 1f;   // khoảng cách tối thiểu
    private float       _objectHeight = 1f;  // Pyramid, Cube — chiều cao 1 object
    private float       _cubeCols    = 3f;   // Cube — số object theo X
    private float       _cubeRows    = 3f;   // Cube — số object theo Z
    private float       _cubeLayers  = 3f;   // Cube — số tầng theo Y

    // Vị trí trung tâm
    private Vector3 _center = Vector3.zero;

    // Parent transform (optional)
    private Transform _parent;

    // Preview
    private List<Vector3> _previewPoints = new();
    private bool          _showPreview   = true;

    // Scroll
    private Vector2 _scroll;

    // ── Menu ──────────────────────────────────────────────────────────────────

    [MenuItem("Tools/Obstacle Pattern Placer")]
    public static void Open()
    {
        var window = GetWindow<ObstaclePatternPlacer>("Obstacle Placer");
        window.minSize = new Vector2(300, 480);
    }

    // ── GUI ───────────────────────────────────────────────────────────────────

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        DrawPrefabSection();
        EditorGUILayout.Space(6);
        DrawCenterSection();
        EditorGUILayout.Space(6);
        DrawPatternSection();
        EditorGUILayout.Space(6);
        DrawSpacingSection();
        EditorGUILayout.Space(6);
        DrawParentSection();
        EditorGUILayout.Space(10);
        DrawPreviewToggle();
        EditorGUILayout.Space(10);
        DrawActionButtons();

        EditorGUILayout.EndScrollView();

        // Tự redraw khi di chuyển mouse trong Scene view (để preview cập nhật)
        if (_showPreview)
            Repaint();
    }

    // ── Sections ──────────────────────────────────────────────────────────────

    private void DrawPrefabSection()
    {
        EditorGUILayout.LabelField("Prefab", EditorStyles.boldLabel);
        _prefab = (GameObject)EditorGUILayout.ObjectField("Obstacle Prefab", _prefab, typeof(GameObject), false);
    }

    private void DrawCenterSection()
    {
        EditorGUILayout.LabelField("Vị trí trung tâm", EditorStyles.boldLabel);
        _center = EditorGUILayout.Vector3Field("Center (X, Y, Z)", _center);

        if (GUILayout.Button("Lấy từ Selection", GUILayout.Height(22)))
        {
            if (Selection.activeGameObject != null)
            {
                Vector3 p = Selection.activeGameObject.transform.position;
                _center = new Vector3(p.x, 0f, p.z);
            }
            else
            {
                Debug.LogWarning("[ObstaclePatternPlacer] Không có object nào được chọn.");
            }
        }
    }

    private void DrawPatternSection()
    {
        EditorGUILayout.LabelField("Pattern", EditorStyles.boldLabel);
        _patternType = (PatternType)EditorGUILayout.EnumPopup("Loại pattern", _patternType);

        switch (_patternType)
        {
            case PatternType.FilledRectangle:
            case PatternType.HollowRectangle:
                _rectCols = Mathf.Max(1f, EditorGUILayout.FloatField("Số object theo chiều ngang (X)", _rectCols));
                _rectRows = Mathf.Max(1f, EditorGUILayout.FloatField("Số object theo chiều dọc (Z)",   _rectRows));
                break;

            case PatternType.FilledCircle:
            case PatternType.HollowCircle:
                _radius = Mathf.Max(1f, EditorGUILayout.FloatField("Số vòng", _radius));
                break;

            case PatternType.FilledTriangle:
            case PatternType.HollowTriangle:
                _sideLength = Mathf.Max(1f, EditorGUILayout.FloatField("Số tầng", _sideLength));
                break;

            case PatternType.Pyramid:
                _radius       = Mathf.Max(1f,   EditorGUILayout.FloatField("Số vòng",           _radius));
                _objectHeight = Mathf.Max(0.01f, EditorGUILayout.FloatField("Chiều cao 1 object", _objectHeight));
                break;

            case PatternType.Cube:
                _cubeCols    = Mathf.Max(1f,    EditorGUILayout.FloatField("Số object theo X",       _cubeCols));
                _cubeRows    = Mathf.Max(1f,    EditorGUILayout.FloatField("Số object theo Z",       _cubeRows));
                _cubeLayers  = Mathf.Max(1f,    EditorGUILayout.FloatField("Số tầng theo Y",         _cubeLayers));
                _objectHeight = Mathf.Max(0.01f, EditorGUILayout.FloatField("Chiều cao 1 object (Y)", _objectHeight));
                break;
        }

        // Live count
        List<Vector3> pts = GeneratePoints();
        EditorGUILayout.HelpBox($"Số object sẽ spawn: {pts.Count}", MessageType.Info);
    }

    private void DrawSpacingSection()
    {
        EditorGUILayout.LabelField("Khoảng cách", EditorStyles.boldLabel);
        _spacing = Mathf.Max(0.1f, EditorGUILayout.FloatField("Khoảng cách tối thiểu", _spacing));
    }

    private void DrawParentSection()
    {
        EditorGUILayout.LabelField("Hierarchy", EditorStyles.boldLabel);
        _parent = (Transform)EditorGUILayout.ObjectField("Parent Transform", _parent, typeof(Transform), true);
    }

    private void DrawPreviewToggle()
    {
        _showPreview = EditorGUILayout.Toggle("Hiện preview (Scene)", _showPreview);
    }

    private void DrawActionButtons()
    {
        GUI.enabled = _prefab != null;
        if (GUILayout.Button("Spawn", GUILayout.Height(36)))
            SpawnPattern();
        GUI.enabled = true;

        if (GUILayout.Button("Xóa preview", GUILayout.Height(22)))
            _previewPoints.Clear();
    }

    // ── Scene View Preview ────────────────────────────────────────────────────

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!_showPreview) return;

        _previewPoints = GeneratePoints();

        Handles.color = new Color(0f, 1f, 0.5f, 0.8f);
        foreach (Vector3 p in _previewPoints)
        {
            Handles.DrawWireDisc(p, Vector3.up, _spacing * 0.45f);
            Handles.DrawLine(p + Vector3.left  * _spacing * 0.45f,
                             p + Vector3.right * _spacing * 0.45f);
            Handles.DrawLine(p + Vector3.forward * _spacing * 0.45f,
                             p + Vector3.back    * _spacing * 0.45f);
        }

        // Vẽ center
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(_center, Vector3.up, 0.2f);
    }

    // ── Spawn ─────────────────────────────────────────────────────────────────

    private void SpawnPattern()
    {
        List<Vector3> points = GeneratePoints();
        if (points.Count == 0) { Debug.LogWarning("[ObstaclePatternPlacer] Không có điểm nào được tạo."); return; }

        Undo.SetCurrentGroupName("Spawn Obstacle Pattern");
        int group = Undo.GetCurrentGroup();

        const RigidbodyConstraints freezeRot =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;

        foreach (Vector3 pos in points)
        {
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.identity;

            if (_parent != null)
                go.transform.SetParent(_parent, true);

            // Freeze rotation X Y Z trên tất cả Rigidbody trong object (kể cả child)
            foreach (Rigidbody rb in go.GetComponentsInChildren<Rigidbody>())
                rb.constraints |= freezeRot;

            Undo.RegisterCreatedObjectUndo(go, "Spawn Obstacle");
        }

        Undo.CollapseUndoOperations(group);
        Debug.Log($"[ObstaclePatternPlacer] Spawned {points.Count} objects ({_patternType}).");
    }

    // ── Point Generation ──────────────────────────────────────────────────────

    private List<Vector3> GeneratePoints()
    {
        return _patternType switch
        {
            PatternType.FilledRectangle => GenerateRectangle(hollow: false),
            PatternType.HollowRectangle => GenerateRectangle(hollow: true),
            PatternType.FilledCircle    => GenerateFilledCircle(),
            PatternType.HollowCircle    => GenerateHollowCircle(),
            PatternType.FilledTriangle  => GenerateTriangle(hollow: false),
            PatternType.HollowTriangle  => GenerateTriangle(hollow: true),
            PatternType.Pyramid         => GeneratePyramid(),
            PatternType.Cube            => GenerateCube(),
            _                           => new List<Vector3>()
        };
    }

    // Hình chữ nhật — filled hoặc hollow (chỉ viền)
    // cols = số object theo X, rows = số object theo Z
    private List<Vector3> GenerateRectangle(bool hollow)
    {
        var points = new List<Vector3>();

        int   cols  = Mathf.Max(1, Mathf.RoundToInt(_rectCols));
        int   rows  = Mathf.Max(1, Mathf.RoundToInt(_rectRows));
        float halfX = (cols - 1) * _spacing / 2f;
        float halfZ = (rows - 1) * _spacing / 2f;

        for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
        {
            if (hollow)
            {
                // 1x1 hoặc 1xN: không có interior → spawn tất cả
                bool onEdge = row == 0 || row == rows - 1
                           || col == 0 || col == cols - 1;
                if (!onEdge) continue;
            }

            float x = _center.x - halfX + col * _spacing;
            float z = _center.z - halfZ + row * _spacing;
            points.Add(new Vector3(x, 0f, z));
        }

        return points;
    }

    // Hình tròn lấp đầy — _radius = số vòng.
    // Ring 0 = 1 object tại tâm. Ring r = 6*r objects trên vòng bán kính r*_spacing.
    private List<Vector3> GenerateFilledCircle()
    {
        var points = new List<Vector3>();
        int rings  = Mathf.Max(1, Mathf.RoundToInt(_radius));

        for (int r = 0; r < rings; r++)
            AddRing(points, r);

        return points;
    }

    // Hình tròn rỗng — chỉ spawn vòng ngoài cùng.
    private List<Vector3> GenerateHollowCircle()
    {
        var points = new List<Vector3>();
        int rings  = Mathf.Max(1, Mathf.RoundToInt(_radius));

        AddRing(points, rings - 1);

        return points;
    }

    /// <summary>
    /// Thêm một ring vào danh sách điểm.
    /// Ring 0: 1 object tại tâm.
    /// Ring r > 0: 6*r objects phân bố đều trên vòng tròn bán kính r*_spacing.
    /// </summary>
    private void AddRing(List<Vector3> points, int r)
    {
        if (r == 0)
        {
            points.Add(new Vector3(_center.x, 0f, _center.z));
            return;
        }

        int   count  = 6 * r;
        float radius = r * _spacing;

        for (int i = 0; i < count; i++)
        {
            float angle = 2f * Mathf.PI * i / count;
            float x     = _center.x + radius * Mathf.Cos(angle);
            float z     = _center.z + radius * Mathf.Sin(angle);
            points.Add(new Vector3(x, 0f, z));
        }
    }

    // Hình tam giác — filled hoặc hollow (chỉ 3 cạnh viền)
    // Đỉnh nhọn hướng lên (Z+), đáy hướng xuống (Z-)
    // _sideLength = số tầng. Tầng 1 (đỉnh) = 1 object, tầng N (đáy) = N objects.
    private List<Vector3> GenerateTriangle(bool hollow)
    {
        var points = new List<Vector3>();

        int   rows        = Mathf.Max(1, Mathf.RoundToInt(_sideLength));
        float rowStep     = _spacing * Mathf.Sqrt(3f) / 2f;
        float totalHeight = (rows - 1) * rowStep;

        for (int row = 0; row < rows; row++)
        {
            int   colCount = row + 1;
            float rowWidth = (colCount - 1) * _spacing;
            float z        = _center.z + totalHeight / 2f - row * rowStep;

            for (int col = 0; col < colCount; col++)
            {
                if (hollow && rows > 2)
                {
                    // Viền tam giác gồm:
                    //   - Hàng đỉnh (row 0): object duy nhất
                    //   - Hàng đáy (row cuối): toàn bộ object
                    //   - Cạnh trái: col == 0
                    //   - Cạnh phải: col == row (object cuối của mỗi hàng)
                    bool onEdge = row == 0
                               || row == rows - 1
                               || col == 0
                               || col == row;
                    if (!onEdge) continue;
                }

                float x = _center.x - rowWidth / 2f + col * _spacing;
                points.Add(new Vector3(x, 0f, z));
            }
        }

        return points;
    }
    // Kim tự tháp — đáy hình tròn (dùng ring system như FilledCircle).
    // _radius = số vòng. Ring ngoài cùng cao 1 tầng, vào trong thêm 1.
    // Ring tâm (r=0) cao nhất = rings tầng. Objects xếp chồng sát nhau theo Y.
    private List<Vector3> GeneratePyramid()
    {
        var points = new List<Vector3>();
        int rings  = Mathf.Max(1, Mathf.RoundToInt(_radius));

        for (int r = 0; r < rings; r++)
        {
            // Chiều cao của ring r: tâm (r=0) cao nhất, ngoài cùng (r=rings-1) thấp nhất
            int height = rings - r;

            // Lấy các vị trí XZ của ring r (tái dụng AddRing nhưng không add thẳng)
            List<Vector3> ringPositions = new List<Vector3>();
            AddRing(ringPositions, r);

            // Với mỗi vị trí XZ, stack đủ số object theo trục Y
            foreach (Vector3 basePos in ringPositions)
            {
                for (int h = 0; h < height; h++)
                {
                    float y = _center.y + h * _objectHeight;
                    points.Add(new Vector3(basePos.x, y, basePos.z));
                }
            }
        }

        return points;
    }
    // Hình khối vuông 3D — xếp các object thành lưới cols × rows × layers
    // _cubeCols = số object theo X, _cubeRows = số object theo Z, _cubeLayers = số tầng theo Y
    // _spacing = khoảng cách giữa các object theo X và Z
    // _objectHeight = khoảng cách giữa các tầng theo Y
    private List<Vector3> GenerateCube()
    {
        var points = new List<Vector3>();

        int cols   = Mathf.Max(1, Mathf.RoundToInt(_cubeCols));
        int rows   = Mathf.Max(1, Mathf.RoundToInt(_cubeRows));
        int layers = Mathf.Max(1, Mathf.RoundToInt(_cubeLayers));

        float halfX = (cols   - 1) * _spacing     / 2f;
        float halfZ = (rows   - 1) * _spacing     / 2f;
        float baseY = _center.y;

        for (int layer = 0; layer < layers; layer++)
        for (int row   = 0; row   < rows;   row++)
        for (int col   = 0; col   < cols;   col++)
        {
            float x = _center.x - halfX + col   * _spacing;
            float y = baseY             + layer  * _objectHeight;
            float z = _center.z - halfZ + row    * _spacing;
            points.Add(new Vector3(x, y, z));
        }

        return points;
    }
}
#endif
