unit edidMonitorUtils;

{$Mode objfpc}{$H+}

interface

uses
  Types,
  {$IfDef darwin}MacOSAll,{$EndIf}
  {$IfDef mswindows}Windows, winEdidUtils, edidTypes,{$EndIf}
  {$IfDef LINUX}Process, FileUtil,{$EndIf}
  Classes, SysUtils;

type
  TMonitor = class
  public
    Name       : String;
    Resolution : TSize;
    PhysSizeMm : TSize; // millimeters
    Frequency  : Double;
    Bounds     : TRect;
  end;

function GetSysMonitors(list: TList): Boolean;

implementation

{$ifdef mswindows}
function CallbackEnum(monitor: HMONITOR;
  Arg2: HDC;  Arg3: LPRECT; Arg4: LPARAM): BOOL; stdcall;
var
  dst     : TList;
  mi      : MONITORINFOEXA;
  wm      : TMonitor;
  monname : string;
  ed      : TEDIDRec;
  devmod  : TDEVMODEA;
begin
  dst := Tlist(Arg4);

  FillChar(mi, sizeof(mi), 0);
  mi.info.cbSize := sizeof(mi);
  if GetMonitorInfoA(monitor, @mi) then
    monname := mi.szDevice
  else
    monname := '';

  if (monname <>'') and (GetEdidForDevicePath( monname, ed)) then begin
    wm := TMonitor.Create;
    wm.bounds := mi.info.rcMonitor;
    EdidGetPhysSizeMm(ed, wm.PhysSizeMm.cx, wm.PhysSizeMm.cy);

    FillChar(devmod, sizeof(devmod), 0);
    devmod.dmSize := sizeof(devmod);
    EnumDisplaySettingsA(PChar(monname), ENUM_CURRENT_SETTINGS, devmod);
    wm.Name := EdidGetDisplayName(ed);

    wm.Resolution.cx := devmod.dmPelsWidth;
    wm.Resolution.cy := devmod.dmPelsHeight;
    wm.Frequency := devmod.dmDisplayFrequency;
    dst.Add(wm);
  end;

  Result := True;
end;

function WinEnumMonitors(list: TList): Boolean;
begin
  Result := EnumDisplayMonitors(0, Nil, @CallbackEnum, LParam(list));
end;
{$EndIf}

{$IfDef darwin}
function CocoaEnumMonitors(list: TList): Boolean;
var
  wm  : TMonitor;
  dsp : array of CGDirectDisplayID;
  i   : Integer;
  cnt : UInt32;
  sz  : CGSize;
  r   : CGRect;
  md  : CGDisplayModeRef;
begin
  Result := Assigned(list);
  if not Result then Exit;

  SetLength(dsp, 256);
  cnt := 0;
  CGGetActiveDisplayList(length(dsp), @dsp[0], cnt);
  for i:= 0 to Integer(cnt)-1 do begin
    wm := TMonitor.Create;
    md := CGDisplayCopyDisplayMode(dsp[i]);
    try
      sz := CGDisplayScreenSize(dsp[i]);
      wm.PhysSizeMm.cx := Round(sz.width);
      wm.PhysSizeMm.cy := Round(sz.height);
      r := CGDisplayBounds(dsp[i]);
      wm.Bounds := Bounds( Round(r.origin.x), Round(r.origin.y),
        Round(r.size.width), Round(r.size.height));
      wm.Resolution.cx := Round(r.size.width);
      wm.Resolution.cy := Round(r.size.height);
      wm.Frequency := CGDisplayModeGetRefreshRate(md);
      if wm.Frequency = 0 then
        wm.Frequency := 60;
    finally
      CGDisplayModeRelease(md);
    end;
    list.Add(wm);
    Inc(cnt);
  end;
  Result := (cnt > 0)
end;
{$EndIf}

{$IfDef Linux}
function LinuxEnumMonitors(aList: TList): Boolean;
const
  BufSize = 2048;
