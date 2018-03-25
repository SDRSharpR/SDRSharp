using PortAudioSharp;
using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio.PortAudio
{
	public class WavePlayer : IDisposable
	{
		private IntPtr _streamHandle;

		private GCHandle _gcHandle;

		private readonly AudioBufferNeededDelegate _bufferNeeded;

		private unsafe readonly PaStreamCallbackDelegate _paCallback = WavePlayer.PaStreamCallback;

		public unsafe WavePlayer(int deviceIndex, double sampleRate, int framesPerBuffer, AudioBufferNeededDelegate bufferNeededDelegate)
		{
			this._bufferNeeded = bufferNeededDelegate;
			PaStreamParameters paStreamParameters = new PaStreamParameters
			{
				device = deviceIndex,
				channelCount = 2,
				suggestedLatency = 0.0,
				sampleFormat = PaSampleFormat.PaFloat32
			};
			PaError paError = PortAudioAPI.Pa_IsFormatSupported(IntPtr.Zero, ref paStreamParameters, sampleRate);
			if (paError != 0)
			{
				throw new ApplicationException(paError.ToString());
			}
			this._gcHandle = GCHandle.Alloc(this);
			paError = PortAudioAPI.Pa_OpenStream(out this._streamHandle, IntPtr.Zero, ref paStreamParameters, sampleRate, (uint)framesPerBuffer, PaStreamFlags.PaNoFlag, this._paCallback, (IntPtr)this._gcHandle);
			if (paError != 0)
			{
				this._gcHandle.Free();
				throw new ApplicationException(paError.ToString());
			}
			paError = PortAudioAPI.Pa_StartStream(this._streamHandle);
			if (paError == PaError.paNoError)
			{
				return;
			}
			PortAudioAPI.Pa_CloseStream(this._streamHandle);
			this._gcHandle.Free();
			throw new ApplicationException(paError.ToString());
		}

		private unsafe static PaStreamCallbackResult PaStreamCallback(float* input, float* output, uint frameCount, ref PaStreamCallbackTimeInfo timeInfo, PaStreamCallbackFlags statusFlags, IntPtr userData)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(userData);
			if (!gCHandle.IsAllocated)
			{
				return PaStreamCallbackResult.PaAbort;
			}
			WavePlayer wavePlayer = (WavePlayer)gCHandle.Target;
			try
			{
				if (wavePlayer._bufferNeeded != null)
				{
					wavePlayer._bufferNeeded(output, (int)frameCount);
				}
			}
			catch
			{
				return PaStreamCallbackResult.PaAbort;
			}
			return PaStreamCallbackResult.PaContinue;
		}

		public void Dispose()
		{
			if (this._streamHandle != IntPtr.Zero)
			{
				PortAudioAPI.Pa_StopStream(this._streamHandle);
				PortAudioAPI.Pa_CloseStream(this._streamHandle);
				this._streamHandle = IntPtr.Zero;
			}
			this._gcHandle.Free();
		}
	}
}
