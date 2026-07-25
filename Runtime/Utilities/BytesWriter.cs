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
    public class BytesWriter: BytesData
    {
		public BytesWriter(ref byte[] ptr, int len)
		{
			pos = 0;
			data = ptr;
			length = len;
		}
		
		public BytesWriter(int size)
		{
			pos = 0;
			data = new byte[size];
			length = size;
		}
	
		public void Reset(/*int len=0*/)
		{
			pos = 0;
			// if (len > 0)
			// 	length = len;
		}
	
		
	
		public void WriteUint8(byte dest)
		{
			if (length - pos < 1)
			{
				throw new BytesWriteException(this, "WriteUInt8");
			}
			data[pos++] = dest  ;
		}
	
		public void WriteInt8(sbyte dest)
		{
			if (length - pos < 1)
			{
				throw new BytesWriteException(this, "WriteInt8");
			}
			data[pos++] = (byte) dest;
		}
	
		public void WriteBool(bool dest)
		{
			if (length - pos < 1)
			{
				throw new BytesWriteException(this, "WriteBool");
			}
			data[pos++] = (byte)(dest ? 1 :0 );
		}
	
		public void WriteUint16(ushort src)
		{
			if (length - pos < 2)
			{
				throw new BytesWriteException(this, "WriteUInt16");
			}
//			if (!BitConverter.IsLittleEndian)
//			{
//				src = ReverseUInt16(src);
//			}
			
//			byte[] bytes = BitConverter.GetBytes(src);
//			for (int i = 0; i < bytes.GetLength(0); i++)
//			{
//				this.data[pos++] = bytes[i];
//			}
			FastBitConverter.GetBytes(this.data, pos, src);
			pos += 2;
		}
		
		public void ModifyUint16(ushort src, int thePos)
		{
			if (length - thePos < 2)
			{
				throw new BytesWriteException(this, "ModifyUInt16");
			}
//			if (!BitConverter.IsLittleEndian)
//			{
//				src = ReverseUInt16(src);
//			}
			//byte[] bytes = BitConverter.GetBytes(src);
			//Array.Copy(bytes,0,this.data,thePos,bytes.GetLength(0));
			
			FastBitConverter.GetBytes(this.data, thePos, src);
			
		}
	
		public void WriteInt16(short src)
		{
			if (length - pos < 2)
			{
				throw new BytesWriteException(this, "WriteInt16");
			}
//			if (!BitConverter.IsLittleEndian)
//			{
//				src = (short)ReverseUInt16((ushort)src);
//			}
//			byte[] bytes = BitConverter.GetBytes(src);
//			for (int i = 0; i < bytes.GetLength(0); i++)
//			{
//				this.data[pos++] = bytes[i];
//			}
			FastBitConverter.GetBytes(this.data, pos, src);
			pos += 2;
		}
	
		public void WriteUint32(uint src)
		{
			if (length - pos < 4)
			{
				throw new BytesWriteException(this, "WriteUInt32");
			}
			FastBitConverter.GetBytes(this.data, pos, src);
			pos += 4;
//			if (!BitConverter.IsLittleEndian)
//			{
//				src = ReverseUInt32(src);
//			}
//			byte[] bytes = BitConverter.GetBytes(src);
//			for (int i = 0; i < bytes.GetLength(0); i++)
//			{
//				this.data[pos++] = bytes[i];
//			}
		}

		public void ModifyUint32(uint src, int thePos)
		{
			if (length - thePos < 4)
			{
				throw new BytesWriteException(this, "ModifyUInt32, thePos=" + thePos);
			}
//			if (!BitConverter.IsLittleEndian)
//			{
//				src = ReverseUInt32(src);
//			}
//			byte[] bytes = BitConverter.GetBytes(src);
//			Array.Copy(bytes,0,this.data,thePos,bytes.GetLength(0));
			FastBitConverter.GetBytes(this.data, thePos, src);

		}
	
		public void WriteInt32(int src)
		{
			if (length - pos < 4)
			{
				throw new BytesWriteException(this, "WriteInt32");
			}
//			if (!BitConverter.IsLittleEndian)
//			{
//				src = (int)ReverseUInt32((uint)src);
//			}
//			byte[] bytes = BitConverter.GetBytes(src);
//			for (int i = 0; i < bytes.GetLength(0); i++)
//			{
//				this.data[pos++] = bytes[i];
//			}
			FastBitConverter.GetBytes(this.data, pos, src);
			pos += 4;
		}
	
		public void WriteInt64(long src)
		{
			if (length - pos < 8)
			{
				throw new BytesWriteException(this, "WriteInt64");
			}
//			if (!BitConverter.IsLittleEndian)
//			{
//				src = (long)ReverseUInt64((ulong)src);
//			}
//			byte[] bytes = BitConverter.GetBytes(src);
//			for (int i = 0; i < bytes.GetLength(0); i++)
//			{
//				this.data[pos++] = bytes[i];
//			}
			FastBitConverter.GetBytes(this.data, pos, src);
			pos += 8;
		}
	
		public void WriteUint64(ulong src)
		{
			if (length - pos < 8)
			{
				throw new BytesWriteException(this, "WriteUInt64");
			}
//			if (!BitConverter.IsLittleEndian)
//			{
//				src = ReverseUInt64(src);
//			}
//			byte[] bytes = BitConverter.GetBytes(src);
//			for (int i = 0; i < bytes.GetLength(0); i++)
//			{
//				this.data[pos++] = bytes[i];
//			}
			FastBitConverter.GetBytes(this.data, pos, src);
			pos += 8;
		}
	
		public void WriteFloat(float src)
		{
			if (length - pos < 4)
			{
				throw new BytesWriteException(this, "WriteFloat");
			}
//			byte[] bytes = BitConverter.GetBytes(src);
//			//Debug.LogError("write: "+ BitConverter.ToString(bytes) );
//			Array.Copy(bytes,0,this.data,pos,4);
//			pos += 4;
			FastBitConverter.GetBytes(this.data, pos, src);
			pos += 4;
		}
	    
	    /*public void WriteDFloat(DFloat src)
	    {
		    WriteUint32(src.u);
	    }
	    
	    public void WriteDFloat(float src)
	    {
		    WriteDFloat(new DFloat(src));
	    }
	    
	    public void WriteDFloatArray(DFloat[] array,int count=0)
	    {
		    if (count > array.Length)
		    {
			    throw new BytesWriteException(this, "WriteDFloatArray");
		    }
		    ushort cnt = (ushort) (count==0 ? array.Length : count);
		    WriteUint16(cnt);
		    
		    for (int i = 0; i < cnt; i++)
		    {
			    WriteUint32(array[i].u);
		    }
	    }*/
	    
	    public void WriteBytes(byte[] bytes,int count=0)
	    {
		    if (count > bytes.Length)
		    {
			    throw new BytesWriteException(this, "WriteBytes");
		    }
		    
		    int cnt =  count==0  ? bytes.Length : count;
		    
		    if (length - pos < cnt + 4)
		    {
			    throw new BytesWriteException(this, "WriteBytes");
		    }

		    WriteInt32(cnt);
		    
		    Array.Copy(bytes,0,this.data,pos,cnt);
		    pos += cnt;
	    }

	    /*public void WriteDVector3(DVector3 vec)
	    {
		    WriteDFloat(vec.x);
		    WriteDFloat(vec.y);
		    WriteDFloat(vec.z);
	    }

	    public void WriteDQuaternion(DQuaternion q)
	    {
		    WriteDFloat(q.x);
		    WriteDFloat(q.y);
		    WriteDFloat(q.z);
		    WriteDFloat(q.w);
	    }

	    public void WriteDVector3Array(DVector3[] array,int count=0)
	    {
		    ushort cnt = (ushort) (count==0 || count > array.Length ? array.Length : count);
		    WriteUint16(cnt);
		    for (int i = 0; i < cnt; i++)
		    {
			    WriteDVector3(array[i]);
		    }
	    }*/
	    
		public void WriteDouble(double src)
		{
			if (length - pos < 8)
			{
				throw new BytesWriteException(this, "WriteDouble");
			}
	
//			byte[] bytes = BitConverter.GetBytes(src);
//			for (int i = 0; i < bytes.GetLength(0); i++)
//			{
//				this.data[pos++] = bytes[i];
//			}
			FastBitConverter.GetBytes(this.data, pos, src);
			pos += 8;
		}

	
		public void WriteString(string src)
		{
			if (src == null)
			{
				WriteUint16(0);
				return;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(src);
			var bytesLen = bytes.GetLength(0);

			if (bytesLen > ushort.MaxValue)
			{
				throw new BytesWriteException(this, $"WriteString: string too long ({bytesLen} bytes)");
			}
			if (length - pos < bytesLen + 2)   // 含 2 字节长度前缀，原子检查，避免写半截损坏流
			{
				throw new BytesWriteException(this, "WriteString");
			}
			WriteUint16((ushort) bytesLen);
			Array.Copy(bytes, 0, data, pos, bytesLen);
			pos += bytesLen;
		}

	    public void WriteEnum<T>(T value)
	    {
			WriteInt32(Convert.ToInt32(value));
	    }

	    public void WriteChar(char c)
	    {
		    byte a = (byte) ((c & 0xFF00) >> 8); 
		    byte b = (byte) (c & 0xFF);
		    WriteUint8(a);
		    WriteUint8(b);
	    }

		public byte[] ToArray()
		{
			byte[] ret = new byte[Position];
			for (int i = 0; i < Position; i++)
			{
				ret[i] = data[i];
			}
			return ret;
		}

		public void WriteSerializableArray<T>(T[] array) where T : IBinarySerializable
		{
			int cnt = array != null ? array.Length : 0;
			if (cnt > short.MaxValue)
			{
				throw new BytesWriteException(this, $"WriteSerializableArray: too many elements ({cnt})");
			}
			WriteInt16((short)cnt);
			for (int i = 0; i < cnt; i++)
			{
				array[i].Serialize(this);
			}
		}

		public void WriteSerializableList<T>(List<T> list) where T : IBinarySerializable
		{
			int cnt = list != null ? list.Count : 0;
			if (cnt > short.MaxValue)
			{
				throw new BytesWriteException(this, $"WriteSerializableList: too many elements ({cnt})");
			}
			WriteInt16((short)cnt);
			for (int i = 0; i < cnt; i++)
			{
				list[i].Serialize(this);
			}
		}

		public void WriteCount(int cnt)
		{
			short i = (short)(cnt < 0 ? 0 : cnt);
			WriteInt16(i);
		}

		// public void Write(string s)
		// {
		// 	this.WriteString(s);
		// }
		//
		//
		// public void Write(sbyte i)
		// {
		// 	this.WriteInt8(i);
		// }
		//
		// public void Write(Int16 i)
		// {
		// 	this.WriteInt16(i);
		// }
		//
		// public void Write(Int32 i)
		// {
		// 	this.WriteInt32(i);
		// }
		//
		// public void Write(Int64 i)
		// {
		// 	this.WriteInt64(i);
		// }
		//
		//
		// public void Write(byte i)
		// {
		// 	this.WriteUint8(i);
		// }
		//
		// public void Write(UInt16 i)
		// {
		// 	this.WriteUint16(i);
		// }
		//
		// public void Write(UInt32 i)
		// {
		// 	this.WriteUint32(i);
		// }
		//
		// public void Write(UInt64 i)
		// {
		// 	this.WriteUint64(i);
		// }
		//
		// public void Write(float i)
		// {
		// 	this.WriteFloat(i);
		// }
		//
		// public void Write(double i)
		// {
		// 	this.WriteDouble(i);
		// }
		//
		// public void Write(IBinarySerializable o)
		// {
		// 	 o.Serialize(this);
		// }


		public class BytesWriteException : Exception
	    {
		    public BytesWriteException(BytesWriter writer, string msg)
			    :base(msg + ", pos="+writer.Position+", size="+writer.Size)
		    {
		    }
	    }
    }

}