; Inno Setup 脚本 —— WPE x64 安装向导
; 由 GitHub Actions（windows-latest）在编译后调用 iscc 生成 Setup.exe
; 可通过命令行覆盖版本：iscc /DMyAppVersion=1.1.0 installer\wpe.iss

#ifndef MyAppVersion
  #define MyAppVersion "1.1.0"
#endif

#define MyAppName "Winsock Packet Editor x64"
#define MyAppExeName "WinsockPacketEditor.exe"
#define MyAppPublisher "WPE x64"
#define MyAppURL "https://github.com/GQBA7010/zhuruqi"
; 编译产物目录（相对于本 .iss 所在目录）
#define BuildDir "..\src\Wpe.App\bin\Release\net48"

[Setup]
AppId={{8F2C7A34-1E9B-4C6D-9A21-7B3E5C4D6A21}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
DefaultDirName={autopf}\WPE x64
DefaultGroupName=WPE x64
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=WPE_x64_Setup_{#MyAppVersion}
SetupIconFile=..\src\Wpe.App\Assets\wpe.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
; 钩子/注入需要管理员权限
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64
MinVersion=6.1sp1

[Languages]
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#BuildDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\WPE x64"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,WPE x64}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\WPE x64"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,WPE x64}"; Flags: nowait postinstall skipifsilent runascurrentuser
