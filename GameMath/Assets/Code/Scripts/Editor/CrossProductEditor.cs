using UnityEngine;
using UnityEditor;

namespace GameMath
{
    public class CrossProductEditor : CommonEditor, IUpdateSceneGUI
    {
        public Vector3 m_p;
        public Vector3 m_q;
        public Vector3 m_pxq;

        private SerializedObject obj;
        private SerializedProperty propP;
        private SerializedProperty propQ;
        private SerializedProperty propPXQ;

        private GUIStyle guiStyle = new GUIStyle();


        [MenuItem("Tools/Cross Product")]
        public static void ShowWindow()
        {
            GetWindow(typeof(CrossProductEditor), true, "Cross Product");
        }


        /// <summary>
        /// 기본 벡터 값을 설정
        /// </summary>
        private void SetDefaultValues()
        {
            m_p = new Vector3(0.0f, 1.0f, 0.0f);
            m_q = new Vector3(1.0f, 0.0f, 0.0f);
        }

        private void OnEnable()
        {
            // 벡터 값이 초기화되 않았다면 기본 값 설정
            if (m_p == Vector3.zero && m_q == Vector3.zero)
            {
                SetDefaultValues();
            }

            // 직렬화 객체와 속성 초기화
            obj = new SerializedObject(this);
            propP = obj.FindProperty("m_p");
            propQ = obj.FindProperty("m_q");
            propPXQ = obj.FindProperty("m_pxq");

            // GUI 스타일 설정
            guiStyle.fontSize = 25;
            guiStyle.fontStyle = FontStyle.Bold;
            guiStyle.normal.textColor = Color.white;

            // SceneGUI 이벤트 등록
            SceneView.duringSceneGui += SceneGUI;

            // Undo 이벤트 등록
            Undo.undoRedoPerformed += RepaintOnGUI;
        }

        private void OnDisable()
        {
            // SceneGUI 이벤트 해제
            SceneView.duringSceneGui -= SceneGUI;

            // Undo 이벤트 해제
            Undo.undoRedoPerformed -= RepaintOnGUI;
        }


        /// <summary>
        /// 에디터 창의 GUI를 그립니다.
        /// </summary>
        private void OnGUI()
        {
            // 직렬화 객체 업데이트
            obj.Update();

            // 각 벡터에 대한 속성을 그립니다.
            DrawBlockGUI("p", propP);
            DrawBlockGUI("q", propQ);
            DrawBlockGUI("PXQ", propPXQ);

            // 변경 사항이 적용되면 SceneView를 다시 그립니다.
            if (obj.ApplyModifiedProperties())
            {
                SceneView.RepaintAll();
            }

            // 리셋 버튼을 누르면 기본 값으로 설정합니다.
            if (GUILayout.Button("Reset Values"))
            {
                SetDefaultValues();
            }

        }

        /// <summary>
        /// SceneView 뷰에서 벡터 핸들을 그리며 벡터를 이동할 수 있게 합니다.
        /// </summary>
        /// <param name="view"></param>
        public void SceneGUI(SceneView view)
        {
            // 벡터 핸들을 그립니다.
            Vector3 p = Handles.PositionHandle(m_p, Quaternion.identity);
            Vector3 q = Handles.PositionHandle(m_q, Quaternion.identity);

            // 교차 곱 벡터를 계산하여 파란색 디스크로 그립니다.
            Handles.color = Color.blue;
            Vector3 pxq = CrossProduct(p, q);
            Handles.DrawSolidDisc(pxq, Vector3.forward, 0.05f);

            // 벡터가  이동되면 벡터 값을 업데이트하고 창을 다시 그립니다.
            if (m_p != p || m_q != q)
            {
                Undo.RecordObject(this, "Tool Move");

                m_p = p;
                m_q = q;
                m_pxq = pxq;

                RepaintOnGUI();
            }

            // 각 벡터와 라벨을 그립니다.
            DrawLineGUI(p, "P", Color.green);
            DrawLineGUI(q, "Q", Color.red);
            DrawLineGUI(pxq, "PXQ", Color.blue);
        }

        /// <summary>
        /// 벡터와 라벨을 그리는 메서드
        /// </summary>
        private void DrawLineGUI(Vector3 pos, string tex, Color col)
        {
            Handles.color = col;
            Handles.Label(pos, tex, guiStyle);
            Handles.DrawAAPolyLine(3f, pos, Vector3.zero);
        }

        /// <summary>
        /// 에디터 창을 다시 그리는 메서드
        /// </summary>
        private void RepaintOnGUI()
        {
            Repaint();
        }

        // private Vector3 CrossProduct(Vector3 p, Vector3 q)
        // {
        //     float x = p.y * q.z - p.z * q.y;
        //     float y = p.z * q.x - p.x * q.z;
        //     float z = p.x * q.y - p.y * q.x;

        //     return new Vector3(x, y, z);
        // }

        /// <summary>
        /// 벡터의 교차 곱을 계산하는 메서드 (행렬을 사용하여 계산)
        /// </summary>
        private Vector3 CrossProduct(Vector3 p, Vector3 q)
        {
            // 3x3 행렬을 생성하여 교차 곱을 계산합니다.
            Matrix4x4 m = new Matrix4x4();
            m[0, 0] = 0;
            m[0, 1] = q.z;
            m[0, 2] = -q.y;

            m[1, 0] = -q.z;
            m[1, 1] = 0;
            m[1, 2] = q.x;

            m[2, 0] = q.y;
            m[2, 1] = -q.x;
            m[2, 2] = 0;

            // 행렬과 벡터의 곱을 통해 교차 곱을 계산하여 반환합니다.
            return m * p;
        }
    }
}