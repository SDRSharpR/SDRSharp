using PortAudioSharp;
using System.Collections.Generic;

namespace SDRSharp.Radio.PortAudio
{
	public class AudioDevice
	{
		public int Index
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string Host
		{
			get;
			set;
		}

		public DeviceDirection Direction
		{
			get;
			set;
		}

		public bool IsDefault
		{
			get;
			set;
		}

		public static List<AudioDevice> GetDevices(DeviceDirection direction)
		{
			List<AudioDevice> list = new List<AudioDevice>();
			int num = PortAudioAPI.Pa_GetDefaultInputDevice();
			int num2 = PortAudioAPI.Pa_GetDefaultOutputDevice();
			int num3 = PortAudioAPI.Pa_GetDeviceCount();
			for (int i = 0; i < num3; i++)
			{
				PaDeviceInfo paDeviceInfo = PortAudioAPI.Pa_GetDeviceInfo(i);
				DeviceDirection deviceDirection = (paDeviceInfo.maxInputChannels <= 0) ? DeviceDirection.Output : ((paDeviceInfo.maxOutputChannels > 0) ? DeviceDirection.InputOutput : DeviceDirection.Input);
				if (deviceDirection == direction || deviceDirection == DeviceDirection.InputOutput)
				{
					PaHostApiInfo paHostApiInfo = PortAudioAPI.Pa_GetHostApiInfo(paDeviceInfo.hostApi);
					AudioDevice audioDevice = new AudioDevice();
					audioDevice.Name = paDeviceInfo.name;
					audioDevice.Host = paHostApiInfo.name;
					audioDevice.Index = i;
					audioDevice.Direction = deviceDirection;
					audioDevice.IsDefault = (i == num || i == num2);
					list.Add(audioDevice);
				}
			}
			return list;
		}

		public override string ToString()
		{
			return "[" + this.Host + "] " + this.Name;
		}
	}
}
