# Parry Paws

적의 공격을 받아치는 **패링**이 중심인 캐주얼 2D 액션 게임입니다.

- 2025년 12월, 2인 개발
- 플레이 영상: [포트폴리오에서 보기](https://rkdwlals.github.io/#porta)
- Unity 6000.2.14f1 · C#

## 자료

- `docs/게임개발실습 발표자료_강지민,박덕환.pdf`: 발표 자료
- `docs/Porta Demo.mp4`: 데모 영상 (Porta는 개발 당시 이름)
- 실행 파일은 Releases에 올립니다.

## 맡은 역할

| 담당 | 내용 |
|---|---|
| 강지민 | 게임 시스템(패링 시스템, 패링 이펙트, 애니메이션, 캐릭터 구동, 캐릭터 HP, 데미지)과 사운드 |
| 팀원 | UI, 시작 화면, 스테이지 화면, 맵 |

### 사운드
- 플레이어 사운드를 관리하는 `PlayerSoundManager`를 작성했습니다.
- 걷기, 구르기, 점프, 공격 4종, 찌르기, 패링, 피격, 사망 등 동작 15가지의 소리 칸을 나눴습니다.
- 걷기 소리는 별도 AudioSource로 분리해 움직이는 동안에만 반복 재생되게 했습니다.

## 폴더 구성

```
Assets/
  Scripts/          플레이어, 적(닌자·까마귀 궁수), 투사체, 카메라 등
  Sound/plus/       PlayerSoundManager, 골 지점, 스테이지 클리어 연출, 레벨 선택 씬
  UI or scene/      UI·시작 화면
  animations/       캐릭터 애니메이션
  Prefabs/, Prefabs 1/, Scenes/, Game1.unity
```

## 포함하지 않은 파일

라이선스 때문에 아래는 저장소에 넣지 않았습니다. 프로젝트를 열면 그림과 소리가 빠져 보이니, 완성된 모습은 플레이 영상을 참고해 주세요.

- 캐릭터 스프라이트(Red Panda, cobra, crow, tanuki), Cainos, 2D Dimensions, RPG Button 7
- 맵 배경·타일(Environment, Tiles): 팀원이 외부 에셋으로 구성
- 모든 소리 파일: 외부 소스를 피치·EQ로 가공해 사용했고, BGM은 Suno AI로 생성

## Claude가 수정한 부분

아래는 2026년 10월 포트폴리오 영상 녹화 전에 Claude(AI)가 고친 부분입니다.

- `CrowArcherController.cs`, `NinjaEnemyController.cs`: 적이 죽은 뒤 콜라이더를 끄고, 까마귀 시체가 사라지게 함
- `PlayerSoundManager.cs`: 소리가 찢어지지 않도록 클립별 볼륨 조정

README와 `.gitignore` 정리도 Claude의 도움을 받았습니다.
