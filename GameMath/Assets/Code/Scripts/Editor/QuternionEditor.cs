using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using System.Collections.Generic;



namespace GameMath
{
    public class QuternionEditor : CommonEditor, IUpdateSceneGUI
    {
        // 회전 각도와 축을 나타내는 변수
        [Range(-360, 360)]
        public float m_angle = 0f;
        public Vector3 m_axis = new Vector3(0, 1, 0);

        // 직렬화 객체와 속성들 선언
        private SerializedObject obj;
        private SerializedProperty propAngle;
        private SerializedProperty propAxis;

        // 회전시킬 정점들을 저장할 리스트
        private List<Vector3> vertices;

        [MenuItem("Tools/Quaternion")]
        public static void ShowWindow()
        {
            GetWindow(typeof(QuternionEditor), true, "Quaternion");
        }

        private void OnEnable()
        {
            // 직렬화 객체와 속성 초기화
            obj = new SerializedObject(this);
            propAngle = obj.FindProperty("m_angle");
            propAxis = obj.FindProperty("m_axis");

            SceneView.duringSceneGui += SceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= SceneGUI;
        }

        /// <summary>
        /// scene 뷰에서 정점들을 그리고 회전시킵니다.
        /// </summary>
        public void SceneGUI(SceneView view)
        {
            // 정점들을 초기화
            vertices = new List<Vector3>
            {
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3( 0.5f, 0.5f, 0.5f),
                new Vector3( 0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3( 0.5f, 0.5f, -0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f)
            };

            // 각도를 라디안으로 변환
            float angle = m_angle * Mathf.PI / 180;

            // 각 정점들을 회전시킴
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = HQuaternion.Rotate(vertices[i], m_axis, angle);
                Handles.SphereHandleCap(0, vertices[i],
                Quaternion.identity, 0.1f, EventType.Repaint);
            }

            // 정점들을 연결하는 인덱스 배열
            int[][] index =
            {
                new int[] {0,1},
                new int[] {1,2},
                new int[] {2,3},
                new int[] {3,0},
                new int[] {4,5},
                new int[] {5,6},
                new int[] {6,7},
                new int[] {7,4},
                new int[] {4,0},
                new int[] {5,1},
                new int[] {6,2},
                new int[] {7,3}
            };

            // 정점들을 연결하여 선을 그림
            for (int i = 0; i < index.Length; i++)
            {
                Handles.DrawAAPolyLine(vertices[index[i][0]], vertices[index[i][1]]);
            }
        }

        // 에디터 창의 GUI를 그립니다.
        private void OnGUI()
        {
            // 직렬화 객체 업데이트
            obj.Update();

            // 각 속성을 위한 GUI 블록을 그립니다.
            DrawBlockGUI("Angle", propAngle);
            DrawBlockGUI("Axis", propAxis);

            // 변경 사항이 적용되면 SceneView를 다시 그립니다.
            if (obj.ApplyModifiedProperties())
            {
                SceneView.RepaintAll();
            }
        }
    }

    // 쿼터니언을 사용하여 회전을 계산하는 함수들을 포함
    public struct HQuaternion
    {
        private float x;
        private float y;
        private float z;
        private float w;

        public HQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// 주어진 축과 각도로 쿼터니언을 생성하는 메서드
        /// </summary>
        private static HQuaternion Create(float angle, Vector3 axis)
        {
            float sin = Mathf.Sin(angle / 2f);
            float cos = Mathf.Cos(angle / 2f);
            Vector3 v = Vector3.Normalize(axis) * sin;

            return new HQuaternion(v.x, v.y, v.z, cos);
        }


        /// <summary>
        /// 주어진 쿼터니언의 컬레를 반환하는 메서드
        /// </summary>
        private static HQuaternion Conjugate(HQuaternion q)
        {
            float s = q.w;
            Vector3 v = new Vector3(-q.x, -q.y, -q.z);

            return new HQuaternion(v.x, v.y, v.z, s);
        }

        /// <summary>
        /// 두 쿼터니언을 곱하는 메서드
        /// </summary>
        private static HQuaternion Multiplication(HQuaternion q1, HQuaternion q2)
        {
            float s1 = q1.w;
            float s2 = q2.w;

            Vector3 v1 = new Vector3(q1.x, q1.y, q1.z);
            Vector3 v2 = new Vector3(q2.x, q2.y, q2.z);

            float s = s1 * s2 - Vector3.Dot(v1, v2);
            Vector3 v = s1 * v2 + s2 * v1 + Vector3.Cross(v1, v2);

            return new HQuaternion(v.x, v.y, v.z, s);
        }


        /// <summary>
        /// 주어진 점을 주어진 축과 각도로 회전시키는 메서드
        /// </summary>
        public static Vector3 Rotate(Vector3 point, Vector3 axis, float angle)
        {
            HQuaternion q = Create(angle, axis);
            HQuaternion _q = Conjugate(q);
            HQuaternion p = new HQuaternion(point.x, point.y, point.z, 0f);

            HQuaternion rotatedPoint = Multiplication(q, p);
            rotatedPoint = Multiplication(rotatedPoint, _q);

            return new Vector3(rotatedPoint.x, rotatedPoint.y, rotatedPoint.z);
        }
    }
}
