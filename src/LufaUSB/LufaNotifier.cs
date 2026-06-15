using System;
using System.Threading.Tasks;
using BlockSerial;
using WMAData;

namespace LufaUSB
{
    public class LufaNotifier : IAsyncExperimentNotifier, IDisposable
    {
        BlockSerialAsync serial;

        public bool logResponse = false;
        public string dead_time_photodiode_ms;
        public string dead_time_button_ms;
        public int PhotodiodeInput = 0;  //input bit number of photodiode

        public virtual void Dispose()
        {
            if (serial == null)
                return;
            serial.Dispose();
            serial = null;
        }
        
        private async Task write(string s, int timeoutMs = 5)
        {
            await serial.SendString($"{s}\r\n");
            if (logResponse)
            {
                string rcv = await serial.RecvString(timeoutMs);
                log(rcv);
            }
        }

        private void log(string s)
        {

        }

        public async Task StartExperiment()
        {
            await write($"X");
            await write($"D I 0 {dead_time_photodiode_ms}");
            await write($"D I 1 {dead_time_photodiode_ms}");
            await write($"D I 2 {dead_time_button_ms}");
            await write($"D I 3 {dead_time_button_ms}");
        }
        public async Task MarkerS1(int marker)
        {
            // if LUFA USB serial device is present, load it with trigger data
            // i.e. send it a photodiode input-parallel port output mapping.
            //lufaUSBserial.flush;
            //lufaUSBserial.purge;

            //Left photodiode
            // one-shot mapping. Photodiode, falling edge -> Parallel port s1_marker
            await write($"M 1 I {PhotodiodeInput} 0 P {marker}");
            // one-shot mapping. Photodiode, falling edge -> Digital output 0, 5ms
            await write($"M 1 I {PhotodiodeInput} 0 O 0 0 1 5");

            //Right photodiode
            // one-shot mapping. Photodiode, falling edge -> Parallel port 251
            await write("M 1 I 1 0 P 251");

            // one-shot mapping. Photodiode, falling edge -> Digital output 1, 1000ms
            await write("M 1 I 1 0 O 0 0 2 5");
        }

        public async Task MarkerS2(int marker)
        {
            // one-shot mapping. Photodiode, falling edge -> Parallel port s1_marker
            await write($"M 1 I {PhotodiodeInput} 0 P {marker}");
            // one-shot mapping. Photodiode, falling edge -> Digital output 0, 5ms
            await write($"M 1 I {PhotodiodeInput} 0 O 0 0 1 5");

        }

        public async Task MarkerS3(int marker)
        {
            // one-shot mapping. Photodiode, falling edge -> Parallel port s1_marker
            await write("M 1 I {PhotodiodeInput} 0 P {marker}");
            // one-shot mapping. Photodiode, falling edge -> Digital output 0, 5ms
            await write("M 1 I {PhotodiodeInput} 0 O 0 0 1 5");
        }
        public async Task MarkerS4(int marker)
        {
            log("");
            //log("TrialNo = ' + inttostr(TrialNo) + '. S4');
            log("1.......");
            await write("M 1 I {PhotodiodeInput} 0 P {marker}");

            // one-shot mapping. Photodiode, falling edge -> Digital output 4, 1000ms
            log("2.......");
            // one-shot mapping. Photodiode, falling edge -> Digital output 0, 5ms
            await write("M 1 I {PhotodiodeInput} 0 O 0 0 1 5");

            log("3.......");
            // set up one-shot IO mapping :Input 2 falling edge -> parallel port 252
            await write("M 1 I 2 0 P 252");

            log("4.......");
            // one-shot mapping. Input 2 falling edge -> Digital output 5, 1000ms
            await write("M 1 I 2 0 O 0 0 4 5");

            log("5.......");
            // set up one-shot IO mapping :Input 3 falling edge -> parallel port 253
            await write("M 1 I 3 0 P 253");

            log("6.......");
            // one-shot mapping. Input 2 falling edge -> Digital output 2, 1000ms
            await write("M 1 I 3 0 O 0 0 8 5");

        }
        public async Task Feedback(bool isCorrect)
        {
            if (isCorrect)
            {
                // set up one-shot IO mapping :photodiode  falling edge -> parallel triggerStationData
                // lufaUSBserial.sendstring( 'M 1 I '+inttostr(lufaUSBserialPhotodiodeInput)+' 0 P 254' );
                log("");
                log("TrialNo = ' + inttostr(TrialNo) + '. Feedback correct");
                //lufaUSBserial.flush;
                //lufaUSBserial.purge;
                // one-shot mapping. Photodiode, falling edge -> Parallel port correct
                await write($"M 1 I {PhotodiodeInput} 0 P 254");

                // one-shot mapping. Photodiode, falling edge -> digital out 4, 5ms
                await write($"M 1 I {PhotodiodeInput} 0 O 0 0 16 5");
            }
            else
            {
                // set up one-shot IO mapping :photodiode  falling edge -> parallel triggerStationData
                //lufaUSBserial.sendstring( 'M 1 I '+inttostr(lufaUSBserialPhotodiodeInput)+' 0 P 255' );

                log("");
                log("TrialNo = ' + inttostr(TrialNo) + '. Feedback incorrect");
                //lufaUSBserial.flush;
                //lufaUSBserial.purge;
                // one-shot mapping. Photodiode, falling edge -> Parallel port incorrect
                await write($"M 1 I {PhotodiodeInput} 0 P 255");

                // one-shot mapping. Photodiode, falling edge -> digital out 5, 5ms
                await write($"M 1 I {PhotodiodeInput} 0 O 0 0 32 5");
            }
        }
    }
}
