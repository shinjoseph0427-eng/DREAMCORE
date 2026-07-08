# DREAM CORE 웹 프리뷰 오디오 안내

## 소스 오디오 위치

`Ambience.mp3`는 아래 위치에서 찾았습니다.

```text
D:\DREAM CORE\Ambience.mp3
```

Unity 프로젝트 안에도 같은 파일이 있습니다.

```text
D:\DREAM CORE\Assets\_Project\Audio\Ambience\Ambience.mp3
```

이번 WebPreview에는 우선순위가 더 높은 루트 파일을 사용했습니다.

## WebPreview 복사 위치

브라우저 프리뷰에서 로드할 수 있도록 아래 위치로 복사했습니다.

```text
D:\DREAM CORE\WebPreview\assets\audio\Ambience.mp3
```

WebPreview는 아래 상대 경로로 음악을 불러옵니다.

```text
assets/audio/Ambience.mp3
```

## 실행 방법

아래 파일을 실행합니다.

```text
D:\DREAM CORE\PLAY_WEB_PREVIEW.cmd
```

그 다음 브라우저에서 아래 주소를 엽니다.

```text
http://localhost:8777
```

## 조작법

```text
Click = 입장 / 오디오 시작
M = 음악 음소거 / 음소거 해제
R = 음악 처음부터 다시 재생
WASD = 이동
Mouse = 시점 조작
ESC = 마우스 잠금 해제
```

## 브라우저 오디오 주의사항

브라우저는 자동 재생을 막기 때문에 음악은 페이지가 열리자마자 재생되지 않습니다.

화면을 클릭해서 포인터 잠금으로 들어갈 때 `Ambience.mp3`가 시작되며, 볼륨 `0`에서 `0.45`까지 약 4초 동안 천천히 페이드 인됩니다.

프리뷰를 보는 동안 음악이 들리지 않으면 먼저 화면을 클릭해서 입장했는지 확인하세요.

음악의 첫 부분을 다시 듣고 싶으면 `R` 키를 누르세요. 음악이 처음으로 돌아가고 다시 부드럽게 페이드 인됩니다.
