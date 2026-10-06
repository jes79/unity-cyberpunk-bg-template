using UnityEngine;

/// <summary>
/// STEP 8 — LOD 테스트 씬 전용 카메라.
/// 카메라를 앞뒤로 움직이면서, 화면 왼쪽 위에 각 오브젝트의 "화면 점유율"과 "현재 LOD 단계"를 보여준다.
///
/// - Play 중 autoMove가 켜져 있으면 minDistance ~ maxDistance를 자동으로 왕복한다.
/// - Play가 아닐 때(또는 autoMove를 끄면) Inspector의 distance 슬라이더를 직접 드래그해서 확인할 수 있다.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class LODTestViewer : MonoBehaviour
{
    [Header("카메라 거리 (m)")]
    [Range(2f, 400f)] public float distance = 12f;
    public float height = 3f;

    [Header("자동 왕복 (Play 중에만)")]
    public bool autoMove = true;
    public float minDistance = 8f;
    public float maxDistance = 400f;
    [Tooltip("왕복 속도. 0.05면 약 20초에 한 번 갔다 옴")]
    public float speed = 0.05f;

    [Header("화면에 정보를 표시할 LOD Group")]
    public LODGroup[] targets;

    private Camera _camera;
    private GUIStyle _style;

    // [ExecuteAlways] — 편집 모드에서도 Inspector 값을 바꾸면 Update가 불려 카메라가 바로 따라온다
    private void Update()
    {
        if (autoMove && Application.isPlaying)
        {
            // 로그 스케일로 왕복 — 가까운 구간에서 너무 빨리 지나가지 않도록
            float t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.time * speed * 2f, 1f));
            distance = minDistance * Mathf.Pow(maxDistance / minDistance, t);
        }
        ApplyPosition();
    }

    private void ApplyPosition()
    {
        transform.SetPositionAndRotation(new Vector3(0f, height, -distance), Quaternion.identity);
    }

    private void OnGUI()
    {
        if (_camera == null) _camera = GetComponent<Camera>();
        if (_style == null)
            _style = new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.UpperLeft, richText = true };

        var text = new System.Text.StringBuilder();
        text.AppendLine($"카메라 거리  <b>{distance:0.0} m</b>     LOD Bias  <b>{QualitySettings.lodBias}</b>");
        text.AppendLine("<color=#4CFF4C>■ LOD0</color>   <color=#FFE14C>■ LOD1</color>   <color=#FF5A4C>■ LOD2</color>   □ Culled");
        text.AppendLine();

        if (targets != null)
        {
            foreach (LODGroup group in targets)
            {
                if (group == null) continue;
                float relative = ScreenRelativeHeight(group, _camera);
                int lod = CurrentLOD(group, relative);
                string lodText = lod < 0 ? "Culled" : $"LOD{lod}";
                text.AppendLine($"{group.name}   화면 {relative * 100f:0.0}%  →  <b>{lodText}</b>");
            }
        }

        GUILayout.BeginArea(new Rect(12, 12, 560, 400));
        GUILayout.Label(text.ToString().TrimEnd(), _style);
        GUILayout.EndArea();
    }

    /// <summary>
    /// Unity가 LOD를 고를 때 쓰는 값과 같은 방식으로 "화면 높이 대비 오브젝트 크기"를 계산.
    /// LOD Bias가 크면 거리를 줄인 것처럼 계산되어 더 오래 높은 LOD를 유지한다.
    /// </summary>
    public static float ScreenRelativeHeight(LODGroup group, Camera cam)
    {
        Vector3 scale = group.transform.lossyScale;
        float worldSize = group.size * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

        if (cam.orthographic)
            return worldSize * 0.5f / cam.orthographicSize;

        Vector3 center = group.transform.TransformPoint(group.localReferencePoint);
        float dist = Vector3.Distance(center, cam.transform.position) / QualitySettings.lodBias;
        float halfFov = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        return worldSize * 0.5f / (dist * halfFov);
    }

    /// <summary>화면 점유율이 각 LOD의 전환 기준 이상인 첫 단계. 모두 미만이면 -1(Culled).</summary>
    public static int CurrentLOD(LODGroup group, float screenRelativeHeight)
    {
        LOD[] lods = group.GetLODs();
        for (int i = 0; i < lods.Length; i++)
        {
            if (screenRelativeHeight >= lods[i].screenRelativeTransitionHeight)
                return i;
        }
        return -1;
    }
}
