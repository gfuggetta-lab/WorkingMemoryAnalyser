Unit TriggerStationDevice_DLL_1_0_TLB;

//  Imported TriggerStationDevice_DLL on 12/11/2016 18:20:56 from C:\Program Files\TriggerStation\TriggerStationDevice_DLL.tlb

{$mode delphi}{$H+}

interface

Uses
  Windows,ActiveX,Classes,Variants;
Const
  TriggerStationDevice_DLLMajorVersion = 1;
  TriggerStationDevice_DLLMinorVersion = 0;
  TriggerStationDevice_DLLLCID = 0;
  LIBID_TriggerStationDevice_DLL : TGUID = '{8210BC20-BBA3-41AF-A4E4-0B14207E82DF}';

  CLASS_TriggerStationDevice : TGUID = '{22EB74FC-0A64-3F11-B238-F4A919398858}';
  IID_ITriggerStationDevice : TGUID = '{B9B57834-210A-3CA6-A5D0-C7DEA9715919}';
  IID__TriggerStationDevice : TGUID = '{10523AB6-A82B-3264-AC31-1C14F3EDE7B8}';

//Enums

Type
  keyModifiers =LongWord;
Const
  keyModifiers_NoEvent = $0000000000000000;
  keyModifiers_CtrlMask = $0000000000000001;
  keyModifiers_ShiftMask = $0000000000000002;
  keyModifiers_AltMask = $0000000000000004;
  keyModifiers_WinMask = $0000000000000008;
  keyModifiers_AltGrMask = $0000000000000040;
Type
  ekeyCodes =LongWord;
Const
  ekeyCodes_none = $0000000000000000;
  ekeyCodes_key_a = $0000000000000004;
  ekeyCodes_key_b = $0000000000000005;
  ekeyCodes_key_c = $0000000000000006;
  ekeyCodes_key_d = $0000000000000007;
  ekeyCodes_key_e = $0000000000000008;
  ekeyCodes_key_f = $0000000000000009;
  ekeyCodes_key_g = $000000000000000A;
  ekeyCodes_key_h = $000000000000000B;
  ekeyCodes_key_i = $000000000000000C;
  ekeyCodes_key_j = $000000000000000D;
  ekeyCodes_key_k = $000000000000000E;
  ekeyCodes_key_l = $000000000000000F;
  ekeyCodes_key_m = $0000000000000010;
  ekeyCodes_key_n = $0000000000000011;
  ekeyCodes_key_o = $0000000000000012;
  ekeyCodes_key_p = $0000000000000013;
  ekeyCodes_key_q = $0000000000000014;
  ekeyCodes_key_r = $0000000000000015;
  ekeyCodes_key_s = $0000000000000016;
  ekeyCodes_key_t = $0000000000000017;
  ekeyCodes_key_u = $0000000000000018;
  ekeyCodes_key_v = $0000000000000019;
  ekeyCodes_key_w = $000000000000001A;
  ekeyCodes_key_x = $000000000000001B;
  ekeyCodes_key_y = $000000000000001C;
  ekeyCodes_key_z = $000000000000001D;
  ekeyCodes_key_1 = $000000000000001E;
  ekeyCodes_key_2 = $000000000000001F;
  ekeyCodes_key_3 = $0000000000000020;
  ekeyCodes_key_4 = $0000000000000021;
  ekeyCodes_key_5 = $0000000000000022;
  ekeyCodes_key_6 = $0000000000000023;
  ekeyCodes_key_7 = $0000000000000024;
  ekeyCodes_key_8 = $0000000000000025;
  ekeyCodes_key_9 = $0000000000000026;
  ekeyCodes_key_0 = $0000000000000027;
  ekeyCodes_key_Space = $000000000000002C;
  ekeyCodes_key_PageUp = $000000000000004B;
  ekeyCodes_key_PageDown = $000000000000004E;
  ekeyCodes_key_RightArrow = $000000000000004F;
  ekeyCodes_key_LeftArrow = $0000000000000050;
  ekeyCodes_key_DownArrow = $0000000000000051;
  ekeyCodes_key_UpArrow = $0000000000000052;
//Forward declarations

Type
 ITriggerStationDevice = interface;
 ITriggerStationDeviceDisp = dispinterface;
 _TriggerStationDevice = interface;
 _TriggerStationDeviceDisp = dispinterface;

//Map CoClass to its default interface

 TriggerStationDevice = _TriggerStationDevice;

//records, unions, aliases


//interface declarations

