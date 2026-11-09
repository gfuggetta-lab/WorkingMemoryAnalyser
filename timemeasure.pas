unit timemeasure;
{$mode delphi}
{$WARN 5066 off : Symbol "$1" is deprecated: "$2"} // for GetTickCount
interface

uses
  SysUtils;

type
  TTimeTarget = record
    TrgFrames : LongWord; // based on the refresh rate
    TrgTicks  : LongWord; // initTick + duration

    InitTick  : LongWord; // the tick, when StartTarget was called
    Duration  : LongWord; // the duration in ms
    RefreshRate : LongWord; // refresh rate used to calculate target frames
  end;

  TTimeCheck = (tcBoth, tcTick, tcFrames);

procedure StartTarget(durationMs: integer; refreshRate: integer; out tm: TTimeTarget); overload;
function IsTimeOver(check: TTimeCheck; frames: integer; const tm: TTimeTarget): boolean; overload;
function IsTimeOverAny(frames: integer; const tm: TTimeTarget): boolean;
function IsTimeOverTick(const tm: TTimeTarget): boolean;
function IsTimeOverFrames(frames: integer; const tm: TTimeTarget): boolean;

// frame-only check
function IsTimePassOfs(offsetMs: integer; const tm: TTimeTarget): Boolean;
function TimeSinceStart(const tm: TTimeTarget): LongWord;
function FrameSinceStart(const tm: TTimeTarget): LongWord;

function IsSameFrameOrPassOfs(
    checkType: TTimeCheck;
    curFrame, TargetFrame: integer;
    offsetMs: integer; const tm: TTimeTarget):Boolean; overload;
function IsFrameGEOrPassOfs(
    checkType: TTimeCheck;
    curFrame, TargetFrame: integer;
    offsetMs: integer; const tm: TTimeTarget):Boolean; overload;

function IsFrameOffsetIn(
    checkType: TTimeCheck;
    curFrame, MinFrame, MaxFrame: integer;
    MinOfs, MaxOfs: integer;
    const tm: TTimeTarget):Boolean; overload;

var
  DEF_CHECK : TTimecheck = tcBoth;
  DEF_REFRESHRATE : Integer = 60;

procedure StartTarget(durationMs: integer; out tm: TTimeTarget); overload;
function IsTimeOver(frames: integer; const tm: TTimeTarget): boolean; inline; overload;
function IsSameFrameOrPassOfs(
    curFrame, TargetFrame: integer;
    offsetMs: integer; const tm: TTimeTarget):Boolean; overload;

function IsFrameGEOrPassOfs(
    curFrame, TargetFrame: integer;
    offsetMs: integer; const tm: TTimeTarget):Boolean; overload;

function IsFrameOffsetIn(
    curFrame, MinFrame, MaxFrame: integer;
    MinOfs, MaxOfs: integer;
    const tm: TTimeTarget):Boolean; overload;


implementation

function GetTicks: LongWord; inline;
begin
  Result := GetTickCount;
end;

procedure StartTarget(durationMs: integer; refreshRate: integer; out tm: TTimeTarget);
begin
  if (durationMs < 0) then
    durationMs := 0;
  if (refreshRate <= 0) then
    refreshRate := 60;
  tm.InitTick := GetTicks;
  tm.Duration := LongWord(durationMs);
  tm.TrgTicks := tm.InitTick + tm.Duration;
  tm.TrgFrames := round (durationMs / 1000 / (1/refreshRate));
  tm.RefreshRate := refreshRate;
end;

function IsTimeOverAny(frames: integer; const tm: TTimeTarget): boolean;
var
  t : LongWord;
begin
  if (frames < 0) then begin
    Result := true;
  end else begin
    result := LongWord(frames) >= tm.TrgFrames;
    if not result then begin
      t := GetTicks;
      Result := t >= tm.TrgTicks;
    end;
  end;
end;

function IsTimeOver(check: TTimeCheck; frames: integer; const tm: TTimeTarget): boolean;
begin
  case check of
    tcFrames:
      result := IsTimeOverFrames(frames, tm);
    tcTick:
      result := IsTimeOverTick(tm);
    else
      result := IsTimeOverAny(frames, tm);
  end
