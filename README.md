# <img alt="ARM-Icon" src="https://imgur.com/zdGDYCN.png" width="26"/> Addressable Resources Management

![GitHub Release](https://img.shields.io/github/v/release/HuiSungz/Unity-AddressableResourcesSystem?display_name=release&style=for-the-badge&logo=github)
![Static Badge](https://img.shields.io/badge/UNITY-2022.3%2B-blue?style=for-the-badge&logo=unity)
![Static Badge](https://img.shields.io/badge/DEPENDENCIES-UniTask--Addressables-green?style=for-the-badge&logo=unity)

[![Static Badge](https://img.shields.io/badge/ARM-Wiki-orange?style=for-the-badge&logo=gitbook)](https://github.com/HuiSungz/Unity-AddressableResourcesSystem/wiki)
[![Static Badge](https://img.shields.io/badge/ARM-한국어-orange?style=for-the-badge)](https://github.com/HuiSungz/Unity-AddressableResourcesSystem/blob/main/README-kr.md)
![Static Badge](https://img.shields.io/badge/LICENSE-MIT-MIT?style=for-the-badge)

<p align="center">
  <img alt="ARM-MainTitle" src="https://imgur.com/0wBjlUx.png" width="400"/>
</p>

## 📌 Introduction

ARM(Addressable Resources Management) is an extension utility for Unity's Addressables package.

It's a system that wraps the handle management elements, which are a drawback of the existing addressables, to automate management as much as possible.

It also simplifies resource loading, caching, and memory management to provide performance and convenience for game development.

## 📌 Table of contents

- [Install via Git](#install-via-git)
  - [Dependencies](#dependencies)
- [📝 Documentation](#-documentation)
- - [Key Features](#key-features)
- [Contact](#contact)
- [License](#license)

## Install via Git

- For Unity 2019.3 and above, you can install via Git URL using the `Package Manager`

  1. Open `Window -> Package Manager` in Unity Editor
  2. Click the `+` button in the top left and select `Add package from git URL...`
  3. Enter the URL below and click the `Add` button
 
```
https://https://github.com/HuiSungz/Unity-AddressableResourcesSystem.git
```

### Dependencies

- The ARM framework has dependencies on the following packages:
  - Unity Addressables : `2.3.16`
  - UniTask : `Latest`
> The package automatically detects and installs the necessary dependencies during installation.<br>
> If installation fails, reinstall or reimport the package.<br>
> When installation is completed successfully, <br>
> the symbol ARM_UNITASK will be added and you can access the classes.

## 📝 Documentation

- Standardized documentation and usage instructions can be easily found through the `Wiki` page.
- `Click on the image to go to the document.`

<p align="center">
  <a href="https://github.com/HuiSungz/Unity-AddressableResourcesSystem/wiki">
    <img alt="ARM-WikiDocs" src="https://github.com/user-attachments/assets/57268b11-d24b-423d-810f-94c49afd5470" width="400"/>
  </a>
</p>

## Key Features

- Asynchronous Loading System: Fast and efficient asynchronous loading based on UniTask
- Automatic Caching Mechanism: Prevents duplicate loading of assets that have been cached at least once
- Memory Management: Reference counting-based memory management system through AssetEntry
  - Users only need to return unused entries through mirroring in destructors or events
- Reference Tracking: Debugging tool for real-time monitoring of asset usage
- Type Safety: Provides type-safe APIs based on generics

## Contact

- Issues & Bug Report: [GitHub Issues](https://github.com/HuiSungz/Unity-AddressableResourcesSystem/issues)
- Email: gmltjd0910@gmail.com | huisung@actionfit.kr

> If you find a bug or have a feature request, please open an issue on GitHub.<br>
> For general questions or if you need help, please contact via email.

[![](https://github.com/user-attachments/assets/d7dfc32d-bb84-452c-a7ee-c0b5ba3b487a)](https://github.com/users/huisungz/sponsorship)

## License

This library is under the MIT License.
