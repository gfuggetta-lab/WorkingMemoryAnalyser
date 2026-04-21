unit edidTypes;
// MIT License
//
// Copyright (c) 2020 Dmitry Boyarintsev
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

{$mode objfpc}{$H+}

interface

type
  // Manufacturer ID is in big-endian byte order. This is important for Letter2
  // as it rolls over the between the first and the second byte
  TEDIDMan = bitpacked record
  case byte of
   1: (
    let3 : 0..31; // bits 0..4
    let2 : 0..31; // bits 5..9
    let1 : 0..31; // bits 10..14
    res  : 0..1;  // bit 15
    );
   2: (w: word;)
  end;

  TEDIDVideoInput = bitpacked record
  case byte of
    // analog
    0:(
      AnVSyncPulse  : 0..1;
      AnSyncOnGreen : 0..1;
      AnIsCompSync  : 0..1;
      AnSepSync     : 0..1;
      AnBlankToBlack: 0..1;
      AnSyncLevel   : 0..3;
      IsNotAnalog   : 0..1; // isDigital
    );
    // digital
    1: (
      DigVideoIntf  : 0..15;
      DigBitDepth   : 0..7;
      isDigital     : 0..1;
    );
  end;

  TEDIDFeatures = bitpacked record
    isContTiming           : 0..1; // Continuous timings with GTF or CVT
    isPrefTimingSpecified  : 0..1; // Preferred timing mode specified in descriptor block 1
    isStdsRGB              : 0..1; // Standard sRGB colour space. Bytes 25–34 must contain sRGB standard values.
    DisplayType            : 0..3; //
    DPMSActiveOff          : 0..1; // DPMS active-off supported
    DPMSSuspend            : 0..1; // DPMS suspend supported
    DPMSStandBy            : 0..1; // DPMS standby supported
  end;

  TEDIDSupportedTiming = bitpacked record
  case byte of
    0: (flag : 0..1 shl 24 - 1);
  end;

  TEDIDAspect = bitpacked record
    VertFreq    : 0..63;
    AspectRatio : 0..3;
  end;

  TEDIDActiveBlank8Bit = packed record
    Active : byte;
    Blank  : byte;
  end;

  TEDIDActiveBlank4Bit = bitpacked record
    Blank  : 0..15;
    Active : 0..15;
  end;

  TEDIDOffsetSync8Bit = packed record
    Active : byte;
    Blank  : byte;
  end;

  TEDIDOffsetSync4Bit = bitpacked record
    Blank  : 0..15;
    Active : 0..15;
  end;

  TEDIDOffsetSync2Bit = bitpacked record
    VertSync   : 0..3;
    VertOffset : 0..3;
    HorzSync   : 0..3;
    HorzOffset : 0..3;
  end;

  TEDIDImageSize4Bit = bitpacked record
    vert  : 0..15;
    horz  : 0..15;
  end;

  TEDIDDetailedTimeDescr = packed record
    pixelClock : Word; // little endian
    horzAB8    : TEDIDActiveBlank8Bit;
    horzAB4    : TEDIDActiveBlank4Bit;
    vertAB8    : TEDIDActiveBlank8Bit;
    vertAB4    : TEDIDActiveBlank4Bit;
    horzPS8    : TEDIDOffsetSync8Bit;
    vertPS4    : TEDIDOffsetSync4Bit;
    PorchSync2 : TEDIDOffsetSync2Bit;
    horzMm     : Byte;
    vertMm     : Byte;
    imageSize  : TEDIDImageSize4Bit;
    horzBorder : Byte;
    vertBorder : Byte;
    Features   : Byte;
  end;

  TEDIDDisplayDescr = packed record
    zero      : Word; // must be zero
    res       : Byte;
    descrType : Byte;
    version   : Byte; // used by
    case byte of // value of descrType  Display Range Limits Descriptor.
      00: (buf         : array [0..12] of byte);
     $FE: (text        : array [0..12] of char);
     $FC: (displayText : array [0..12] of char);
     $FF: (serialNum   : array [0..12] of char);
  end;

  TEDIDDescr = packed record
  case byte of
    0: (time: TEDIDDetailedTimeDescr);
    1: (disp: TEDIDDisplayDescr);
  end;

  TEDIDStdTiming = packed record
    res    : Byte;
    aspect : TEDIDAspect;
  end;

  TEDIDRec = packed record   // Structure, version 1.4
    // Header information
    Hdr      : array [0..7] of byte;
    ManId    : TEDIDMan;
    ProdCode : Word;
    SerNum   : LongWord;
    ManWeek  : Byte;
    ManYear  : Byte;
    EdidVer  : Byte;
    EdidRev  : Byte;
    // Basic display parameters
    VideoInp : TEDIDVideoInput;
    SzH      : Byte;
    SzV      : Byte;
    Gamma    : Byte;
    Features : TEDIDFeatures;
    // Chromaticity coordinates.
    redGreenXY : Byte;
    blueWhite  : Byte;
    redX       : Byte;
    redY       : Byte;
    greenX     : Byte;
    greenY     : Byte;
    blueX      : Byte;
    blueY      : Byte;
    whiteX     : Byte;
    whiteY     : Byte;
    // Supported Timings
    supTime1   : Byte;
    supTime2   : Byte;
    supTime3   : Byte;
    // Standard timing information. Up to 8 2-byte fields describing standard display modes
    suppRes    : array [0..7] of TEDIDStdTiming;
    descr      : array [0..3] of TEDIDDescr;
    extNum     : Byte;
    checkSum   : Byte;
  end;
  PEDIDRec = ^TEDIDRec;

