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

```
Assets/
├── Shaders/      셰이더 (HLSL / Shader Graph)
├── Materials/     머티리얼
├── Prefabs/       프리팹 (건물 모듈, VFX 등)
├── Models/        3D 모델 (FBX)
├── Textures/       텍스처
├── Scenes/        씬 파일
├── Scripts/        C# 스크립트
└── VFX/           VFX Graph
```

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

작업은 `main`에서 계속 진행하고, PART 단계가 끝나는 시점마다 그 상태를 스냅샷으로 보존하기 위해 브랜치를 분리합니다. 머지는 하지 않습니다.

```
main      최종 작업 브랜치 (계속 갱신)
part-0    PART 0 완료 시점
part-1    PART 1 완료 시점
part-2    PART 2 완료 시점
part-3    PART 3 완료 시점
part-4    PART 4 완료 시점
```

GitHub Desktop에서 PART가 끝난 직후, 다음 PART 작업을 시작하기 전에 분기합니다.

```
Current Branch → New Branch → 이름 입력 (예: part-1) → Create Branch → Publish branch
```

이후 다시 main으로 돌아와 작업을 이어갑니다.

```
Current Branch → main 선택
```

각 단계별 진행 상황은 브랜치 목록에서 그대로 확인할 수 있습니다.

## 문서

전체 제작 가이드와 셰이더 파라미터 설명은 [Wiki](../../wiki)에서 확인할 수 있습니다.

## 라이선스

개인 포트폴리오 및 학습 목적 템플릿입니다.
