/** 
* Scorpio
* @author JiangHao 
* @date 2019.3
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace VeryFS.Framework.Runtime.Utilities
{

	
	
	public class BytesReader : BytesData
	{
		public BytesReader(ref byte[] ptr, int len)
		{
			pos = 0;
			data = ptr;
			length = len;
			
		}

		public void Reset(int len=-1)
		{
			pos = 0;
			if (len > 0)
			{
				if (len > data.Length)
					throw new BytesReadException(this, $"Reset({len})");
				length = len;
			}
		}

		public byte ReadUint8()
		{
			if (length - pos < 1)
			{
				throw new BytesReadException(this,"ReadUInt8");
			}

			return data[pos++];
		}

		public sbyte ReadInt8()
		{
			if (length - pos < 1)
			{
				throw new BytesReadException(this,"ReadInt8");
			}

			return (sbyte) data[pos++];
		}

		public short ReadInt16()
		{
			if (length - pos < 2)
			{
				throw new BytesReadException(this,"ReadInt16");
			}

			short dest = BitConverter.ToInt16(data, pos);
			pos += 2;
			if (!BitConverter.IsLittleEndian)
			{
				dest = (short) ReverseUInt16((ushort) dest);
			}
			return dest;
		}

		public void Skip(int step)
		{
			if (length - pos < step)
			{
				throw new BytesReadException(this,"Skip");
			}
			pos += step;
		}

		public ushort ReadUint16()
		{
			if (length - pos < 2)
			{
				throw new BytesReadException(this,"ReadUInt16");
			}

			ushort dest = BitConverter.ToUInt16(data, pos);
			pos += 2;
			if (!BitConverter.IsLittleEndian)
			{
				dest = ReverseUInt16(dest);
			}
			return dest;
		}

		public int ReadInt32()
		{
			if (length - pos < 4)
			{
				throw new BytesReadException(this,"ReadInt32");
			}

			int dest = BitConverter.ToInt32(data, pos);
			pos += 4;
			if (!BitConverter.IsLittleEndian)
			{
				dest = (int) ReverseUInt32((uint) dest);
			}
			return dest;
		}

		public uint ReadUint32()
		{
			if (length - pos < 4)
			{
				throw new BytesReadException(this,"ReadUInt32");
			}

			uint dest = BitConverter.ToUInt32(data, pos);
			pos += 4;
			if (!BitConverter.IsLittleEndian)
			{
				dest = ReverseUInt32(dest);
			}
			return dest;
		}


		public long ReadInt64()
		{
			if (length - pos < 8)
			{
				throw new BytesReadException(this,"ReadInt64");
			}

			long dest = BitConverter.ToInt64(data, pos);
			pos += 8;
			if (!BitConverter.IsLittleEndian)
			{
				dest = (long) ReverseUInt64((ulong) dest);
			}

			return dest;
		}

		public ulong ReadUint64()
		{
			if (length - pos < 8)
			{
				throw new BytesReadException(this,"ReadUInt64");
			}

			ulong dest = BitConverter.ToUInt64(data, pos);
			pos += 8;
			if (!BitConverter.IsLittleEndian)
			{
				dest = ReverseUInt64(dest);
			}
			return dest;
		}
		
		public float ReadFloat()
		{
			if (length - pos < 4)
			{
				throw new BytesReadException(this,"ReadFloat");
			}
			float dest = BitConverter.ToSingle(data, pos);
			pos += 4;
			return dest;
		}

		public double ReadDouble()
		{
			if (length - pos < 8)
			{
				throw new BytesReadException(this,"ReadDouble");
			}

			double dest = BitConverter.ToDouble(data, pos);
			pos += 8;
			return dest;
		}

		public int ReadCount()
		{
			short cnt = ReadInt16();
			return cnt;
		}

		public bool ReadBool()
		{
			sbyte v = ReadInt8();
			return  v != 0;
		}

		public string ReadString()
		{
			ushort len = ReadUint16();
			if (length - pos < len)
			{
				pos -= 2;
				throw new BytesReadException(this,"ReadString");
			}
			if (len == 0)
			{
				return string.Empty;
			}
			string dest = Encoding.UTF8.GetString(data, pos, len);
			pos += len;
			return dest;
		}

		public T ReadEnum<T>()
		{
			Int32 enumValue = ReadInt32();
			T dest = (T)Enum.ToObject(typeof(T), enumValue);
			return dest;
		}

		public T[] ReadSerializableArray<T>() where T : IBinarySerializable, new()
		{
			short cnt = ReadInt16();
			if (cnt < 0)
			{
				throw new BytesReadException(this, "ReadArrayT,cnt=" + cnt);
			}

			T[] array = new T[cnt];
			for (int i = 0; i < cnt; i++)
			{
				T item = new T();
				item.Deserialize(this);
				array[i] = item;
			}

			return array;
		}

		public List<T> ReadSerializableList<T>() where T : IBinarySerializable, new()
		{
			short cnt = ReadInt16();
			if (cnt < 0)
			{
				throw new BytesReadException(this, "ReadArrayT,cnt=" + cnt);
			}

			List<T> array = new();
			for (int i = 0; i < cnt; i++)
			{
				T item = new T();
				item.Deserialize(this);
				array.Add(item);
			}

			return array;
		}

		public char ReadChar()
		{
			byte a=ReadUint8();
			byte b=ReadUint8();
			return (char) (((a & 0xFF) << 8) | (b & 0xFF));
		}

		public int ReadBytes(byte[] bytes)
		{
			int cnt = ReadInt32();
			if (cnt <= 0)
			{
				return cnt;
			}
			
			if (bytes.Length<cnt || length - pos < cnt)
			{
				pos -= 4;
				throw new BytesReadException(this,"ReadBytes,overflow");
			}
			Array.Copy(this.data,pos, bytes,0,cnt);
			pos += cnt;
			return cnt;
		}
		
		
		/*public DFloat ReadDFloat()
		{
			uint u = ReadUint32();
			DFloat dest = new DFloat();
			dest.u = u;
			return dest;
		}

		public DQuaternion ReadDQuaternion()
		{
			DQuaternion q = new DQuaternion(ReadDFloat(),ReadDFloat(),ReadDFloat(),ReadDFloat());
			return q;
		}
		

		public int ReadDFloatArray(DFloat[] array)
		{
			ushort cnt = ReadUint16();
			if (cnt > array.Length)
			{
				throw new BytesReadException(this,"ReadDFloatArray. "+cnt +" > buffersize.");
			}

			for (int i = 0; i < cnt; i++)
			{
				array[i] = ReadDFloat();
			}
			return cnt;
		}
		
		public DFloat[] ReadDFloatArray()
		{
			ushort cnt = ReadUint16();
			DFloat[] array=new DFloat[cnt];
			for (int i = 0; i < cnt; i++)
			{
				array[i] = ReadDFloat();
			}
			return array;
		}

		public DVector3 ReadDVector3()
		{
			DVector3 vec = new DVector3(ReadDFloat(), ReadDFloat(), ReadDFloat());
			return vec;
		}
		
		public int ReadDVector3Array(DVector3[] array)
		{
			ushort cnt = ReadUint16();
			if (cnt > array.Length)
			{
				throw new BytesReadException(this,"ReadDVector3Array. "+cnt +" > buffersize.");
			}
			
			for (int i = 0; i < cnt; i++)
			{
				array[i] = ReadDVector3();
			}
			return cnt;
		}
		
		public DVector3[]  ReadDVector3Array()
		{
			ushort cnt = ReadUint16();
			DVector3[] array=new DVector3[cnt];
			for (int i = 0; i < cnt; i++)
			{
				array[i] = ReadDVector3();
			}
			return array;
		}*/
	}

	public class BytesReadException : Exception
	{
		public BytesReadException(BytesReader reader, string msg)
			:base(msg + ", pos="+reader.Position+", size="+reader.Size)
		{
		}
	}
}

