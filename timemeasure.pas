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
  end;

  TTimeCheck = (tcBoth, tcTick, tcFrames);

procedure StartTarget(durationMs: integer; refreshRate: integer; out tm: TTimeTarget); overload;
function IsTimeOver(check: TTimeCheck; frames: integer; const tm: TTimeTarget): boolean; overload;
function IsTimeOverAny(frames: integer; const tm: TTimeTarget): boolean;
function IsTimeOverTick(const tm: TTimeTarget): boolean;
function IsTimeOverFrames(frames: integer; const tm: TTimeTarget): boolean;

var
  DEF_CHECK : TTimecheck = tcBoth;
  DEF_REFRESHRATE : Integer = 60;

procedure StartTarget(durationMs: integer; out tm: TTimeTarget); overload;
function IsTimeOver(frames: integer; const tm: TTimeTarget): boolean; inline; overload;

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

end.
