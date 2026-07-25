/** 
* Scorpio
* @author JiangHao 
* @date 2019.3
*/ 
namespace VeryFS.Framework.Runtime.Utilities
{

    public class BytesData
    {
        protected byte[] data;
        protected int length;
        protected int pos;

        // 翻转字节顺序 (32-bit)
        public static uint ReverseUInt32(uint val)
        {
            return (val & 0x000000FFU) << 24 | (val & 0x0000FF00U) << 8 |
                   (val & 0x00FF0000U) >> 8 | (val & 0xFF000000U) >> 24;
        }

        // 翻转字节顺序 (16-bit)
        public static ushort ReverseUInt16(ushort value)
        {
            return (ushort) ((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        // 翻转字节顺序 (64-bit)
        public static ulong ReverseUInt64(ulong value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }

        public byte[] GetBuffer()
        {
            return data;
        }
        
        public int Position
        {
            get 
            {
                return pos;
            }
        }
        
        public int Size
        {
            get { return this.length; }
        }
        
    }
}