var
  aProcess     : TProcess;
  outputStream : TStream;
  buffer       : array[1..BufSize] of Byte;
  sl           : TStringList;
  mon          : TMonitor = Nil;
  a, b, hRes, vRes, p, bytesRead: Integer;
  s, nme, sHz: String;
  hz: Double;

  function ParsedStringsOK: Boolean;
  var
    i: Integer;
  begin
    Result := False;
    if sl.Count < 3 then
      Exit;
    for i := 0 to sl.Count-1 do
      begin
        s := Trim(sl[i]);
        p := Pos('connected', s);
        if (p > 0) and (Pos('disconnected', s) = 0) then
          begin
            nme := Copy(s, 1, Pred(p));
            p := Pos('primary', s);
            if p = 0 then
              Exit;
            Delete(s, 1, p + 7);
            p := Pos('+', s);
            if p = 0 then
              Exit;
            sHz := Copy(s, 1, Pred(p));
            p := Pos('x', shz);
            if p = 0 then
              Exit;
            hRes := Copy(shz, 1, Pred(p)).ToInteger;
            vRes := Copy(shz, Succ(p), Length(s)).ToInteger;
            p := Pos(') ', s);
            if p = 0 then
              Exit;
            Delete(s, 1, Succ(p));
            s := Trim(s);
            p := Pos('mm', s);
            if p = 0 then
              Exit;
            if not TryStrToInt(Copy(s, 1, Pred(p)), a) then
              Exit;
            p := Pos('x ', s);
            if p = 0 then
              Exit;
            Delete(s, 1, Succ(p));
            p := Pos('mm', s);
            if p = 0 then
              Exit;
            if not TryStrToInt(Copy(s, 1, Pred(p)), b) then
              Exit;
            if ((hRes > vRes) and (a < b)) or ((hRes < vRes) and (a > b)) then
              begin
                p := a;
                a := b;
                b := p;
              end;
            p := i;
            Break;
          end;
      end;
    i := Succ(p);
    if (i > sl.Count-1) then
      Exit;
    p := Pos('*', sl[i]);
    if p = 0 then
      Exit;
    bytesRead := p;
    repeat
      Dec(p);
    until (p = 1) or not (sl[i][p] in ['0'..'9','.']);
    if p > 1 then
      if not TryStrToFloat(Copy(sl[i], Succ(p), Pred(bytesRead - p)), hz) then
        Exit;
    mon := TMonitor.Create;
    with mon do begin
      Name       := nme;
      Resolution := TSize.Create(hRes, vres);
      PhysSizeMm := TSize.Create(a, b); // millimetres
      Frequency  := hz;
      Bounds     := TRect.Create(0, 0, hRes, vres);
    end;
    Result := True;
  end;

begin
  sl := TStringList.Create;
  try
    outputStream := TMemoryStream.Create;
    try
      aProcess := TProcess.Create(Nil);
      try
        aProcess.Executable := FindDefaultExecutablePath('xrandr');
        aProcess.Parameters.Add('--query');
        aProcess.Options := [poUsePipes];
        try
          aProcess.Execute;
        except on E: EProcess do
          Exit(False);
        end;
        repeat
          bytesRead := aProcess.Output.Read(buffer{%H-}, BufSize);
          outputStream.Write(buffer, bytesRead);
        until bytesRead = 0;
      finally
        aProcess.Free;
      end;
      outputStream.Position := 0; // make sure all data is copied from the very start
      sl.LoadFromStream(outputStream);
      Result := ParsedStringsOK;
      if Result then
        aList.Add(mon);
    finally
      outputStream.Free;
    end;
  finally
    sl.Free;
  end;
end;
{$EndIf}

function GetSysMonitors(list: TList): Boolean;
begin
  Result := False;
  if not Assigned(List) then
    Exit;
{$IfDef MSWindows}
  Result := WinEnumMonitors(list);
{$Else}
  {$IfDef darwin}
  Result := CocoaEnumMonitors(list);
  {$Else}
    {$IfDef Linux}
    Result := LinuxEnumMonitors(list);
    {$EndIf}
  {$EndIf}
{$EndIf}
end;

end.
