<div align="center" valign="middle" style="page-break-after: always;">
<br>
    <h1>Behaviour Tree Editor</h1>
    <img src="Assets/BehaviourTree/Documentation/Images/thekiwicoder_profile_square_noborder_512_512.png" width = "64" />
    <br>
    A simple Behaviour Tree Editor and Runtime for Unity.
    <br>
    Created by TheKiwiCoder
</div>

## Overview
This repository is a Unity project which contains the Behaviour Tree Editor package.

The package and documentation is located in this repository here: https://github.com/thekiwicoder0/UnityBehaviourTreeEditor/tree/main/Assets/BehaviourTree

## Requirements

The minimum version currently supported is Unity 2021.3. This is due to the need for this feature:
https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceValue.html

## Installation

To install the editor in your project, you can either install directly from the Package Manager via a giturl, or by manually cloning this repository and creating a custom package. 

(Or just copy the files if you know what you're doing)

### Package Manager

The easiest way to install the package is directly from Package Manager in Unity via the `Add Package From git URL` using the following url:

```
https://github.com/thekiwicoder0/UnityBehaviourTreeEditor.git?path=/Assets/BehaviourTree/
```

This has the added benefit of being able to easily update the package from within your project.

All files will be installed to `Packages/Behaviour Tree Editor`

See https://docs.unity3d.com/Manual/upm-ui-giturl.html for Unity's official documentation on installing packages from Git using the Package Manager.

### Custom Package

Alternatively, you can clone this repository and create a .unitypackage file manually by running the `Export Package` command on the `Behaviour Tree` folder within Unity.

The exported package can then be installed into your project using the `Assets->Import Package->Custom Package` command.

All files will be installed to `Assets/BehaviourTree`

See https://docs.unity3d.com/Manual/AssetPackages.html for Unity's official documentation on creating custom packages.
