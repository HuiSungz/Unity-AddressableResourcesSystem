# <img alt="ARM-Icon" src="https://imgur.com/zdGDYCN.png" width="26"/> Addressable Resources Management

![GitHub Release](https://img.shields.io/github/v/release/HuiSungz/Unity-AddressableResourcesSystem?display_name=release&style=for-the-badge&logo=github)
![Static Badge](https://img.shields.io/badge/UNITY-6000.0%2B-blue?style=for-the-badge&logo=unity)
![Static Badge](https://img.shields.io/badge/DEPENDENCIES-UniTask--Addressables-green?style=for-the-badge&logo=unity)

[![Static Badge](https://img.shields.io/badge/ARM-Wiki-orange?style=for-the-badge&logo=gitbook)](https://github.com/HuiSungz/Unity-AddressableResourcesSystem/wiki)
[![Static Badge](https://img.shields.io/badge/ARM-ENGLISH-orange?style=for-the-badge)](https://github.com/HuiSungz/Unity-AddressableResourcesSystem/blob/main/README.md)
![Static Badge](https://img.shields.io/badge/LICENSE-MIT-MIT?style=for-the-badge)

<p align="center">
  <img alt="ARM-MainTitle" src="https://imgur.com/0wBjlUx.png" width="400"/>
</p>

## 📌 소개

ARM(Addressable Resources Management)은 Unity의 Addressables 패키지 확장 유틸리티입니다.

기존 어드레서블의 단점인 핸들관리 요소를 래핑하여 최대한 자동으로 관리해주는 시스템입니다

또한 리소스 로딩, 캐싱, 메모리 관리 등을 단순화 하여 게임 개발의 성능과 편의성을 제공합니다.

## 📌 목차

- [설치 방법 Git](#설치-방법-git)
  - [설치 방법 OpenUPM](#설치-방법-openupm)
  - [의존성](#의존성)
- [📝 문서](#-문서)
- [주요 기능](#주요-기능)
- [컨택트](#컨택트)
- [라이센스](#라이센스)

## 설치 방법 Git

- Unity 2019.3 이상 버전부터는 `Package Manager`를 통한 Git URL로 설치가 가능합니다.

  1. Unity Editor에서 `Window -> Package Manager` 열기
  2. 좌측 상단에 `+` 버튼 클릭 후 `Add package from git URL...` 선택
  3. 아래 URL을 입력한 후 `Add` 버튼 클릭
 
```
https://https://github.com/HuiSungz/Unity-AddressableResourcesSystem.git
```

### 설치 방법 OpenUPM
- 이 패키지는 [OpenUPM](https://openupm.com) 패키지 레지스트리에서도 추가할 수 있습니다.
- 업데이트가 출시될 때마다 쉽게 받을 수 있어 이 방법이 권장되는 설치 방법입니다.
- [OpenUPM-CLI](https://github.com/openupm/openupm-cli)가 설치되어 있다면, 다음 명령어를 실행하세요.

```
openupm add com.hs-architect.addressable-resources-system
```

### 의존성

- ARM 프레임워크는 다음 패키지에 대한 의존성이 존재합니다.
  - Unity Addressables : `2.3.16`
  - UniTask : `Latest`
> 패키지 설치 시 자동으로 필요한 의존성을 감지해 설치합니다.<br>
> 혹여 설치가 잘못 되었을 시 재설치 또는 리임포트를 합니다.<br>
> 정상적인 설치가 완료되면 심볼에 `ARM_UNITASK`가 추가되고 클래스에 접근할 수 있습니다.

## 📝 문서

- 정규화된 문서 및 사용 방법은 `Wiki`페이지를 통해 손쉽게 확인할 수 있습니다. `이미지를 클릭하면 문서로 이동합니다.`

<p align="center">
  <a href="https://github.com/HuiSungz/Unity-AddressableResourcesSystem/wiki">
    <img alt="ARM-WikiDocs" src="https://github.com/user-attachments/assets/57268b11-d24b-423d-810f-94c49afd5470" width="400"/>
  </a>
</p>

## 주요 기능

- 비동기 로딩 시스템: UniTask 기반의 빠르고 효율적인 비동기 로딩
- 자동 캐싱 매커니즘: 한 번이라도 캐싱된 에셋은 중복 로딩을 방지
- 메모리 관리: AssetEntry를 통한 참조 카운팅 기반의 메모리 관리 시스템
  - 사용자는 미러링을 통해 사용하지 않는 엔트리를 소멸자 또는 이벤트에서 반환만 하면 됨
- 레퍼런스 트래킹: 에셋 사용 현황을 실시간으로 확인 가능한 디버깅 툴
- 타입 안정성: 제네릭 기반의 타입 API를 제공

## 컨택트

- 이슈 & 버그 리포트: [GitHub Issues](https://github.com/HuiSungz/Unity-AddressableResourcesSystem/issues)
- 이메일: gmltjd0910@gmail.com | huisung@actionfit.kr

> 버그를 발견하거나 기능 요청이 있을 경우 GitHub Issue에 오픈해주세요.<br>
> 일반적인 질문이나 도움이 필요하시면 이메일을 통해 연락 부탁드립니다.

[![](https://github.com/user-attachments/assets/d7dfc32d-bb84-452c-a7ee-c0b5ba3b487a)](https://github.com/users/huisungz/sponsorship)

## 라이센스

This library is under the MIT License.
