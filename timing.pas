unit Timing;

//{$MODE Delphi}
 {$mode objfpc}
interface

uses
  windows,Dialogs,sysutils,math;

type
  TMetronome = class
  Frequency:real;
  Period:real;
  Count:integer;
  ElapsedTime:real;
  lpClockFrequency,
  lpStartTime,
  lpCurrentTime:int64;

  procedure Start(freq:real);
  procedure Reset;
  function Query:boolean;
end;

type
  TTimer = class
  TimeOfQuery:real;
  lpClockFrequency,
  lpStartTime,
  lpCurrentTime:int64;

  procedure Start;
  function Query:extended;

end;

type
  TPWM = class
  period:real;
  pulseWidth:real;
  output:boolean;

  pulseWidthSec : real;
  timeModulus:extended;

  PWMtimer :TTimer;

  constructor Create;
  procedure Start;
  function State:boolean;

end;


type
  Tservo = class
  period:real;
  angle:real;

  minAngle :real;
  maxAngle :real;

  pulseWidthSec:real;

  minPulseSec:real;
  maxPulseSec:real;

  output:boolean;

  timeModulus:extended;

  k: array [0..3] of real;    // cubic mapping function to eliminate servo nonlinearity

  remappedangle  :real;

  PWMtimer :TTimer;

  constructor Create;
  //procedure Start;
  function State:boolean;

end;

procedure WaitSecs(Secs:real);

implementation



//------------------------------------------------------------------------------
procedure TMetronome.Start(freq:real);
begin
  Frequency:=freq;
  QueryPerformanceFrequency(lpClockFrequency);
  queryperformancecounter(lpStartTime);
  Count:=0;
  Period:=1/freq;
end;
//------------------------------------------------------------------------------





//------------------------------------------------------------------------------
procedure TMetronome.Reset;
begin
  Frequency:=0;
  Period:=0;
  Count:=0;
  ElapsedTime:=0;
  lpClockFrequency:=0;
  lpStartTime:=0;
  lpCurrentTime:=0;
end;
//------------------------------------------------------------------------------





//------------------------------------------------------------------------------
function TMetronome.Query:boolean;

begin

    queryperformancecounter(lpCurrentTime);
    ElapsedTime:=(lpCurrentTime-lpStartTime)/lpClockFrequency;
    if ElapsedTime > ((Count)*Period) then
    begin
      Count:=Count+1;
      //showmessage(floattostr(elapsedtime));
      result:=true
    end
    else
    begin
      result:=false;
    end;

end;
//------------------------------------------------------------------------------




//------------------------------------------------------------------------------
procedure TTimer.Start;
begin
  QueryPerformanceFrequency(lpClockFrequency);
  queryperformancecounter(lpStartTime);
end;
//------------------------------------------------------------------------------




//------------------------------------------------------------------------------
function TTimer.Query:extended;
begin
  queryperformancecounter(lpCurrentTime);
  TimeOfQuery:= (lpCurrentTime-lpStartTime) / lpClockFrequency;
  result:=TimeOfQuery;
end;
//------------------------------------------------------------------------------





//------------------------------------------------------------------------------
procedure WaitSecs(Secs:real);
var
  lpClockFrequency,
  lpStartTime,
  lpCurrentTime:int64;

begin
  QueryPerformanceFrequency(lpClockFrequency);
  queryperformancecounter(lpStartTime);

  repeat
     queryperformancecounter(lpCurrentTime);
  until ((lpCurrentTime-lpStartTime) / lpClockFrequency) >= Secs;
  
end;
//------------------------------------------------------------------------------





//------------------------------------------------------------------------------
//Create Pulse Width Modulation object

constructor TPWM.Create;

begin
  inherited Create;  // Initialize inherited parts
  period := 1;
  pulseWidth := 0.5;
  output := false;
  PWMtimer :=TTimer.create;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
//Start the Pulse Width Modulation signal

procedure TPWM.Start;

begin
  PWMtimer.start;
end;
//------------------------------------------------------------------------------



//------------------------------------------------------------------------------
//Get the current output state of the Pulse Width Modulation object

function TPWM.State:boolean;

begin

    pulseWidthSec := pulseWidth * period;
    if not(period=0) then
    begin
      timeModulus := PWMtimer.query - Floor ( PWMtimer.query / period ) * period;
    end
    else
    begin
      timemodulus := 0;
    end;

    //showmessage(floattostr(timemodulus));
    if (timeModulus<pulseWidthSec) then
    begin
      output := true;
    end
    else
    begin
      output := false
    end;

end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
//Create servo object
// servo angle:
// 1.25ms = 0 deg
// 1.5ms = 90 deg
// 1.75ms = 180 deg
constructor Tservo.Create;
var
  pulseWidthRange:real;

begin
  inherited Create;  // Initialize inherited parts
  period := 1;
  angle := 90;

  minAngle := 20;
  maxAngle := 160;

  //coefficients of cubic mapping function
{  k[0]:=0;    // cubic
  k[1]:=0;    // quad
  k[2]:=1;    // linear
  k[3]:=0;    // intercept
  }
k[0] := 0;
k[1] := 0;
k[2] := 1;
k[3] := 0;

  MinPulseSec := 0.00075;
  MaxPulseSec := 0.0025;

  remappedangle := (k[0] * power(angle,3)) + (k[1] * power(angle,2)) + (k[2] * angle) + k[3];

  pulseWidthRange := MaxPulseSec - MinPulseSec;
  pulseWidthSec := MinPulseSec + ((remappedangle/180) * PulseWidthRange);

  output := false;
  PWMtimer :=TTimer.create;
  PWMtimer.start;

end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
//Get the current output state of the servo object

function Tservo.State:boolean;
var
  pulseWidthRange:real;
  x: real; // pulsewidth in seconds with linear mapping
begin

  if angle>maxAngle then angle := maxAngle;
  if angle<minAngle then angle := minAngle;

  remappedangle := (k[0] * power(angle,3)) + (k[1] * power(angle,2)) + (k[2] * angle) + k[3];

  pulseWidthRange := MaxPulseSec - MinPulseSec;
  pulseWidthSec := MinPulseSec + ((remappedangle/180) * PulseWidthRange);

  if not(period=0) then
  begin
    timeModulus := PWMtimer.query - Floor ( PWMtimer.query / period ) * period;
  end
  else
  begin
    timemodulus := 0;
  end;

  if (timeModulus<pulseWidthSec) then
  begin
    output := true;
  end
  else
  begin
    output := false
  end;

end;
//------------------------------------------------------------------------------
end.
