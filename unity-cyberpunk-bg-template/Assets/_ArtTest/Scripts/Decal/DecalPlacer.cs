using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 씬에서 위치/회전/크기를 직접(수동으로) 배치한 Decal Projector에 붙이는 가벼운 헬퍼.
/// DecalScatterer처럼 자동으로 위치를 랜덤 생성하지 않는다 — 오브젝트 배치는 사람이 직접 한다.
///
/// 대신 머티리얼만큼은 DecalStyleSO.GetOrCreateMaterial()을 그대로 경유하므로,
/// 같은 (atlasIndex, designIndex) 조합을 쓰는 데칼끼리는 항상 같은 머티리얼 객체를 공유한다.
/// → 손으로 몇 개를 배치하든 머티리얼 개수는 "실제로 사용한 디자인 조합 수" 이하로 유지된다.
/// </summary>
[RequireComponent(typeof(DecalProjector))]
public class DecalPlacer : MonoBehaviour
{
    [Header("스타일")]
    public DecalStyleSO style;

    [Header("이 데칼이 쓸 디자인 (인덱스로 지정)")]
    public int atlasIndex = 0;
    public int designIndex = 0;

    private DecalProjector _projector;

#if UNITY_EDITOR
    // 인스펙터에서 값을 바꿀 때마다 즉시 반영 — 에디터에서 배치하면서 바로 확인 가능.
    // OnValidate 안에서 바로 머티리얼을 만들거나 바꾸면 씬 로딩/임포트 도중에 실행되어 경고가 날 수 있으므로,
    // 에디터의 다음 업데이트로 한 박자 미뤄서 실행한다.
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            // 프리팹 애셋 안(씬 밖)에서는 실행하지 않음 — 저장 안 되는 임시 머티리얼이 프리팹 애셋에 박히는 것 방지
            if (this != null && gameObject.scene.IsValid()) Apply();
        };
    }
#endif

    [ContextMenu("Apply")]
    public void Apply()
    {
        if (style == null || style.atlases == null || style.atlases.Length == 0) return;
        if (atlasIndex < 0 || atlasIndex >= style.atlases.Length) return;

        int maxDesign = style.atlases[atlasIndex].DesignCount;
        if (maxDesign <= 0)
        {
            Debug.LogWarning($"{name}: atlases[{atlasIndex}]의 gridSize가 0이라 디자인이 없습니다.");
            return;
        }
        designIndex = Mathf.Clamp(designIndex, 0, maxDesign - 1);

        Material mat = style.GetOrCreateMaterial(atlasIndex, designIndex);
        if (mat == null) return;

        if (_projector == null) _projector = GetComponent<DecalProjector>();
        _projector.material = mat;
    }
}
