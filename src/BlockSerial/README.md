# BlockSerial

Small `netstandard2.0` library with async C# equivalents of Synapse `BlockSerial.SendString` and `BlockSerial.RecvString`.

`SendStringAsync` writes the encoded bytes of a string to `SerialPort.BaseStream`.
`RecvStringAsync` waits for a CR/LF terminated string and returns it without `\r\n`, preserving bytes after the terminator in an internal line buffer.

```csharp
using System.IO.Ports;
using System.Text;
using BlockSerial;

var port = new SerialPort("COM3", 9600);
port.Open();

var serial = new TBlockSerialAsync(port, Encoding.GetEncoding(1251));

await serial.SendStringAsync("AT\r");
string line = await serial.RecvStringAsync(1000);

if (serial.LastError != TBlockSerialAsync.SOK)
{
    Console.WriteLine(serial.LastErrorDesc);
}
```

By default `InterPacketTimeout` is `true`, matching Synapse: the timeout is applied between incoming packets. Set it to `false` to treat the timeout as a total operation timeout.
