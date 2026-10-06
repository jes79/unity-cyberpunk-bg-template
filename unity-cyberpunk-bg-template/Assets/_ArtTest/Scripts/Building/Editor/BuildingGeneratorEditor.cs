// BuildingGeneratorEditor.cs
// ⚠️ 반드시 "Editor" 라는 이름의 폴더 안에 위치해야 함 (Unity 규칙 — 빌드 시 자동 제외됨)
//
// ⚠️ 주의: 프로젝트에 이미 BuildingGenerator용 커스텀 에디터가 있다면
// [CustomEditor(typeof(BuildingGenerator))]가 중복되어 컴파일 에러(CS0101류)가 남.
// 이 파일을 넣기 전에 Assets 전체에서 "CustomEditor(typeof(BuildingGenerator))"를
// 검색해서 기존에 또 있는지 먼저 확인할 것.
//
// Inspector 버튼으로 실행한 Generate/Clear는 Undo에 기록되어 Ctrl+Z로 되돌릴 수 있다.
// (Play 진입/종료 때 자동으로 도는 재생성은 Undo에 남지 않도록 런타임 쪽 코드는 그대로 둠)

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingGenerator))]
public class BuildingGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var gen = (BuildingGenerator)target;

        EditorGUILayout.Space(8);

        if (GUILayout.Button("Generate", GUILayout.Height(32)))
        {
            GenerateWithUndo(gen);
        }

        if (GUILayout.Button("Clear"))
        {
            ClearWithUndo(gen, "Clear Building");
        }
    }

    private static void GenerateWithUndo(BuildingGenerator gen)
    {
        Undo.SetCurrentGroupName("Generate Building");
        int group = Undo.GetCurrentGroup();

        Undo.RecordObject(gen, "Generate Building");
        // 기존 자식을 먼저 Undo 기록과 함께 지워두면, Generate() 안의 Clear()는 지울 게 없어서 그냥 지나간다
        DestroyChildrenWithUndo(gen.transform);

        gen.Generate();

        foreach (Transform child in gen.transform)
            Undo.RegisterCreatedObjectUndo(child.gameObject, "Generate Building");

        Undo.CollapseUndoOperations(group);
    }

    private static void ClearWithUndo(BuildingGenerator gen, string undoName)
    {
        Undo.SetCurrentGroupName(undoName);
        int group = Undo.GetCurrentGroup();
        DestroyChildrenWithUndo(gen.transform);
        Undo.CollapseUndoOperations(group);
    }

    /// <summary>
    /// 자식 오브젝트와, 그 자식들이 쓰던 인스턴스 머티리얼(간판/쇼윈도/창문 유리)을 Undo 기록과 함께 삭제.
    /// 머티리얼도 같이 기록해야 Ctrl+Z로 되살렸을 때 유리/간판이 핑크로 나오지 않는다.
    /// </summary>
    private static void DestroyChildrenWithUndo(Transform root)
    {
        var instanceMaterials = new HashSet<Material>();
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (BuildingMaterialApplier.IsInstanceMaterial(renderer.sharedMaterial))
                instanceMaterials.Add(renderer.sharedMaterial);
        }

        for (int i = root.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);

        foreach (Material mat in instanceMaterials)
            Undo.DestroyObjectImmediate(mat);
    }
}
#endif
