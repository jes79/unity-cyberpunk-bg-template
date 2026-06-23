using UnityEngine;

/// <summary>
/// Inverted Hull 방식 아웃라인 — 원본 메쉬/머티리얼은 건드리지 않고,
/// 같은 메쉬를 쓰는 자식 렌더러를 하나 더 만들어서 Outline 머티리얼만 입힌다.
/// 다른 프로젝트에서도 이 스크립트 + Outline 셰이더 그래프 하나만 복사하면 그대로 동작.
///
/// 사용법:
///   1. 아웃라인을 원하는 오브젝트(MeshRenderer가 있는 GameObject)에 이 컴포넌트 부착
///   2. outlineMaterial에 Outline 셰이더 그래프로 만든 머티리얼 연결
///   3. enabled 토글로 건물별/오브젝트별 on/off
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OutlineToggle : MonoBehaviour
{
    [Tooltip("Outline 셰이더 그래프로 만든 머티리얼 (Cull Front, Vertex 확장)")]
    public Material outlineMaterial;

    [Tooltip("아웃라인 두께 (월드 단위, m)")]
    public float outlineWidth = 0.02f;

    [Tooltip("아웃라인 색상")]
    public Color outlineColor = Color.black;

    private GameObject outlineChild;
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");

    private void OnEnable()
    {
        CreateOrUpdateOutline();
    }

    private void OnDisable()
    {
        if (outlineChild != null)
            outlineChild.SetActive(false);
    }

    /// <summary>
    /// 인스펙터에서 값 바꿨을 때 바로 반영되도록 (OnValidate에서도 호출됨).
    /// </summary>
    [ContextMenu("아웃라인 갱신")]
    public void CreateOrUpdateOutline()
    {
        if (outlineMaterial == null)
        {
            Debug.LogWarning($"[OutlineToggle] '{name}'에 outlineMaterial이 비어있어 아웃라인을 만들 수 없음.");
            return;
        }

        if (outlineChild == null)
        {
            outlineChild = new GameObject("Outline");
            outlineChild.transform.SetParent(transform, false);
            outlineChild.transform.localPosition = Vector3.zero;
            outlineChild.transform.localRotation = Quaternion.identity;
            outlineChild.transform.localScale = Vector3.one;

            MeshFilter sourceFilter = GetComponent<MeshFilter>();
            MeshFilter outlineFilter = outlineChild.AddComponent<MeshFilter>();
            outlineFilter.sharedMesh = sourceFilter.sharedMesh;

            outlineChild.AddComponent<MeshRenderer>();
        }

        outlineChild.SetActive(true);

        MeshRenderer outlineRenderer = outlineChild.GetComponent<MeshRenderer>();
        outlineRenderer.sharedMaterial = outlineMaterial;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        outlineRenderer.GetPropertyBlock(block);
        block.SetFloat(OutlineWidthId, outlineWidth);
        block.SetColor(OutlineColorId, outlineColor);
        outlineRenderer.SetPropertyBlock(block);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (outlineChild != null)
            CreateOrUpdateOutline();
    }
#endif
}
