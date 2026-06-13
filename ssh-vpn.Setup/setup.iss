; SSH VPN Installer Script
; Inno Setup

#define MyAppName "SSH VPN"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "SSH VPN Team"
#define MyAppExe "ssh-vpn.exe"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=ssh-vpn-setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "persian"; MessagesFile: "compiler:Languages\Persian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\ssh-vpn\bin\x64\Release\{#MyAppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ssh-vpn\bin\x64\Release\Renci.SshNet.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ssh-vpn\bin\x64\Release\ssh-vpn.exe.config"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExe}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, 1, 22)}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
  if not FileExists('..\ssh-vpn\bin\x64\Release\{#MyAppExe}') then
  begin
    MsgBox('Please build the project in Release x64 mode first.', mbError, MB_OK);
    Result := False;
  end
  else
    Result := True;
end;