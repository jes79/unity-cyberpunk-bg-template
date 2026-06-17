# Unity 6 URP — 사이버펑크 배경 제작 템플릿

Unity 6.3 LTS / URP 기반 사이버펑크 도시 배경 제작용 템플릿 프로젝트입니다.
새 배경 프로젝트를 시작할 때 클론해서 모델과 텍스처만 교체하면 바로 작업을 시작할 수 있도록 구성했습니다.

## 환경

| 항목 | 버전 |
|---|---|
| Unity | 6.3 LTS (6000.3.9f1) |
| 렌더 파이프라인 | URP (Forward+) |
| 셰이더 | HLSL + Shader Graph 병행 |

## 시작하기

```bash
git clone https://github.com/아이디/unity-cyberpunk-bg-template
cd unity-cyberpunk-bg-template
git lfs pull
```

Unity Hub에서 6000.3.9f1 버전으로 프로젝트를 엽니다.

## 폴더 구조

Unity URP 템플릿이 기본으로 생성하는 `Settings`, `TutorialInfo`, `InputSystem_Actions`, `Readme`, `Scenes` 등은 그대로 두고 손대지 않습니다. 우리가 작업하는 영역은 `_Art`와 `_ArtTest` 두 폴더로 한정합니다.

```
Assets/
├── Settings/          Unity 기본 생성 — 손대지 않음
├── TutorialInfo/       Unity 기본 생성 — 손대지 않음
├── InputSystem_Actions Unity 기본 생성 — 손대지 않음
├── Readme              Unity 기본 생성 — 손대지 않음
├── Scenes/             Unity 기본 생성 — 손대지 않음
│
├── _Art/               아트팀 실 사용 리소스
│   ├── Shaders/         셰이더 (HLSL / Shader Graph)
│   ├── Materials/       머티리얼
│   ├── Models/          3D 모델 (FBX)
│   ├── Textures/        텍스처
│   ├── Prefabs/         건물·네온·VFX 등 실제 사용 프리팹
│   └── VFX/             VFX Graph
│
└── _ArtTest/            아트팀 테스트 전용 (프로그램팀 리소스와 분리)
    ├── Scenes/           테스트 씬
    ├── Scripts/          카메라·플레이어 등 테스트용 스크립트
    └── Prefabs/          테스트용 프리팹 (카메라 리그, 플레이어 캡슐 등)
```

**원칙** — 언더스코어(`_`)가 붙은 폴더만 직접 작업하는 영역입니다. 그 외 폴더는 Unity가 자동 생성한 기본 구조이므로 이동·삭제하지 않습니다. 추후 프로그래밍팀 합류 시 별도 폴더(`_Program` 등)로 추가되며, `_Art` / `_ArtTest`와 충돌하지 않도록 설계했습니다.

## 커밋 규칙

```
feat:     새 기능 추가 (셰이더, 시스템 등)
fix:      버그 수정
asset:    모델/텍스처/프리팹 등 에셋 추가·수정
shader:   셰이더 관련 작업
vfx:      VFX 관련 작업
docs:     문서 수정 (README, Wiki 등)
chore:    설정 파일, 빌드 관련 등 잡일
refactor: 코드 구조 개선 (기능 변화 없음)
```

예) `feat: BuildingGenerator 층수 자동 조립 구현`

## 브랜치 규칙

PART 단계별로 브랜치를 순차적으로 분기하며, 각 브랜치에서 직접 작업·커밋·푸시한다. 이전 브랜치의 내용을 이어받아 누적되는 방식이다.

```
part-1                  PART 1 작업 (최초 브랜치)
part-2 (part-1에서 분기)  PART 2 작업
part-3 (part-2에서 분기)  PART 3 작업
part-4 (part-3에서 분기)  PART 4 작업
```

**진행 순서**

1. `part-1` 브랜치 생성 → PART 1 작업 → 커밋 → Push
2. PART 1 완료 후 `part-1`에서 `part-2` 분기 → PART 2 작업 → 커밋 → Push
3. PART 2 완료 후 `part-2`에서 `part-3` 분기 → 반복
4. 마지막 PART까지 동일하게 진행

**GitHub Desktop에서 다음 PART 브랜치 분기하는 순서**

```
현재 PART 브랜치에서 작업 완료 + 커밋 + Push 확인
→ Current Branch 클릭 → New Branch
→ 이름 입력 (예: part-2) → Create Branch
→ Publish branch
→ 이 브랜치에서 다음 PART 작업 계속
```

**최종 main 머지 (전체 작업 완료 시 1회만)**

```
Current Branch → main으로 전환
→ Branch 메뉴 → Merge into current branch
→ 마지막 PART 브랜치 선택 (예: part-4)
→ Merge → Push origin
```

머지 이후 `main`은 최종 완성 상태가 되며, `part-1`~`part-4`는 각 단계별 스냅샷으로 보존된다.

## 문서

전체 제작 가이드와 셰이더 파라미터 설명은 [Wiki](../../wiki)에서 확인할 수 있습니다.

## 라이선스

개인 포트폴리오 및 학습 목적 템플릿입니다.
