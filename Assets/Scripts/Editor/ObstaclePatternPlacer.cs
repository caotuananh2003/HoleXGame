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
        Square,
        Rectangle,
        FilledCircle,
        HollowCircle,
        Triangle,
        Pyramid,
    }

    // ── Fields ────────────────────────────────────────────────────────────────

    // Prefab
    private GameObject _prefab;

    // Pattern
    private PatternType _patternType = PatternType.Square;
    private float       _sideLength  = 5f;   // Square, Triangle
    private float       _rectCols    = 5f;   // Rectangle — số object theo trục X
    private float       _rectRows    = 3f;   // Rectangle — số object theo trục Z
    private float       _radius      = 3f;   // Circle, Pyramid
    private float       _spacing     = 1f;   // khoảng cách tối thiểu
    private float       _objectHeight = 1f;  // Pyramid — chiều cao 1 object (để xếp sát nhau)

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
            case PatternType.Square:
                _sideLength = Mathf.Max(1f, EditorGUILayout.FloatField("Số object mỗi cạnh", _sideLength));
                break;

            case PatternType.Rectangle:
                _rectCols = Mathf.Max(1f, EditorGUILayout.FloatField("Số object theo chiều ngang (X)", _rectCols));
                _rectRows = Mathf.Max(1f, EditorGUILayout.FloatField("Số object theo chiều dọc (Z)",   _rectRows));
                break;

            case PatternType.FilledCircle:
                _radius = Mathf.Max(1f, EditorGUILayout.FloatField("Số vòng", _radius));
                break;

            case PatternType.HollowCircle:
                _radius = Mathf.Max(1f, EditorGUILayout.FloatField("Số vòng", _radius));
                break;

            case PatternType.Triangle:
                _sideLength = Mathf.Max(1f, EditorGUILayout.FloatField("Số tầng", _sideLength));
                break;

            case PatternType.Pyramid:
                _radius       = Mathf.Max(1f,   EditorGUILayout.FloatField("Số vòng",           _radius));
                _objectHeight = Mathf.Max(0.01f, EditorGUILayout.FloatField("Chiều cao 1 object", _objectHeight));
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

        foreach (Vector3 pos in points)
        {
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.identity;

            if (_parent != null)
                go.transform.SetParent(_parent, true);

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
            PatternType.Square       => GenerateSquare(),
            PatternType.Rectangle    => GenerateRectangle(),
            PatternType.FilledCircle => GenerateFilledCircle(),
            PatternType.HollowCircle => GenerateHollowCircle(),
            PatternType.Triangle     => GenerateTriangle(),
            PatternType.Pyramid      => GeneratePyramid(),
            _                        => new List<Vector3>()
        };
    }

    // Hình vuông — _sideLength = số object trên mỗi cạnh, cách nhau _spacing
    private List<Vector3> GenerateSquare()
    {
        var points = new List<Vector3>();

        int   count     = Mathf.Max(1, Mathf.RoundToInt(_sideLength));
        float totalSize = (count - 1) * _spacing;
        float half      = totalSize / 2f;

        for (int row = 0; row < count; row++)
        for (int col = 0; col < count; col++)
        {
            float x = _center.x - half + col * _spacing;
            float z = _center.z - half + row * _spacing;
            points.Add(new Vector3(x, 0f, z));
        }

        return points;
    }

    // Hình chữ nhật — _rectCols = số object theo X, _rectRows = số object theo Z
    private List<Vector3> GenerateRectangle()
    {
        var points = new List<Vector3>();

        int   cols      = Mathf.Max(1, Mathf.RoundToInt(_rectCols));
        int   rows      = Mathf.Max(1, Mathf.RoundToInt(_rectRows));
        float halfX     = (cols - 1) * _spacing / 2f;
        float halfZ     = (rows - 1) * _spacing / 2f;

        for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
        {
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

    // Hình tam giác đều xuôi — đỉnh nhọn hướng lên (Z+), đáy hướng xuống (Z-)
    // _sideLength = số tầng. Tầng 1 (đỉnh) = 1 object, tầng N (đáy) = N objects.
    // Khoảng cách ngang giữa object: _spacing. Khoảng cách dọc giữa hàng: _spacing * sin(60°).
    private List<Vector3> GenerateTriangle()
    {
        var points = new List<Vector3>();

        int   rows     = Mathf.Max(1, Mathf.RoundToInt(_sideLength));
        float rowStep  = _spacing * Mathf.Sqrt(3f) / 2f; // khoảng cách dọc giữa 2 hàng

        // Tổng chiều cao của tam giác
        float totalHeight = (rows - 1) * rowStep;

        for (int row = 0; row < rows; row++)
        {
            int   colCount  = row + 1;                          // hàng 0 = 1 object, hàng N-1 = N objects
            float rowWidth  = (colCount - 1) * _spacing;        // độ rộng của hàng hiện tại
            float z         = _center.z + totalHeight / 2f - row * rowStep; // đỉnh ở Z+, đáy ở Z-

            for (int col = 0; col < colCount; col++)
            {
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
}
#endif
