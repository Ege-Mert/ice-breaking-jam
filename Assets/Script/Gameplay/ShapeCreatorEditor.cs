using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShapeCreator))]
public class ShapeCreatorEditor : Editor
{
    private ShapeCreator shapeCreator;
    private bool isCreating = false;
    private Tool lastTool = Tool.None;

    private void OnEnable()
    {
        shapeCreator = (ShapeCreator)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Shape Creation Tools", EditorStyles.boldLabel);

        if (GUILayout.Button(isCreating ? "Stop Creating" : "Start Creating"))
        {
            isCreating = !isCreating;
            if (isCreating)
            {
                lastTool = Tools.current;
                Tools.current = Tool.None;
            }
            else
            {
                Tools.current = lastTool;
            }
        }

        if (GUILayout.Button("Clear Points"))
        {
            shapeCreator.ClearPoints();
        }

        if (GUILayout.Button("Save Shape"))
        {
            SaveShape();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Click in scene view to add points.\nPress ESC to stop creating.", MessageType.Info);
    }

    private void OnSceneGUI()
    {
        if (!isCreating) return;

        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 mousePosition = ray.origin;
            mousePosition.z = 0;  // Ensure 2D position

            shapeCreator.AddPoint(mousePosition);
            e.Use();  // Consume the event
        }
        else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            isCreating = false;
            Tools.current = lastTool;
            e.Use();
        }

        // Force scene view to repaint
        if (isCreating)
        {
            SceneView.RepaintAll();
        }
    }

    private void SaveShape()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Shape",
            "NewShape",
            "asset",
            "Save shape as..."
        );

        if (string.IsNullOrEmpty(path)) return;

        ShapeData shapeData = ScriptableObject.CreateInstance<ShapeData>();
        shapeData.pathPoints = shapeCreator.GetPoints();
        
        AssetDatabase.CreateAsset(shapeData, path);
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = shapeData;
    }
}