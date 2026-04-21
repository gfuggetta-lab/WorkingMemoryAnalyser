unit usbserialunit;

{$mode delphi}

interface







uses
  Classes, SysUtils, SynaSer, registry;



  function GetSerialPortNamesExt: string;


implementation



  function GetSerialPortNamesExt: string;
var
  reg  : TRegistry;
  l,v  : TStringList;
  n    : integer;
  pn,fn: string;

  function findFriendlyName(key: string; port: string): string;
  var
    r : TRegistry;
    k : TStringList;
    i : Integer;
    ck: string;
    rs: string;
  begin
    r := TRegistry.Create;
    k := TStringList.Create;

    r.RootKey := HKEY_LOCAL_MACHINE;
    r.OpenKeyReadOnly(key);
    r.GetKeyNames(k);
    r.CloseKey;

    try
      for i := 0 to k.Count - 1 do
      begin
        ck := key + k[i] + '\'; // current key
        // looking for "PortName" stringvalue in "Device Parameters" subkey
        if r.OpenKeyReadOnly(ck + 'Device Parameters') then
        begin
          if r.ReadString('PortName') = port then
          begin
            //Memo1.Lines.Add('--> ' + ck);
            r.CloseKey;
            r.OpenKeyReadOnly(ck);
            rs := r.ReadString('FriendlyName');
            Break;
          end // if r.ReadString('PortName') = port ...
        end  // if r.OpenKeyReadOnly(ck + 'Device Parameters') ...
        // keep looking on subkeys for "PortName"
        else // if not r.OpenKeyReadOnly(ck + 'Device Parameters') ...
        begin
          if r.OpenKeyReadOnly(ck) and r.HasSubKeys then
          begin
            rs := findFriendlyName(ck, port);
            if rs <> '' then Break;
          end; // if not (r.OpenKeyReadOnly(ck) and r.HasSubKeys) ...
        end; // if not r.OpenKeyReadOnly(ck + 'Device Parameters') ...
      end; // for i := 0 to k.Count - 1 ...
      result := rs;
    finally
      r.Free;
      k.Free;
    end; // try ...
  end; // function findFriendlyName ...

begin
  v      := TStringList.Create;
  l      := TStringList.Create;
  reg    := TRegistry.Create;
  Result := '';

  try
    reg.RootKey := HKEY_LOCAL_MACHINE;
    if reg.OpenKeyReadOnly('HARDWARE\DEVICEMAP\SERIALCOMM') then
    begin
      reg.GetValueNames(l);

      for n := 0 to l.Count - 1 do
      begin
        pn := reg.ReadString(l[n]);
        fn := findFriendlyName('\System\CurrentControlSet\Enum\', pn);
        v.Add(pn + ' = '+ fn);
      end; // for n := 0 to l.Count - 1 ...

      Result := v.CommaText;
    end; // if reg.OpenKeyReadOnly('HARDWARE\DEVICEMAP\SERIALCOMM') ...
  finally
    reg.Free;
    v.Free;
  end; // try ...
end;


end.
