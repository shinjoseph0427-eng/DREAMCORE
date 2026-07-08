# Unity 재설치 절차

## 현재 문제

이번 문제의 원인은 `D:\DREAM CORE` 프로젝트가 아닙니다.

확인된 문제는 Unity 6.5 설치가 불완전하다는 점입니다. 특히 다음 파일이
Unity Editor 설치 폴더 안에 없어야 할 위치에서 누락되어 있었습니다.

```text
C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Data\Resources\Licensing\Client\Unity.Licensing.Client.exe
```

또한 Unity Hub 로그에서 설치 잠금/install lock 및 캐시 문제가 보였습니다.
그래서 먼저 Unity Editor와 Unity Hub 캐시 상태를 깨끗하게 정리한 뒤
새로 설치해야 합니다.

이 스크립트들은 Unity 설치 및 캐시 폴더만 정리합니다.
`D:\DREAM CORE` 프로젝트는 삭제하지 않습니다.

## 실행 순서

1. PowerShell을 관리자 권한으로 실행합니다.
2. 아래 폴더로 이동합니다.

```powershell
cd /d "D:\DREAM CORE\UnityInstallRepair"
```

PowerShell에서 `cd /d`가 동작하지 않으면 아래 명령을 사용합니다.

```powershell
Set-Location "D:\DREAM CORE\UnityInstallRepair"
```

3. 정리 스크립트를 실행합니다.

```powershell
powershell -ExecutionPolicy Bypass -File .\CLEAN_BROKEN_UNITY_INSTALL.ps1
```

4. 확인 문구가 나오면 정확히 아래처럼 입력합니다.

```text
CLEAN UNITY
```

5. 정리가 끝날 때까지 기다립니다.
6. Windows를 재부팅합니다.
7. Unity Hub를 관리자 권한으로 실행합니다.
8. 권장 설치 버전:

```text
Unity 6.3 LTS
```

9. 최소 모듈만 설치합니다.

- Unity Editor
- Microsoft Visual Studio Community
- Windows Build Support

10. 아직 설치하지 않을 선택 모듈:

- Web Build Support
- Android Build Support
- iOS Build Support
- Mac Build Support
- Linux Build Support
- Documentation

11. 설치 후 먼저 빈 Universal 3D 프로젝트를 새로 만들거나 열어 봅니다.
12. 빈 프로젝트가 정상적으로 열릴 때만 아래 프로젝트를 엽니다.

```text
D:\DREAM CORE
```

## 대체 방법

Unity 6.3 LTS가 Unity Hub에서 보이지 않는다면, 정리 후 Unity 6.5를 다시
설치해도 됩니다. 이 경우에도 최소 모듈만 설치하고, 먼저 빈 Universal 3D
프로젝트가 열리는지 확인한 뒤 `D:\DREAM CORE`를 열어야 합니다.
