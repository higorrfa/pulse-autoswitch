#define AppVersion "0.3.0"
#ifndef PackageDir
  #error PackageDir must point to an extracted preview package
#endif

[Setup]
AppId={{C079355B-91CB-4D18-B467-A39E1DA9768C}
AppName=PULSE AutoSwitch
AppVersion={#AppVersion}
AppPublisher=PULSE AutoSwitch contributors
AppPublisherURL=https://github.com/higorrfa/pulse-autoswitch
DefaultDirName={localappdata}\Programs\PulseAutoSwitch
DefaultGroupName=PULSE AutoSwitch
DisableProgramGroupPage=yes
DisableWelcomePage=no
AppMutex=Local\PulseAutoSwitch054C0D5E
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
WizardStyle=modern
SetupIconFile=..\assets\pulse.ico
UninstallDisplayIcon={app}\PulseAutoSwitch.exe
OutputDir=..\dist
OutputBaseFilename=PulseAutoSwitch-Setup-{#AppVersion}-win-x64
Compression=lzma2
SolidCompression=yes
CloseApplications=yes
RestartApplications=no
LicenseFile=..\LICENSE

[Files]
Source: "{#PackageDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "config.example.xml,Start PULSE AutoSwitch.exe"
Source: "{#PackageDir}\config.example.xml"; DestDir: "{app}"; DestName: "config.xml"; Flags: onlyifdoesntexist uninsneveruninstall
Source: "..\scripts\SetupPrerequisites.ps1"; Flags: dontcopy

[Tasks]
Name: "desktopicon"; Description: "Create a Desktop shortcut"; Flags: checkedonce
Name: "startup"; Description: "Start PULSE AutoSwitch with Windows"; Flags: unchecked

[Icons]
Name: "{userprograms}\PULSE AutoSwitch"; Filename: "{app}\PulseAutoSwitch.exe"; WorkingDir: "{app}"
Name: "{userdesktop}\PULSE AutoSwitch"; Filename: "{app}\PulseAutoSwitch.exe"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{userstartup}\PULSE AutoSwitch"; Filename: "{app}\PulseAutoSwitch.exe"; Parameters: "--tray"; WorkingDir: "{app}"; Tasks: startup

[Run]
Filename: "{app}\PulseAutoSwitch.exe"; Description: "Open PULSE AutoSwitch and choose your audio outputs"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent runasoriginaluser

[Code]
var
  PrerequisitePage: TWizardPage;
  StatusLabel: TNewStaticText;
  CheckButton, ConfigureButton: TNewButton;
  PrerequisiteState: String;

procedure RefreshPrerequisites(Mode: String);
var
  ExitCode: Integer;
  Script, Output, Params: String;
begin
  CheckButton.Enabled := False;
  ConfigureButton.Enabled := False;
  WizardForm.NextButton.Enabled := False;
  StatusLabel.Caption := 'Checking requirements. Downloads and driver setup may take several minutes...';
  WizardForm.Update;
  Script := ExpandConstant('{tmp}\SetupPrerequisites.ps1');
  Output := ExpandConstant('{tmp}\prerequisites.ini');
  ExtractTemporaryFile('SetupPrerequisites.ps1');
  Params := '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + Script + '" -Mode ' + Mode + ' -OutputPath "' + Output + '"';
  DeleteFile(Output);
  if Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), Params, ExpandConstant('{tmp}'), SW_HIDE, ewWaitUntilTerminated, ExitCode) and FileExists(Output) then begin
    PrerequisiteState := GetIniString('Prerequisites', 'State', 'Error', Output);
    StatusLabel.Caption := GetIniString('Prerequisites', 'Message', 'Requirement check failed.', Output);
  end else begin
    PrerequisiteState := 'Error';
    StatusLabel.Caption := 'Could not check requirements. Ensure Windows PowerShell and device enumeration are allowed by your computer policy.';
  end;
  CheckButton.Enabled := True;
  ConfigureButton.Enabled := (PrerequisiteState = 'Runtime') or (PrerequisiteState = 'Driver');
  if PrerequisiteState = 'Runtime' then ConfigureButton.Caption := 'Install .NET Framework'
  else ConfigureButton.Caption := 'Open driver setup';
  WizardForm.NextButton.Enabled := PrerequisiteState = 'Ready';
end;

procedure CheckClick(Sender: TObject);
begin
  RefreshPrerequisites('Check');
end;

procedure ConfigureClick(Sender: TObject);
begin
  if PrerequisiteState = 'Runtime' then RefreshPrerequisites('Runtime')
  else if PrerequisiteState = 'Driver' then begin
    MsgBox('In Zadig, select USB ID 054C / 0D5E / 03 only. Choose WinUSB and click Replace Driver. Approve the administrator prompt, then close Zadig to continue.', mbInformation, MB_OK);
    RefreshPrerequisites('Driver');
  end;
end;

procedure InitializeWizard;
begin
  PrerequisitePage := CreateCustomPage(wpWelcome, 'Prepare your PULSE 3D', 'Connect the receiver. This assistant checks the runtime and control driver before installing.');
  StatusLabel := TNewStaticText.Create(PrerequisitePage);
  StatusLabel.Parent := PrerequisitePage.Surface;
  StatusLabel.SetBounds(0, 0, PrerequisitePage.SurfaceWidth, ScaleY(135));
  StatusLabel.AutoSize := False;
  StatusLabel.WordWrap := True;
  StatusLabel.Caption := 'Click Check requirements to begin. Internet access is needed only to download missing components from Microsoft or the official Zadig release. Runtime and driver installation require administrator approval.';
  CheckButton := TNewButton.Create(PrerequisitePage);
  CheckButton.Parent := PrerequisitePage.Surface;
  CheckButton.SetBounds(0, ScaleY(155), ScaleX(145), ScaleY(28));
  CheckButton.Caption := 'Check requirements';
  CheckButton.OnClick := @CheckClick;
  ConfigureButton := TNewButton.Create(PrerequisitePage);
  ConfigureButton.Parent := PrerequisitePage.Surface;
  ConfigureButton.SetBounds(ScaleX(160), ScaleY(155), ScaleX(170), ScaleY(28));
  ConfigureButton.Caption := 'Open driver setup';
  ConfigureButton.Enabled := False;
  ConfigureButton.OnClick := @ConfigureClick;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = PrerequisitePage.ID then RefreshPrerequisites('Check');
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = PrerequisitePage.ID then begin
    RefreshPrerequisites('Check');
    Result := PrerequisiteState = 'Ready';
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  RefreshPrerequisites('Check');
  if PrerequisiteState <> 'Ready' then Result := StatusLabel.Caption
  else Result := '';
end;
