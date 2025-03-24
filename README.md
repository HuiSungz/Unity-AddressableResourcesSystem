# <img alt="ARM-Icon" src="https://imgur.com/zdGDYCN.png" width="26"/> Addressable Resources Management

![GitHub Release](https://img.shields.io/github/v/release/HuiSungz/Unity-AddressableResourcesSystem?display_name=release&style=for-the-badge&logo=github)
![Static Badge](https://img.shields.io/badge/UNITY-2022.3%2B-blue?style=for-the-badge&logo=unity)
![Static Badge](https://img.shields.io/badge/DEPENDENCIES-UniTask--Addressables-green?style=for-the-badge&logo=unity)

[![Static Badge](https://img.shields.io/badge/ARM-Wiki-blue?style=for-the-badge)](https://github.com/HuiSungz/Unity-AddressableResourcesSystem/wiki)
![Static Badge](https://img.shields.io/badge/ARM-ENGLISH-blue?style=for-the-badge)
![Static Badge](https://img.shields.io/badge/LICENSE-MIT-MIT?style=for-the-badge)

<p align="center">
  <img alt="ARM-MainTitle" src="https://imgur.com/0wBjlUx.png" width="400"/>
</p>

## 📌 소개
ARM(Addressable Resources Management)은 Unity의 Addressables 패키지 확장 유틸리티입니다.

기존 어드레서블의 단점인 핸들관리 요소를 래핑하여 최대한 자동으로 관리해주는 시스템입니다

또한 리소스 로딩, 캐싱, 메모리 관리 등을 단순화 하여 게임 개발의 성능과 편의성을 제공합니다.

## 📌 목차
- [설치 방법](#설치-방법)
  - [의존성](#의존성)
- [주요 기능](#주요-기능)
- [문서](#문서)
- [라이센스](#라이센스)

## 설치 방법
- Unity 2019.3 이상 버전부터는 `Package Manager`를 통한 Git URL로 설치가 가능합니다.

  1. Unity Editor에서 `Window -> Package Manager` 열기
  2. 좌측 상단에 `+` 버튼 클릭 후 `Add package from git URL...` 선택
  3. 아래 URL을 입력한 후 `Add` 버튼 클릭
 
```
https://https://github.com/HuiSungz/Unity-AddressableResourcesSystem.git
```

### 의존성
- ARM 프레임워크는 다음 패키지에 대한 의존성이 존재합니다.
  - Unity Addressables : `2.3.16`
  - UniTask : `Latest`
> 패키지 설치 시 자동으로 필요한 의존성을 감지해 설치합니다.<br>
> 혹여 설치가 잘못 되었을 시 재설치 또는 리임포트를 합니다.<br>
> 정상적인 설치가 완료되면 심볼에 `ARM_UNITASK`가 추가되고 클래스에 접근할 수 있습니다.

## 문서
- 정규화된 문서 및 사용 방법은 `Wiki`페이지를 확인하면 손쉽게 확인할 수 있습니다.
- [ARM-Documentation-Wiki](https://github.com/HuiSungz/Unity-AddressableResourcesSystem/wiki)

## 주요 기능
- 비동기 로딩 시스템: UniTask 기반의 빠르고 효율적인 비동기 로딩
- 자동 캐싱 매커니즘: 한 번이라도 캐싱된 에셋은 중복 로딩을 방지
- 메모리 관리: AssetEntry를 통한 참조 카운팅 기반의 메모리 관리 시스템
  - 사용자는 미러링을 통해 사용하지 않는 엔트리를 소멸자 또는 이벤트에서 반환만 하면 됨
- 레퍼런스 트래킹: 에셋 사용 현황을 실시간으로 확인 가능한 디버깅 툴
- 타입 안정성: 제네릭 기반의 타입 API를 제공

## 라이센스
This library is under the MIT License.