end;

function IsTimeOverTick(const tm: TTimeTarget): boolean;
var
  t : LongWord;
begin
  t := GetTicks;
  Result := t >= tm.TrgTicks;
end;

function IsTimeOverFrames(frames: integer; const tm: TTimeTarget): boolean;
begin
  if (frames < 0) then
    result := true
  else
    result := LongWord(frames) >= tm.TrgFrames;
end;

procedure StartTarget(durationMs: integer; out tm: TTimeTarget);
begin
  StartTarget(durationMs, DEF_REFRESHRATE, tm);
end;

function IsTimeOver(frames: integer; const tm: TTimeTarget): boolean; overload;
begin
  Result := IsTimeOver(DEF_CHECK, frames, tm);
end;

function IsTimePassOfs(offsetMs: integer; const tm: TTimeTarget): Boolean;
var
  t : LongWord;
begin
  if (offsetMs < 0) then
    offsetMs :=0;
  t := TimeSinceStart(tm);
  Result := t > LongWord(offsetMs);
end;

function TimeSinceStart(const tm: TTimeTarget): LongWord;
begin
  Result := GetTicks - tm.InitTick;
end;

function FrameSinceStart(const tm: TTimeTarget): LongWord;
var
  ms : LongWord;
begin
  ms := TimeSinceStart(tm);
  Result := round (ms / 1000 / (1/tm.RefreshRate));
end;

function IsSameFrameOrPassOfs(
    checkType: TTimeCheck;
    curFrame, TargetFrame: integer;
    offsetMs: integer; const tm: TTimeTarget):Boolean; overload;
begin
  case checkType of
    tcTick:
      Result := IsTimePassOfs(offsetMs, tm);
    tcFrames:
      Result := curFrame = targetFrame;
    else
      Result := (curFrame = targetFrame) or IsTimePassOfs(offsetMs, tm);
  end;
end;

function IsSameFrameOrPassOfs(
    curFrame, TargetFrame: integer;
    offsetMs: integer; const tm: TTimeTarget):Boolean; overload;
begin
  Result := IsSameFrameOrPassOfs(DEF_CHECK, curFrame, TargetFrame, offsetMs, tm);
end;

function IsFrameGEOrPassOfs(
    checkType: TTimeCheck;
    curFrame, TargetFrame: integer;
    offsetMs: integer; const tm: TTimeTarget):Boolean; overload;
begin
  case checkType of
    tcTick:
      Result := IsTimePassOfs(offsetMs, tm);
    tcFrames:
      Result := curFrame >= targetFrame;
    else
      Result := (curFrame >= targetFrame) or IsTimePassOfs(offsetMs, tm);
  end;
end;

function IsFrameGEOrPassOfs(
    curFrame, TargetFrame: integer;
    offsetMs: integer; const tm: TTimeTarget):Boolean; overload;
begin
  Result := IsFrameGEOrPassOfs(DEF_CHECK, curFrame, TargetFrame, offsetMs, tm);
end;

function IsFrameOffsetIn(
    checkType: TTimeCheck;
    curFrame, MinFrame, MaxFrame: integer;
    MinOfs, MaxOfs: integer;
    const tm: TTimeTarget):Boolean; overload;
var
  t : LongWord;
begin
  case checkType of
    tcTick: begin
      t := TimeSinceStart(tm);
      Result := (t >= LongWord(MinOfs)) and (t <= LongWord(MaxOfs));
    end;
    tcFrames:
      Result := (curFrame >= minFrame) and (curFrame <= maxFrame);
    else
    begin
      Result := (curFrame >= minFrame) and (curFrame <= maxFrame);
      if not result then begin
        t := TimeSinceStart(tm);
        Result := (t >= LongWord(MinOfs)) and (t <= LongWord(MaxOfs));
      end;
    end;
  end;
end;

function IsFrameOffsetIn(
    curFrame, MinFrame, MaxFrame: integer;
    MinOfs, MaxOfs: integer;
    const tm: TTimeTarget):Boolean; overload;
begin
  Result := IsFrameOffsetIn(
    DEF_CHECK,
    curFrame, MinFrame, MaxFrame,
    MinOfs, MaxOfs, tm);
end;

end.
