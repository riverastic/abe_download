Inno Setup
=====================

Build installer automatically for Arent projects

### Usage

Run `Build_*.bat` file in Windows Eg:

```
.\Build_sample.bat
```

### Config

Configuration is saved in `json` file

Example config: `./conf/config_sample.json`

```json
{
  "Versions": [
    {
      "Name": "<Version number>",
      "SourcePath": "<path to source folder (Reference from 'Module source folder'>"
    },
    {
      "Name": "2020",
      "SourcePath": "Debug\\2020\\net472\\"
    },
    ...
  ],
  "AppInfo": {
    "AppName": "ArentReinforcement",
    "AppId": "160B212D-0935-48A6-BE88-28A9FCFDB9EA",
    "AppVersion": "0.1",
    "IsIncludedLicense": "<Is there a license attached ? true/false >",
    "IsIncludedManual": "<Is there a manual attached ? true/false >",
    "IsIncludedAppExe": "<Is there a App exe attached ? true/false >",
    "AppExeName": "ArentDLLVersion.exe",
    "AppLicenseName": "ライセンス",
    "AppLicenseTxt": "license.txt",
    "AppManualName": "マニュアル",
    "AppManualPdf": "Manual.pdf",
    "CopyRight": "commit245ec1a",
    "AppAssocName": "<default value: AppName + ' File'>",
    "AppAssocExt": "<default value: '.myp'>",
    "AppAssocKey": "<default value: AppAssocName + AppAssocExt>",
    "AppPublisher": "Arent Inc.",
    "AppUrl": "https://arent3d.com",
    "OutputFilename": "ArentReinforcementDLLInstaller",
    "CustomConfig": {
      "customKey": "Custom value",
      ...<put
      any
      key-value
      you
      want
      to
      use
      in
      template>
    }
  },
  "Modules": [
    {
      "Name": "Arent3d.Architecture.Rc.App",
      "SrcPath": "<path to Module source folder>",
      "DestPath": "<Module files is copied to this folder (References from C:\ProgramData\Autodesk\Revit\Addins\{version})>",
      "CustomConfig": {
      }
    }
  ]
}
```

### Customize inno setup script template

Project use [Fluid.core](https://github.com/sebastienros/fluid#using-fluid-in-your-project) template engine. (
Open-source)

- To change template: Edit file `./conf/templateScript.iss`
- To add data context to use in template: `./GenerateInnoSetupScript/GenScript.cs` method `MakeTemplateContext`

### How to use PreHandleApp/GenerateInnoSetupScript/GenerateInnoSetupScript.exe

####  * Full command:

```
GenerateInnoSetupScript.exe -cfg <path-to-config-file> -tmp <path-to-template-file> -o <path-to-script-file-output>
```

####  * Arguments:

- `-cfg`: Path to config file. Default: `./conf/config.json`
- `-tmp`: Path to template file. Default: `./conf/templateScript.iss`
- `-o`: Path to script file output. Default: `./`

###  Sign with certification:

Please config these field as instruction below:
- `SignTool`: The path of SignTool.exe in your computer.
- `CertPath`: The path of digital certificate
- `CertAuthen`: URL of Time Stamp Server ("Time stamping allows Authenticode signatures to be verifiable even after the certificates used for signature have expired.")
	#### For reference please see: https://docs.microsoft.com/en-us/windows/win32/seccrypto/time-stamping-authenticode-signatures
	#### Example Time Stamp Server:
		http://timestamp.sectigo.com
		http://timestamp.comodoca.com/authenticode
- `CertPath`: Password of certificate

###  Build ISO file with ImgBurn:

Please install `ImgBurn` program for building the ISO file
  #### Download link: https://www.imgburn.com/index.php?act=download
  #### ImgBurn Setting to skip UI while building the ISO file:
    Tool => Setting => Build => Page 3 & 4 => Set all `Prompts` with `Answer 'Yes'`
    
Please config the field of ImgBurn as instruction below:
- `ImgBurn`: The path of ImgTool.exe in your computer.
