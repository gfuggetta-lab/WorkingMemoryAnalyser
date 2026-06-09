using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSerial
{
    /// <summary>
    /// Async adapter for the Synapse TBlockSerial SendString and RecvString behavior.
    /// </summary>
    public sealed class BlockSerialAsync : IDisposable
    {
        public const int SOK = 0;
        public const int ErrPortNotOpen = 9994;
        public const int ErrMaxBuffer = 9996;
        public const int ErrTimeout = 9997;

        private static readonly byte[] CrLf = { 13, 10 };

        private readonly SerialPort _serialPort;
        private readonly bool _ownsPort;
        private readonly List<byte> _lineBuffer = new List<byte>();

        public BlockSerialAsync(SerialPort serialPort, Encoding encoding = null, bool ownsPort = false)
        {
            if (serialPort == null)
            {
                throw new ArgumentNullException(nameof(serialPort));
            }

            _serialPort = serialPort;
            _ownsPort = ownsPort;
            Encoding = encoding ?? Encoding.Default;
            LastError = SOK;
            LastErrorDesc = GetErrorDesc(SOK);
            ReceiveBufferSize = 4096;
            InterPacketTimeout = true;
        }

        public Encoding Encoding { get; set; }

        public int LastError { get; private set; }

        public string LastErrorDesc { get; private set; }

        public int ReceiveBufferSize { get; set; }

        public int MaxLineLength { get; set; }

        public bool InterPacketTimeout { get; set; }

        public string LineBuffer
        {
            get { return Encoding.GetString(_lineBuffer.ToArray(), 0, _lineBuffer.Count); }
            set
            {
                _lineBuffer.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    _lineBuffer.AddRange(Encoding.GetBytes(value));
                }
            }
        }

        public async Task SendStringAsync(string data, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!EnsurePortOpen())
            {
                return;
            }

            SetSynaError(SOK);
            byte[] bytes = Encoding.GetBytes(data);
            if (bytes.Length == 0)
            {
                return;
            }

            await _serialPort.BaseStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
            await _serialPort.BaseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task SendString(string data, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendStringAsync(data, cancellationToken);
        }

        public async Task<string> RecvStringAsync(int timeoutMilliseconds, CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] bytes = await RecvTerminatedAsync(timeoutMilliseconds, CrLf, cancellationToken).ConfigureAwait(false);
            if (LastError == SOK)
            {
                return Encoding.GetString(bytes, 0, bytes.Length);
            }

            return string.Empty;
        }

        public Task<string> RecvString(int timeoutMilliseconds, CancellationToken cancellationToken = default(CancellationToken))
        {
            return RecvStringAsync(timeoutMilliseconds, cancellationToken);
        }

        public Task<string> Recvstring(int timeoutMilliseconds, CancellationToken cancellationToken = default(CancellationToken))
        {
            return RecvStringAsync(timeoutMilliseconds, cancellationToken);
        }

        private async Task<byte[]> RecvTerminatedAsync(
            int timeoutMilliseconds,
            byte[] terminator,
            CancellationToken cancellationToken)
        {
            if (!EnsurePortOpen())
            {
                return new byte[0];
            }

            SetSynaError(SOK);
            if (terminator == null || terminator.Length == 0)
            {
                return new byte[0];
            }

            int effectiveTimeout = timeoutMilliseconds;
            var received = new List<byte>();

            while (true)
            {
                long started = Stopwatch.GetTimestamp();
                byte[] packet = await RecvPacketAsync(effectiveTimeout, cancellationToken).ConfigureAwait(false);
                if (LastError != SOK)
                {
                    break;
                }

                received.AddRange(packet);

                if (MaxLineLength != 0 && received.Count > MaxLineLength)
                {
                    SetSynaError(ErrMaxBuffer);
                    break;
                }

                int terminatorIndex = IndexOf(received, terminator);
                if (terminatorIndex >= 0)
                {
                    byte[] result = received.GetRange(0, terminatorIndex).ToArray();
                    int restStart = terminatorIndex + terminator.Length;
                    _lineBuffer.Clear();
                    if (restStart < received.Count)
                    {
                        _lineBuffer.AddRange(received.GetRange(restStart, received.Count - restStart));
                    }

                    return result;
                }

                if (!InterPacketTimeout && effectiveTimeout >= 0)
                {
                    effectiveTimeout -= ElapsedMilliseconds(started);
                    if (effectiveTimeout <= 0)
                    {
                        SetSynaError(ErrTimeout);
                        break;
                    }
                }
            }

            _lineBuffer.Clear();
            _lineBuffer.AddRange(received);
            return new byte[0];
        }

        private async Task<byte[]> RecvPacketAsync(int timeoutMilliseconds, CancellationToken cancellationToken)
        {
            SetSynaError(SOK);

            if (_lineBuffer.Count > 0)
            {
                byte[] buffered = _lineBuffer.ToArray();
                _lineBuffer.Clear();
                return buffered;
            }

            await Task.Yield();

            int waiting = _serialPort.BytesToRead;
            if (waiting <= 0)
            {
                bool canRead = await WaitForDataAsync(timeoutMilliseconds, cancellationToken).ConfigureAwait(false);
                if (!canRead)
                {
                    SetSynaError(ErrTimeout);
                    return new byte[0];
                }

                waiting = _serialPort.BytesToRead;
                if (waiting <= 0)
                {
                    SetSynaError(ErrTimeout);
                    return new byte[0];
                }
            }

            return await ReadAvailableAsync(waiting, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> WaitForDataAsync(int timeoutMilliseconds, CancellationToken cancellationToken)
        {
            long started = Stopwatch.GetTimestamp();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_serialPort.BytesToRead > 0)
                {
                    return true;
                }

                if (timeoutMilliseconds >= 0 && ElapsedMilliseconds(started) >= timeoutMilliseconds)
                {
                    return false;
                }

                await Task.Delay(10, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<byte[]> ReadAvailableAsync(int requestedCount, CancellationToken cancellationToken)
        {
            int count = Math.Max(1, Math.Min(requestedCount, ReceiveBufferSize));
            byte[] buffer = new byte[count];
            int read = await _serialPort.BaseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

            if (read == buffer.Length)
            {
                return buffer;
            }

            byte[] result = new byte[read];
            Array.Copy(buffer, result, read);
            return result;
        }

        private bool EnsurePortOpen()
        {
            if (!_serialPort.IsOpen)
            {
                SetSynaError(ErrPortNotOpen);
                return false;
            }

            return true;
        }

        private void SetSynaError(int error)
        {
            LastError = error;
            LastErrorDesc = GetErrorDesc(error);
        }

        private static int IndexOf(List<byte> source, byte[] value)
        {
            int limit = source.Count - value.Length;
            for (int i = 0; i <= limit; i++)
            {
                bool matched = true;
                for (int j = 0; j < value.Length; j++)
                {
                    if (source[i + j] != value[j])
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int ElapsedMilliseconds(long startedTimestamp)
        {
            long ticks = Stopwatch.GetTimestamp() - startedTimestamp;
            return (int)(ticks * 1000 / Stopwatch.Frequency);
        }

        public static string GetErrorDesc(int errorCode)
        {
            switch (errorCode)
            {
                case SOK:
                    return "OK";
                case ErrPortNotOpen:
                    return "Instance not yet connected";
                case ErrMaxBuffer:
                    return "Maximal buffer length exceeded";
                case ErrTimeout:
                    return "Timeout during operation";
                default:
                    return "Communication error " + errorCode;
            }
        }

        public void Dispose()
        {
            if (_ownsPort)
            {
                _serialPort.Dispose();
            }
        }
    }
}
