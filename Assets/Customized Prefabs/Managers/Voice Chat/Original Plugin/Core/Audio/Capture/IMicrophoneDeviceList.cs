using System.Collections.Generic;

namespace Dissonance.Audio.Capture
{
    public interface IMicrophoneDeviceList
    {
        /// <summary>
        /// Gets a list of all valid microphone devices.
        /// </summary>
        /// <param name="output">A list for results to be added to</param>
#if !UNITY_WEBGL
        void GetDevices(List<string> output);
#endif
    }
}
