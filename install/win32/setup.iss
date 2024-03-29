; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Working Memory Analyser"
#define MyAppVersion "1.0.2"
#define MyAppPublisher "Giorgio Fuggetta"
#define MyAppURL "https://github.com/gfuggetta-lab/WorkingMemoryAnalyser"
#define MyAppExeName "Working Memory Analyser.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{941CDF3B-CACF-42DB-A38E-12DF4D1BA89D}
; AppId={{AE344CFB-89D4-4562-8B21-3E23D52D31FA} 
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={userappdata}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=LICENSE
OutputBaseFilename=Working_Memory_Analyser_setup_{#MyAppVersion}
Compression=lzma
SolidCompression=yes
SourceDir=..\..
PrivilegesRequired=lowest

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]

Source: "scms_project_1.exe"; DestDir: "{app}"; DestName: "{#MyAppExeName}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "zlib1.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "libfreetype-6.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "SDL2_mixer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "inpout32.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "SDL2_ttf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "SDL2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "libpng16-16.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "SDL2_image.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "participant_consent.rtf"; DestDir: "{app}"; Flags: ignoreversion
Source: "Sounds\correct.wav"; DestDir: "{app}\Sounds"; Flags: ignoreversion
Source: "Sounds\incorrect.wav"; DestDir: "{app}\Sounds"; Flags: ignoreversion
Source: "Sounds\HIBEEP.wav"; DestDir: "{app}\Sounds"; Flags: ignoreversion

Source: "Experiment Library\Exp 01 Working Memory Capacity\*"; DestDir: "{userdocs}\Working Memory Analyser\Exp 01 Working Memory Capacity"; Flags: ignoreversion recursesubdirs
Source: "Experiment Library\Exp 02 Working Memory Load and Distractor processing\*"; DestDir: "{userdocs}\Working Memory Analyser\Exp 02 Working Memory Load and Distractor processing"; Flags: ignoreversion recursesubdirs

[Dirs]
Name: "{userdocs}\Working Memory Analyser\Exp 01 Working Memory Capacity\Output Data"; Permissions: users-modify; 
Name: "{userdocs}\Working Memory Analyser\Exp 02 Working Memory Load and Distractor processing\Output Data"; Permissions: users-modify; 

[Icons]
Name: "{userprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{userdesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

