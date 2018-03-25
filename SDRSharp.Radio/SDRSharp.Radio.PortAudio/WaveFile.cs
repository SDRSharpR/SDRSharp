using System;
using System.IO;
using System.Text;

namespace SDRSharp.Radio.PortAudio
{
	public sealed class WaveFile : IDisposable
	{
		private unsafe static readonly float* _lutu8;

		private static readonly UnsafeBuffer _lutu8Buffer;

		private unsafe static readonly float* _lut16;

		private static readonly UnsafeBuffer _lut16Buffer;

		private readonly Stream _stream;

		private bool _isPCM;

		private long _dataPos;

		private short _formatTag;

		private int _sampleRate;

		private int _avgBytesPerSec;

		private int _length;

		private short _blockAlign;

		private short _bitsPerSample;

		private UnsafeBuffer _tempBuffer;

		private byte[] _temp;

		private unsafe byte* _tempPtr;

		public long Position
		{
			get
			{
				return this._stream.Position - this._dataPos;
			}
			set
			{
				this._stream.Seek(value + this._dataPos, SeekOrigin.Begin);
			}
		}

		public short FormatTag
		{
			get
			{
				return this._formatTag;
			}
		}

		public int SampleRate
		{
			get
			{
				return this._sampleRate;
			}
		}

		public int AvgBytesPerSec
		{
			get
			{
				return this._avgBytesPerSec;
			}
		}

		public short BlockAlign
		{
			get
			{
				return this._blockAlign;
			}
		}

		public short BitsPerSample
		{
			get
			{
				return this._bitsPerSample;
			}
		}

		public int Length
		{
			get
			{
				return this._length;
			}
		}

		unsafe static WaveFile()
		{
			WaveFile._lutu8Buffer = UnsafeBuffer.Create(256, 4);
			WaveFile._lut16Buffer = UnsafeBuffer.Create(65536, 4);
			WaveFile._lutu8 = (float*)(void*)WaveFile._lutu8Buffer;
			for (int i = 0; i < 256; i++)
			{
				WaveFile._lutu8[i] = (float)(i - 128) * 0.007874016f;
			}
			WaveFile._lut16 = (float*)(void*)WaveFile._lut16Buffer;
			for (int j = 0; j < 65536; j++)
			{
				WaveFile._lut16[j] = (float)(j - 32768) * 3.051851E-05f;
			}
		}

		~WaveFile()
		{
			this.Dispose();
		}

		public WaveFile(string fileName)
		{
			this._stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			this.ReadHeader();
		}

		public void Dispose()
		{
			this.Close();
			GC.SuppressFinalize(this);
		}

		public void Close()
		{
			if (this._stream != null)
			{
				this._stream.Close();
			}
		}

		private static string ReadChunk(BinaryReader reader)
		{
			byte[] array = new byte[4];
			reader.Read(array, 0, array.Length);
			return Encoding.ASCII.GetString(array);
		}

		private void ReadHeader()
		{
			BinaryReader binaryReader = new BinaryReader(this._stream);
			if (WaveFile.ReadChunk(binaryReader) != "RIFF")
			{
				throw new Exception("Invalid file format");
			}
			binaryReader.ReadInt32();
			if (WaveFile.ReadChunk(binaryReader) != "WAVE")
			{
				throw new Exception("Invalid file format");
			}
			if (WaveFile.ReadChunk(binaryReader) != "fmt ")
			{
				throw new Exception("Invalid file format");
			}
			int num = binaryReader.ReadInt32();
			if (num < 16)
			{
				throw new Exception("Invalid file format");
			}
			this._formatTag = binaryReader.ReadInt16();
			this._isPCM = (this._formatTag == 1);
			if (binaryReader.ReadInt16() != 2)
			{
				throw new Exception("Invalid file format");
			}
			this._sampleRate = binaryReader.ReadInt32();
			this._avgBytesPerSec = binaryReader.ReadInt32();
			this._blockAlign = binaryReader.ReadInt16();
			this._bitsPerSample = binaryReader.ReadInt16();
			for (num -= 16; num > 0; num--)
			{
				binaryReader.ReadByte();
			}
			while (this._stream.Position < this._stream.Length && WaveFile.ReadChunk(binaryReader) != "data")
			{
				num = binaryReader.ReadInt32();
				while (this._stream.Position < this._stream.Length && num > 0)
				{
					binaryReader.ReadByte();
					num--;
				}
			}
			if (this._stream.Position >= this._stream.Length)
			{
				throw new Exception("Invalid file format");
			}
			this._length = binaryReader.ReadInt32();
			this._dataPos = this._stream.Position;
		}

