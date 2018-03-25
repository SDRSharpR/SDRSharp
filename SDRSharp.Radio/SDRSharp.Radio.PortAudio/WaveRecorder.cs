using PortAudioSharp;
using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio.PortAudio
{
	public class WaveRecorder : IDisposable
	{
		private IntPtr _streamHandle;

		private GCHandle _gcHandle;

		private readonly AudioBufferAvailableDelegate _bufferAvailable;

		private unsafe readonly PaStreamCallbackDelegate _paCallback = WaveRecorder.PaStreamCallback;

		public unsafe WaveRecorder(int deviceIndex, double sampleRate, int framesPerBuffer, AudioBufferAvailableDelegate bufferAvailable)
		{
			this._bufferAvailable = bufferAvailable;
			PaStreamParameters paStreamParameters = new PaStreamParameters
			{
				device = deviceIndex,
				channelCount = 2,
				suggestedLatency = 0.0,
				sampleFormat = PaSampleFormat.PaFloat32
			};
			PaError paError = PortAudioAPI.Pa_IsFormatSupported(ref paStreamParameters, IntPtr.Zero, sampleRate);
			if (paError != 0)
			{
				throw new ApplicationException(paError.ToString());
			}
			this._gcHandle = GCHandle.Alloc(this);
			paError = PortAudioAPI.Pa_OpenStream(out this._streamHandle, ref paStreamParameters, IntPtr.Zero, sampleRate, (uint)framesPerBuffer, PaStreamFlags.PaNoFlag, this._paCallback, (IntPtr)this._gcHandle);
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
			WaveRecorder waveRecorder = (WaveRecorder)gCHandle.Target;
			try
			{
				if (waveRecorder._bufferAvailable != null)
				{
					waveRecorder._bufferAvailable(input, (int)frameCount);
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
