# 강제 Unity 정리 보고서

## 문제 요약

Unity 6.5 설치가 깨진 상태입니다.

특히 아래 파일이 Unity Editor 설치 폴더 안에 없습니다.

```text
C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Data\Resources\Licensing\Client\Unity.Licensing.Client.exe
```

이 파일이 없어서 Unity 라이선스 초기화가 실패했고, Unity가 프로젝트를
열기도 전에 `SoftwareTermsWindow` 단계에서 충돌했습니다.

즉, 현재 1차 문제는 `D:\DREAM CORE`가 아니라 깨진 Unity Editor 설치입니다.

## 스크립트가 하는 일

`FORCE_CLEAN_UNITY_INSTALL.ps1`는 다음 작업을 수행합니다.

- 관리자 권한인지 확인합니다.
- 사용자가 `FORCE CLEAN UNITY`를 직접 입력해야 진행합니다.
- Unity Hub, Unity, Unity Crash Handler, Unity Licensing Client 프로세스를 종료합니다.
- `node.exe`는 명령줄에 `UnityHub`, `Unity Hub`, `unityhub`가 포함된 경우에만 종료합니다.
- 깨진 Unity 6.5 폴더의 소유권과 권한을 복구합니다.
- 아래 폴더만 강제로 제거합니다.

```text
C:\Program Files\Unity\Hub\Editor\6000.5.2f1
```

- Unity Hub 및 Unity 캐시 폴더는 삭제하지 않고 타임스탬프가 붙은 백업 이름으로 변경합니다.
- `D:\DREAM CORE`는 삭제하지 않습니다.

## UAC 승인 필요

`RUN_AS_ADMIN_CLEAN_UNITY.cmd`를 실행하면 Windows UAC 창이 뜹니다.
사용자는 이 UAC 요청을 한 번 승인해야 합니다.

승인 후 PowerShell 창에서 아래 문구를 정확히 입력해야 합니다.

```text
FORCE CLEAN UNITY
```

## Access Denied가 다시 발생하면

`D3D12Core.dll` 같은 파일에서 다시 Access Denied가 발생하면 다음 순서로 진행합니다.

1. Windows를 재부팅합니다.
2. Unity Hub를 열지 않습니다.
3. 재부팅 직후 바로 `RUN_AS_ADMIN_CLEAN_UNITY.cmd`를 다시 실행합니다.
4. UAC를 승인합니다.
5. `FORCE CLEAN UNITY`를 다시 입력합니다.

## 권장 재설치

정리가 성공하면 Windows를 재부팅한 뒤 Unity Hub를 관리자 권한으로 실행합니다.

권장 설치 버전:

```text
Unity 6.3 LTS
```

최소 모듈만 설치합니다.

- Unity Editor
- Microsoft Visual Studio Community
- Windows Build Support

설치 후 먼저 빈 Universal 3D 프로젝트가 열리는지 확인합니다.
빈 프로젝트가 정상적으로 열린 뒤에만 아래 프로젝트를 엽니다.

```text
D:\DREAM CORE
```