		public unsafe void Read(Complex* iqBuffer, int length)
		{
			if (this._temp == null || this._temp.Length != this._blockAlign * length)
			{
				this._temp = new byte[this._blockAlign * length];
				this._tempBuffer = UnsafeBuffer.Create(this._temp);
				this._tempPtr = (byte*)(void*)this._tempBuffer;
			}
			int i = 0;
			int num2;
			for (int length2 = this._tempBuffer.Length; i < length2; i += num2)
			{
				int num = length2 - i;
				num2 = this._stream.Read(this._temp, i, num);
				if (num2 < num)
				{
					this._stream.Position = this._dataPos;
				}
			}
			this.FillIQ(iqBuffer, length);
		}

		private unsafe void FillIQ(Complex* iqPtr, int length)
		{
			if (this._isPCM)
			{
				if (this._blockAlign == 6)
				{
					Int24* ptr = (Int24*)this._tempPtr;
					for (int i = 0; i < length; i++)
					{
						Complex* intPtr = iqPtr;
						Int24* intPtr2 = ptr;
						ptr = intPtr2 + 1;
						intPtr->Real = (float)(*intPtr2) * 1.192093E-07f;
						Complex* intPtr3 = iqPtr;
						Int24* intPtr4 = ptr;
						ptr = intPtr4 + 1;
						intPtr3->Imag = (float)(*intPtr4) * 1.192093E-07f;
						iqPtr++;
					}
				}
				else if (this._blockAlign == 4)
				{
					short* ptr2 = (short*)this._tempPtr;
					for (int j = 0; j < length; j++)
					{
						Complex* intPtr5 = iqPtr;
						float* lut = WaveFile._lut16;
						short* intPtr6 = ptr2;
						ptr2 = intPtr6 + 1;
						intPtr5->Real = lut[*intPtr6 + 32768];
						Complex* intPtr7 = iqPtr;
						float* lut2 = WaveFile._lut16;
						short* intPtr8 = ptr2;
						ptr2 = intPtr8 + 1;
						intPtr7->Imag = lut2[*intPtr8 + 32768];
						iqPtr++;
					}
				}
				else if (this._blockAlign == 2)
				{
					byte* ptr3 = this._tempPtr;
					for (int k = 0; k < length; k++)
					{
						Complex* intPtr9 = iqPtr;
						float* lutu = WaveFile._lutu8;
						byte* intPtr10 = ptr3;
						ptr3 = intPtr10 + 1;
						intPtr9->Real = lutu[(int)(*intPtr10)];
						Complex* intPtr11 = iqPtr;
						float* lutu2 = WaveFile._lutu8;
						byte* intPtr12 = ptr3;
						ptr3 = intPtr12 + 1;
						intPtr11->Imag = lutu2[(int)(*intPtr12)];
						iqPtr++;
					}
				}
			}
			else
			{
				float* ptr4 = (float*)this._tempPtr;
				for (int l = 0; l < length; l++)
				{
					Complex* intPtr13 = iqPtr;
					float* intPtr14 = ptr4;
					ptr4 = intPtr14 + 1;
					intPtr13->Real = *intPtr14;
					Complex* intPtr15 = iqPtr;
					float* intPtr16 = ptr4;
					ptr4 = intPtr16 + 1;
					intPtr15->Imag = *intPtr16;
					iqPtr++;
				}
			}
		}
	}
}
