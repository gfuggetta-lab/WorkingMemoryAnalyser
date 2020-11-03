; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Working Memory Analyzer"
#define MyAppVersion "1.0"
#define MyAppPublisher "Giorgio Fuggetta"
#define MyAppURL "https://github.com/gfuggetta-lab/WorkingMemoryAnalyser"
#define MyAppExeName "scms_project_1.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{941CDF3B-CACF-42DB-A38E-12DF4D1BA89D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=LICENSE
OutputBaseFilename=Working_Memory_Analyzer_setup_{#MyAppVersion}
Compression=lzma
SolidCompression=yes
SourceDir=..\..

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]

Source: "scms_project_1.exe"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "zlib1.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "libfreetype-6.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "SDL2_mixer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "inpout32.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "SDL2_ttf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "SDL2.dll"; DestDir: "{app}"; Flags: ignoreversion

Source: "Experiment Library\Working Memory Load Experiment 1\*"; DestDir: "{userdocs}\Working Memory Analyzer\Working Memory Load Experiment 1"; Flags: ignoreversion recursesubdirs
Source: "Experiment Library\Working Memory Load Experiment 2\*"; DestDir: "{userdocs}\Working Memory Analyzer\Working Memory Load Experiment 2"; Flags: ignoreversion recursesubdirs

[Dirs]
Name: "{userdocs}\Working Memory Analyzer\Working Memory Load Experiment 1\Output Data"; Permissions: users-modify; 
Name: "{userdocs}\Working Memory Analyzer\Working Memory Load Experiment 2\Output Data"; Permissions: users-modify; 

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

