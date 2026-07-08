# DREAM CORE 웹 프리뷰 연결 문제 해결

## 왜 ERR_CONNECTION_REFUSED가 발생했나요?

`http://localhost:8777`에서 `ERR_CONNECTION_REFUSED`가 보인다는 것은 브라우저가 8777 포트에 접속하려고 했지만, 그 주소에서 실행 중인 로컬 서버를 찾지 못했다는 뜻입니다.

즉, DREAM CORE 웹 프리뷰 파일이 없어졌다는 뜻이 아니라 보통 아래 상황입니다.

- `PLAY_WEB_PREVIEW.cmd`를 실행하지 않았습니다.
- 서버 창을 닫았습니다.
- Python 서버가 시작되기 전에 창이 종료되었습니다.
- Python이 설치되어 있지 않거나 PATH에서 찾을 수 없습니다.

## 실행 방법

아래 파일을 더블클릭하세요.

```text
D:\DREAM CORE\PLAY_WEB_PREVIEW.cmd
```

PowerShell로 실행하고 싶다면 아래 파일을 사용할 수 있습니다.

```text
D:\DREAM CORE\PLAY_WEB_PREVIEW_POWERSHELL.ps1
```

## 정확한 명령

직접 실행하려면 명령 프롬프트에서 아래처럼 입력합니다.

```bat
cd /d "D:\DREAM CORE\WebPreview"
python -m http.server 8777
```

## 정확한 URL

서버 창이 열린 상태에서 브라우저에 아래 주소를 입력하세요.

```text
http://localhost:8777
```

## 중요

프리뷰를 보는 동안 서버 창을 닫지 마세요.

`python -m http.server 8777`이 실행 중인 창이 닫히면 `http://localhost:8777` 접속도 바로 끊기고 다시 `ERR_CONNECTION_REFUSED`가 나타납니다.

이 웹 프리뷰는 로컬 브라우저용 미리보기입니다. 최종 Unity 버전은 Unity Play Mode에서 따로 테스트해야 합니다.
