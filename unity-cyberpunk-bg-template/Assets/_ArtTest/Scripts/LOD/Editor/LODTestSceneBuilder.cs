// LODTestSceneBuilder.cs
// ⚠️ 반드시 "Editor" 폴더 안에 위치 (빌드 시 자동 제외)
//
// STEP 8 — 지금 열려 있는 씬에 LOD 테스트 오브젝트를 만든다.
//   사용법: 새 씬을 만들어 연 다음 → 메뉴 CyberpunkBG > STEP 8 - 현재 씬에 LOD 테스트 만들기
//   (Ctrl+Z로 한 번에 되돌릴 수 있음)
//
// 만들어지는 것
//   씬 안:  "LOD Test" 오브젝트 아래에 바닥, 큰 구, 작은 구, 소품 박스 5개 + 카메라에 LODTestViewer
//   에셋:   Assets/_ArtTest/Prefabs/LODTest/ (처음 한 번만 생성, 이후엔 재사용)
//     LOD_Sphere.prefab  — 3단계 LOD (LOD0 초록 / LOD1 노랑 / LOD2 빨강) + Culled
//     LOD_PropBox.prefab — 컬링 전용 LOD (단계 1개 + Culled) : "작은 소품은 멀면 그냥 끈다"
//
// 큰 구(지름 6m)와 작은 구(지름 2m)에 "같은 LOD 설정"을 써서,
// 같은 거리에서도 크기에 따라 전환 시점이 달라지는 것(화면 점유율 기준)을 눈으로 확인한다.

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LODTestSceneBuilder
{
    private const string AssetFolder = "Assets/_ArtTest/Prefabs/LODTest";
    private const string SpherePrefabPath = AssetFolder + "/LOD_Sphere.prefab";
    private const string PropPrefabPath = AssetFolder + "/LOD_PropBox.prefab";
    private const string UndoName = "Create LOD Test";

    // LOD 전환 기준 (화면 높이 대비 비율). 이 값보다 작아지면 다음 단계로 넘어간다.
    private const float SphereLOD0 = 0.30f;   // 30% 미만 → LOD1
    private const float SphereLOD1 = 0.12f;   // 12% 미만 → LOD2
    private const float SphereLOD2 = 0.03f;   //  3% 미만 → Culled
    private const float PropCull = 0.05f;     // 소품: 5% 미만 → Culled

    [MenuItem("CyberpunkBG/STEP 8 - 현재 씬에 LOD 테스트 만들기")]
    public static void Build()
    {
        GetOrCreatePrefabs(out GameObject spherePrefab, out GameObject propPrefab);

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        var root = new GameObject("LOD Test");
        Undo.RegisterCreatedObjectUndo(root, UndoName);

        GameObject groundObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        groundObj.name = "Ground";
        groundObj.transform.SetParent(root.transform, false);
        // 카메라가 -Z 방향으로 최대 400m까지 물러나므로 바닥도 그쪽으로 길게 깐다 (Plane 1칸 = 10m)
        groundObj.transform.localScale = new Vector3(4f, 1f, 45f);
        groundObj.transform.position = new Vector3(0f, 0f, -200f);
        groundObj.GetComponent<Renderer>().sharedMaterial =
            AssetDatabase.LoadAssetAtPath<Material>(AssetFolder + "/M_Ground.mat");

        var targets = new List<LODGroup>
        {
            PlaceInstance(spherePrefab, root, "큰 구 (지름 6m)", new Vector3(-4f, 3f, 0f), 6f),
            PlaceInstance(spherePrefab, root, "작은 구 (지름 2m)", new Vector3(3f, 1f, 0f), 2f),
        };

        // 소품 박스 5개 — 첫 번째만 화면 표시 대상으로 등록
        for (int i = 0; i < 5; i++)
        {
            LODGroup prop = PlaceInstance(propPrefab, root, i == 0 ? "소품 박스 (0.5m)" : $"소품 박스 {i + 1}",
                new Vector3(5.5f + i * 0.9f, 0.25f, 0f), 1f);
            if (i == 0) targets.Add(prop);
        }

        // 카메라 — 씬의 Main Camera를 쓰고, 없으면 새로 만든다
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camObj = new GameObject("Main Camera") { tag = "MainCamera" };
            Undo.RegisterCreatedObjectUndo(camObj, UndoName);
            cam = camObj.AddComponent<Camera>();
        }
        Undo.RecordObject(cam, UndoName);
        cam.farClipPlane = Mathf.Max(cam.farClipPlane, 1000f);

        var viewer = cam.GetComponent<LODTestViewer>();
        if (viewer == null) viewer = Undo.AddComponent<LODTestViewer>(cam.gameObject);
        Undo.RecordObject(viewer, UndoName);
        viewer.targets = targets.ToArray();
        viewer.distance = 12f;

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = cam.gameObject;

        Debug.Log("[LODTestSceneBuilder] LOD 테스트 생성 완료 — Play를 누르면 카메라가 앞뒤로 움직입니다. " +
                  "(Play 없이 보려면 Main Camera의 LODTestViewer > Distance 슬라이더를 드래그)");
    }

    // ---------------------------------------------------------------------

    private static void GetOrCreatePrefabs(out GameObject spherePrefab, out GameObject propPrefab)
    {
        spherePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpherePrefabPath);
        propPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PropPrefabPath);
        if (spherePrefab != null && propPrefab != null) return;

        if (!AssetDatabase.IsValidFolder(AssetFolder))
            AssetDatabase.CreateFolder("Assets/_ArtTest/Prefabs", "LODTest");

        // 머티리얼 — LOD 단계를 색으로 구분
        Material green = CreateMaterial("M_LOD0_Green", new Color(0.30f, 1.00f, 0.30f));
        Material yellow = CreateMaterial("M_LOD1_Yellow", new Color(1.00f, 0.88f, 0.30f));
        Material red = CreateMaterial("M_LOD2_Red", new Color(1.00f, 0.35f, 0.30f));
        Material gray = CreateMaterial("M_Gray", new Color(0.55f, 0.55f, 0.58f));
        CreateMaterial("M_Ground", new Color(0.18f, 0.18f, 0.20f));

        // 메쉬 — 단계별로 분할 수를 줄인 구
        Mesh sphere0 = SaveMesh(CreateSphere(48, 32), "SM_Sphere_LOD0"); // 삼각형 약 3,000개
        Mesh sphere1 = SaveMesh(CreateSphere(16, 10), "SM_Sphere_LOD1"); // 약 320개
        Mesh sphere2 = SaveMesh(CreateSphere(8, 5), "SM_Sphere_LOD2");   // 약 80개

        spherePrefab = SavePrefab(BuildLODSphere(sphere0, sphere1, sphere2, green, yellow, red), SpherePrefabPath);
        propPrefab = SavePrefab(BuildPropBox(gray), PropPrefabPath);
        AssetDatabase.SaveAssets();
    }

    private static GameObject BuildLODSphere(Mesh lod0, Mesh lod1, Mesh lod2, Material m0, Material m1, Material m2)
    {
        var root = new GameObject("LOD_Sphere");
        // 자식 이름 규칙: <이름>_LOD0, _LOD1 … — 3ds Max에서 FBX로 내보낼 때도 이 이름이면 Unity가 LOD Group을 자동으로 만들어 줌
        Renderer r0 = CreateChild(root, "LOD_Sphere_LOD0", lod0, m0);
        Renderer r1 = CreateChild(root, "LOD_Sphere_LOD1", lod1, m1);
        Renderer r2 = CreateChild(root, "LOD_Sphere_LOD2", lod2, m2);

        LODGroup group = root.AddComponent<LODGroup>();
        group.fadeMode = LODFadeMode.CrossFade;
        group.animateCrossFading = true;
        group.SetLODs(new[]
        {
            new LOD(SphereLOD0, new[] { r0 }),
            new LOD(SphereLOD1, new[] { r1 }),
            new LOD(SphereLOD2, new[] { r2 }),
        });
        group.RecalculateBounds();
        return root;
    }

    private static GameObject BuildPropBox(Material mat)
    {
        var root = new GameObject("LOD_PropBox");
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(cube.GetComponent<Collider>());
        cube.name = "LOD_PropBox_LOD0";
        cube.transform.SetParent(root.transform, false);
        cube.transform.localScale = Vector3.one * 0.5f;
        cube.GetComponent<Renderer>().sharedMaterial = mat;

        // 단계가 하나뿐인 LOD — 일정 크기보다 작아지면 그냥 끈다 (컬링 전용)
        LODGroup group = root.AddComponent<LODGroup>();
        group.SetLODs(new[] { new LOD(PropCull, new[] { cube.GetComponent<Renderer>() }) });
        group.RecalculateBounds();
        return root;
    }

    private static Renderer CreateChild(GameObject parent, string name, Mesh mesh, Material mat)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent.transform, false);
        child.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = child.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;
        return renderer;
    }

    private static LODGroup PlaceInstance(GameObject prefab, GameObject parent, string name, Vector3 position, float scale)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
        instance.name = name;
        instance.transform.position = position;
        instance.transform.localScale = Vector3.one * scale;
        return instance.GetComponent<LODGroup>();
    }

    private static GameObject SavePrefab(GameObject temp, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
        Object.DestroyImmediate(temp);
        return prefab;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = $"{AssetFolder}/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static Mesh SaveMesh(Mesh mesh, string name)
    {
        string path = $"{AssetFolder}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) return existing;

        mesh.name = name;
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    /// <summary>지름 1m UV 구. longitude=가로 분할 수, latitude=세로 분할 수 (클수록 매끈하고 삼각형이 많음)</summary>
    private static Mesh CreateSphere(int longitude, int latitude)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        for (int y = 0; y <= latitude; y++)
        {
            float v = (float)y / latitude;
            float theta = v * Mathf.PI;
            for (int x = 0; x <= longitude; x++)
            {
                float u = (float)x / longitude;
                float phi = u * Mathf.PI * 2f;
                var n = new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Cos(theta), Mathf.Sin(theta) * Mathf.Sin(phi));
                vertices.Add(n * 0.5f);
                normals.Add(n);
                uvs.Add(new Vector2(u, 1f - v));
            }
        }

        for (int y = 0; y < latitude; y++)
        {
            for (int x = 0; x < longitude; x++)
            {
                int i0 = y * (longitude + 1) + x;
                int i1 = i0 + longitude + 1;
                triangles.AddRange(new[] { i0, i0 + 1, i1 });
                triangles.AddRange(new[] { i1, i0 + 1, i1 + 1 });
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }
}
#endif