// ITriggerStationDevice : 

 ITriggerStationDevice = interface(IDispatch)
   ['{B9B57834-210A-3CA6-A5D0-C7DEA9715919}']
    // BNC1 :  
   function BNC1(outputState:WordBool):WordBool;safecall;
    // BNC2 :  
   function BNC2(outputState:WordBool):WordBool;safecall;
    // BNC3 :  
   function BNC3(outputState:WordBool):WordBool;safecall;
    // ConnectToDevice :  
   function ConnectToDevice(deviceIndex:Integer):WordBool;safecall;
    // DisconnectDevice :  
   procedure DisconnectDevice;safecall;
    // EnumerateDevices :  
   function EnumerateDevices:Integer;safecall;
    // FastPulse :  
   function FastPulse(DutyCycle:Integer;outputState:WordBool):WordBool;safecall;
    // GetDeviceName :  
   function GetDeviceName:WideString;safecall;
    // GetDeviceSerialNumber :  
   function GetDeviceSerialNumber:Integer;safecall;
    // GetUserID :  
   function GetUserID(var userID:Byte):Byte;safecall;
    // LED :  
   function LED(pin:Integer;outputState:WordBool):WordBool;safecall;
    // Microphone :  
   function Microphone(Key:WideString;status:WordBool):WordBool;safecall;
    // ParallelPort :  
   function ParallelPort(value:Integer):WordBool;safecall;
    // PhotoDiode :  
   function PhotoDiode(status:WordBool):WordBool;safecall;
    // ReadSharedRam :  
   function ReadSharedRam(slot:Integer):Integer;safecall;
    // rTMS :  
   function rTMS(outputState:WordBool):WideString;safecall;
    // RunEngine :  
   function RunEngine(status:WordBool):WordBool;safecall;
    // SetPhotoDiode :  
   function SetPhotoDiode(ThresholdValue:Integer):Integer;safecall;
    // TouchPortA :  
   function TouchPortA(KeyPinNumber:Integer;Key:WideString;KeyRelease:WordBool;status:WordBool):WordBool;safecall;
    // TouchPortB :  
   function TouchPortB(KeyPadNumber:Integer;Key:WideString;KeyRelease:WordBool;status:WordBool):WordBool;safecall;
    // WriteSharedRam :  
   function WriteSharedRam(slot:Integer;value:Integer):WordBool;safecall;
  end;


// ITriggerStationDevice : 

 ITriggerStationDeviceDisp = dispinterface
   ['{B9B57834-210A-3CA6-A5D0-C7DEA9715919}']
    // BNC1 :  
   function BNC1(outputState:WordBool):WordBool;dispid 1610743808;
    // BNC2 :  
   function BNC2(outputState:WordBool):WordBool;dispid 1610743809;
    // BNC3 :  
   function BNC3(outputState:WordBool):WordBool;dispid 1610743810;
    // ConnectToDevice :  
   function ConnectToDevice(deviceIndex:Integer):WordBool;dispid 1610743811;
    // DisconnectDevice :  
   procedure DisconnectDevice;dispid 1610743812;
    // EnumerateDevices :  
   function EnumerateDevices:Integer;dispid 1610743813;
    // FastPulse :  
   function FastPulse(DutyCycle:Integer;outputState:WordBool):WordBool;dispid 1610743814;
    // GetDeviceName :  
   function GetDeviceName:WideString;dispid 1610743815;
    // GetDeviceSerialNumber :  
   function GetDeviceSerialNumber:Integer;dispid 1610743816;
    // GetUserID :  
   function GetUserID(var userID:Byte):Byte;dispid 1610743817;
    // LED :  
   function LED(pin:Integer;outputState:WordBool):WordBool;dispid 1610743818;
    // Microphone :  
   function Microphone(Key:WideString;status:WordBool):WordBool;dispid 1610743819;
    // ParallelPort :  
   function ParallelPort(value:Integer):WordBool;dispid 1610743820;
    // PhotoDiode :  
   function PhotoDiode(status:WordBool):WordBool;dispid 1610743821;
    // ReadSharedRam :  
   function ReadSharedRam(slot:Integer):Integer;dispid 1610743822;
    // rTMS :  
   function rTMS(outputState:WordBool):WideString;dispid 1610743823;
    // RunEngine :  
   function RunEngine(status:WordBool):WordBool;dispid 1610743824;
    // SetPhotoDiode :  
   function SetPhotoDiode(ThresholdValue:Integer):Integer;dispid 1610743825;
    // TouchPortA :  
   function TouchPortA(KeyPinNumber:Integer;Key:WideString;KeyRelease:WordBool;status:WordBool):WordBool;dispid 1610743826;
    // TouchPortB :  
   function TouchPortB(KeyPadNumber:Integer;Key:WideString;KeyRelease:WordBool;status:WordBool):WordBool;dispid 1610743827;
    // WriteSharedRam :  
   function WriteSharedRam(slot:Integer;value:Integer):WordBool;dispid 1610743828;
  end;


// _TriggerStationDevice : 

 _TriggerStationDevice = interface(IDispatch)
   ['{10523AB6-A82B-3264-AC31-1C14F3EDE7B8}']
  end;


// _TriggerStationDevice : 

 _TriggerStationDeviceDisp = dispinterface
   ['{10523AB6-A82B-3264-AC31-1C14F3EDE7B8}']
  end;

//CoClasses
  CoTriggerStationDevice = Class
  Public
    Class Function Create: ITriggerStationDevice;
    Class Function CreateRemote(const MachineName: string): ITriggerStationDevice;
  end;

implementation

uses comobj;

Class Function CoTriggerStationDevice.Create: ITriggerStationDevice;
begin
  Result := CreateComObject(CLASS_TriggerStationDevice) as ITriggerStationDevice;
end;

Class Function CoTriggerStationDevice.CreateRemote(const MachineName: string): ITriggerStationDevice;
begin
  Result := CreateRemoteComObject(MachineName,CLASS_TriggerStationDevice) as ITriggerStationDevice;
end;

end.