const
  VIDEOINTF_UNDEF   = 0; // = undefined
  VIDEOINTF_HDMIA   = 2;
  VIDEOINTF_HDMIB   = 3;
  VIDEOINTF_MDDI    = 8; // MDDI
  VIDEOINTF_DISPORT = 9; // DisplayPort

  DISPTYPE_ANALOG_MONO = 0;
  DISPTYPE_ANALOG_RGB  = 1;
  DISPTYPE_ANALOG_NRGB = 2;
  DISPTYPE_ANALOG_UND  = 3;

  DISPTYPE_DIGIT_444         = 0;
  DISPTYPE_DIGIT_444_444     = 1;
  DISPTYPE_DIGIT_444_422     = 2;
  DISPTYPE_DIGIT_444_444_422 = 3;

  SUPTIME1_720_400_70   = 1 shl 7; // VGA
  SUPTIME1_720_400_88   = 1 shl 6; // XGA
  SUPTIME1_640_480_60   = 1 shl 5; // VGA
  SUPTIME1_640_480_67   = 1 shl 4; // Apple Macintosh II
  SUPTIME1_640_480_72   = 1 shl 3;
  SUPTIME1_640_480_75   = 1 shl 2;
  SUPTIME1_800_600_56   = 1 shl 1;
  SUPTIME1_800_600_60   = 1 shl 0;

  SUPTIME2_800_600_72   = 1 shl 7;
  SUPTIME2_800_600_75   = 1 shl 6;
  SUPTIME2_832_624_75   = 1 shl 5; // Apple Macintosh II
  SUPTIME2_1024_768_87  = 1 shl 4; // interlaced (1024×768i)
  SUPTIME2_1024_768_60  = 1 shl 3;
  SUPTIME2_1024_768_70  = 1 shl 2;
  SUPTIME2_1024_768_75  = 1 shl 1;
  SUPTIME2_1280_1024_75 = 1 shl 0;

  SUPTIME3_1152_870_75  = 1 shl 7; // Apple Macintosh II

  ASPECT_16_10 = 0;
  ASPECT_4_3   = 1;
  ASPECT_5_4   = 2;
  ASPECT_16_9  = 3;

  DISPDESCR_DUMMY        = $10;
  DISPDESCR_ADDSTDTIME   = $F8;
  DISPDESCR_CVT          = $F9;
  DISPDESCR_DCM          = $F9;
  DISPDESCR_ADDSTDTIMEID = $FA;
  DISPDESCR_ADDWHITE     = $FB;
  DISPDESCR_DISPNAME     = $FC;
  DISPDESCR_DISPRANGE    = $FD;
  DISPDESCR_TEXT         = $FE;
  DISPDESCR_SERIALNUM    = $FF;

function EdidManToStr(const m: TEDIDMan): string;
function EdidGetDisplayName(const m: TEDIDRec): string;
function EdidGetPhysSizeMm(const ed: TEDIDRec; out horzMm, vertMM: Integer): Boolean;

implementation

function EdidManToStr(const m: TEDIDMan): string;
const
  base =  Ord('A')-1;
var
  le : TEDIDMan;
begin
  Result:='';
  le.w := BEtoN(m.w);
  SetLength(Result, 3);
  Result[1]:=Chr(le.let1+base);
  Result[2]:=Chr(le.let2+base);
  Result[3]:=Chr(le.let3+base);
end;

function Trim(const ch: array of char): string;
var
  i : integer;
begin
  i := length(ch)-1;
  while (i>=0) do begin
    if not (ch[i] in [#0, #32, #9, #13, #10]) then
      Break;
    dec(i);
  end;

  if (i < 0) then Result := '';
  inc(i);
  SetLength(Result, i);
  Move(ch[0], Result[1], i);
end;

function EdidGetDisplayName(const m: TEDIDRec): string;
var
  i : integer;
begin
  for i := low(m.descr) to high(m.descr) do begin
    if m.descr[i].disp.zero <> 0 then Continue;
    if m.descr[i].disp.descrType = DISPDESCR_DISPNAME then
    begin
      Result := Trim(m.descr[i].disp.displayText);
      Exit;
    end;
  end;
  Result := '';
end;

function EdidGetPhysSizeMm(const ed: TEDIDRec; out horzMm, vertMM: Integer): Boolean;
var
  i : integer;
begin
  horzMm := 0;
  vertMm := 0;
  for i:=low(ed.descr) to high(ed.descr) do
    if (ed.descr[i].time.pixelClock>0) then begin
      horzMm := (ed.descr[i].time.imageSize.horz shl 8) or (ed.descr[i].time.horzMm);
      vertMm := (ed.descr[i].time.imageSize.vert shl 8) or (ed.descr[i].time.vertMm);
      break;
    end;

  if (horzMm = 0) and (vertMm = 0) then begin
    horzMm := ed.SzH * 10;
    vertMm := ed.SzV * 10;
  end;

  Result := (horzMm > 0) and (vertMm > 0)
end;

end.

