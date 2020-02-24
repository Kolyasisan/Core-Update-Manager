//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

using UnityEngine;
using UnityEditor;

public class CoreUpdateManagerDebugger : EditorWindow
{
    [MenuItem("Window/Analysis/Core Update Manager Debugger")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        CoreUpdateManagerDebugger window = (CoreUpdateManagerDebugger)EditorWindow.GetWindow(typeof(CoreUpdateManagerDebugger));
        window.titleContent = new GUIContent("CoreUpdateManagerDebugger");
        window.Show();
    }

    private void OnEnable()
    {
        this.autoRepaintOnSceneChange = true;
    }

    Vector2 scroll;
    private void OnGUI()
    {
        CoreUpdateManager.TryInitialize();

        scroll = EditorGUILayout.BeginScrollView(scroll, GUIStyle.none, GUI.skin.verticalScrollbar);

        if (CoreUpdateManager.Instance != null && CoreUpdateManager.Instance.BehaviourQueues != null && CoreUpdateManager.Instance.BehaviourQueues.Count > 0)
        {
            foreach (var queue in CoreUpdateManager.Instance.BehaviourQueues)
            {
                DrawQueue(queue);
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("No loops have been initialized yet");
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    void DrawQueue(BehaviourLoopBase loop)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbarButton);

        string loopTypeName = loop.GetType().Name;
        string loopDisplayName = loopTypeName;

        bool fold = EditorGUILayout.Foldout(EditorPrefs.GetBool(loopTypeName + "_unfolded"), loopDisplayName, true); // use the GetFoldout helper method to see if its open or closed
        EditorPrefs.SetBool(loopTypeName + "_unfolded", fold);

        EditorGUILayout.EndHorizontal();

        if (fold)
        {
            int prevOrder = 0;
            bool orderSet = false;
            var queue = loop.GetQueue();

            if (queue != null)
            {
                for (int i = loop.LowerBound + 1; i < loop.UpperBound; i++)
                {
                    var beh = queue[i];

                    if (beh == null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("null");
                        EditorGUILayout.EndHorizontal();
                        continue;
                    }

                    int thisOrder = loop.GetSettings(beh).UpdateOrder;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space((float)EditorGUI.indentLevel * 18f);

                    MonoBehaviour behAsMonoBeh = beh as MonoBehaviour;
                    bool behIsMonobeh = behAsMonoBeh == null ? false : true;

                    if (behIsMonobeh)
                    {
                        if (GUILayout.Button("@"))
                        {
                            Selection.activeGameObject = behAsMonoBeh.gameObject;
                        }
                    }
                    else
                    {
                        if(GUILayout.Button(" "))
                        {

                        }
                    }

                    if (behIsMonobeh)
                    {
                        GUILayout.Label($"{beh.GetType().Name} ({behAsMonoBeh.gameObject.name}) (Order value: {thisOrder.ToString()})");
                    }
                    else
                    {
                        GUILayout.Label($"{beh.GetType().Name} (Order value: {thisOrder.ToString()})");
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    if (orderSet && prevOrder != thisOrder)
                    {
                        GUILayout.Space(EditorGUI.indentLevel);
                    }

                    prevOrder = thisOrder;
                    orderSet = true;
                }
            }
        }
    }
}
