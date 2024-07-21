using UnityEngine;
using UnityEditor;


public class DotProductEditor : EditorWindow
{
    // 벡터 변수 선언
    public Vector3 m_p0;
    public Vector3 m_p1;
    public Vector3 m_c;

    // 직렬화 객체와 속성 선언
    private SerializedObject obj;
    private SerializedProperty propP0;
    private SerializedProperty propP1;
    private SerializedProperty propC;

    // GUI 스타일 선언
    private GUIStyle guiStyle = new GUIStyle();


    // 메뉴에 "Tools/Dot Product" 항목을 추가하고, 선택 시 에디터 창을 창을 연다.
    [MenuItem("Tools/Dot Product")]
    public static void ShowWindow()
    {
        DotProductEditor window = (DotProductEditor)GetWindow(typeof(DotProductEditor), true, "Dot Product");
        window.Show();
    }

    // 에디터 창이 활성화될 때 호출
    private void OnEnable()
    {
        // 벡터 값 초기화
        if (m_p0 == Vector3.zero && m_p1 == Vector3.zero)
        {
            m_p0 = new Vector3(0.0f, 1.0f, 0.0f);
            m_p1 = new Vector3(0.5f, 0.5f, 0.0f);
            m_c = Vector3.zero;
        }

        // 직렬화 객체와 속성 초기화
        obj = new SerializedObject(this);
        propP0 = obj.FindProperty("m_p0");
        propP1 = obj.FindProperty("m_p1");
        propC = obj.FindProperty("m_c");

        // GUI 스타일 설정
        guiStyle.fontSize = 25;
        guiStyle.fontStyle = FontStyle.Bold;
        guiStyle.normal.textColor = Color.white;

        // SceneView의 duringSceneGui 이벤트에 SceneGUI 메서드 등록
        SceneView.duringSceneGui += SceneGUI;
    }

    // 에디터 창이 비활성화될 때 호출
    private void OnDisable()
    {
         // SceneView의 duringSceneGui 이벤트에 SceneGUI 메서드 제거
        SceneView.duringSceneGui -= SceneGUI;
    }

    // 에디터 창의 GUI를 그립니다.
    private void OnGUI()
    {
        // 직렬화 객체 업데이트
        obj.Update();

        // 각 벡터에 대한 속성을 그립니다.
        DrawBlockGUI("p0", propP0);
        DrawBlockGUI("p1", propP1);
        DrawBlockGUI("c", propC);

        // 변경 사항이 적용되면 SceneView를 다시 그립니다.
        if (obj.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
        }
    }

    // 벡터 속성을 그리는 메서드
    private void DrawBlockGUI(string lab, SerializedProperty prop)
    {
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField(lab, GUILayout.Width(50));
        EditorGUILayout.PropertyField(prop, GUIContent.none);
        EditorGUILayout.EndHorizontal();
    }

    // Scene 뷰에서 벡터 핸들을 그리며 벡터를 이동할 수 있게 합니다.
    private void SceneGUI(SceneView view)
    {
        Handles.color = Color.red;
        Vector3 p0 = SetMovePoint(m_p0);
        Handles.color = Color.green;
        Vector3 p1 = SetMovePoint(m_p1);
        Handles.color = Color.white;
        Vector3 c = SetMovePoint(m_c);

        // 벡터가 이동하면 벡터 값을 업데이트하고 창을 다시 그립니다.
        if (m_p0 != p0 || m_p1 != p1 || m_c != c)
        {
            m_p0 = p0;
            m_p1 = p1;
            m_c = c;

            Repaint();
        }

        // 라벨과 벡터 간의 선을 그립니다.
        DrawLabel(p0, p1, c);
    }


    // 벡터의 내적 값을 라벨을 표시하고 벡터 간의 선을 그립니다.
    private void DrawLabel(Vector3 p0, Vector3 p1, Vector3 c)
    {
        Handles.Label(c, DotProduct(p0, p1, c).ToString("F1"), guiStyle);
        Handles.color = Color.black;

        // 보조 선을 위한 벡터 계산
        Vector3 cLef = WorldRotation(p0, c, new Vector3(0, 1f, 0f));
        Vector3 cRig = WorldRotation(p0, c, new Vector3(0f, -1f, 0f));

        // 벡터 간의 선 그리기
        Handles.DrawAAPolyLine(3f, p0, c);
        Handles.DrawAAPolyLine(3f, p1, c);
        Handles.DrawAAPolyLine(3f, c, cLef);
        Handles.DrawAAPolyLine(3f, c, cRig);
    }

    // 벡터 핸들을 설정하고 사용자가 이동할 수 있게 합니다.
    private Vector3 SetMovePoint(Vector3 pos)
    {
        float size = HandleUtility.GetHandleSize(Vector3.zero) * 0.15f;
        return Handles.FreeMoveHandle(pos, Quaternion.identity, size, Vector3.zero, Handles.SphereHandleCap);
    }

    // 두 벡터의 내적 값을 계산하여 반환합니다.
    private float DotProduct(Vector3 p0, Vector3 p1, Vector3 c)
    {
        Vector3 a = (p0 - c).normalized;
        Vector3 b = (p1 - c).normalized;

        return ((a.x * b.x) + (a.y * b.y) + (a.z * b.z));
    }

    // 두 벡터의 회전값을 계산하여 반환합니다.
    private Vector3 WorldRotation(Vector3 p, Vector3 c, Vector3 pos)
    {
        Vector2 dir = (p - c).normalized;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(ang, Vector3.forward);

        return c + rot * pos;
    }

}
