using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace VideoHomeStorage.TIB
{
    public static class VHSEncoder
    {
        private enum BitDepth { bit = 1, nibble = 4, byt = 8 }; // byte is reserved

        // Configuration constants
        private static int streamWidth = 480; // Horizontal resolution of frame
        private static int streamHeight = 333; // Vertical resolution of frame

        private static BitDepth bitDepth = BitDepth.nibble; // Number of bits per symbol
        // Block size is 8 symbols
        private static int hBlocks = 2; // Number of data blocks per line
        private static bool pairity = true; // Whether a pairity symbol is included after each data block
        private static int vRows = 1; // Number of rows/lines per frame

        // Derived constants
        private static int numCols = (hBlocks * 8) + (pairity ? hBlocks : 0);
        private static int numRows = vRows;
        private static int symbolWidth = streamWidth / numCols;
        private static int symbolHeight = streamHeight / numRows;
        private static int bytesPerFrame = hBlocks * 8 * vRows * ((int)bitDepth / 8);

        public static Bitmap Encode(byte[] data)
        {
            if (data.Count() > bytesPerFrame)
            {
                throw new ArgumentException("Given " + data.Count() + " bytes. Cannot encode more than " + bytesPerFrame + " bytes per frame!");
            }

            Bitmap bmp = new Bitmap(streamWidth, streamHeight);

            int i_data, i_row, i_col;
            i_row = 0;
            i_col = 0;
            int bytePos = 0; // Used for bit-depths less than byte to keep track of the position in the current bit
            for (i_data = 0; i_data < data.Count() - 1;)
            {
                int val = 0;

                if (i_data % 8 == 0 && i_col != 0 && pairity)
                {
                    // We need to insert a parity symbol here
                    val = calculateParity(data, i_data);
                    fillSymbol(bmp, i_row, i_col, val);
                    i_col++; // We are going to another column
                    if(i_col == numCols - 1)
                    {
                        // We are going to another row
                        i_col = 0;
                        i_row++;
                    }
                    continue;
                }

                switch(bitDepth)
                {
                    case BitDepth.bit:
                        val = calculateValue(data, i_data, bytePos, bitDepth);
                        bytePos++;
                        if(bytePos % 8 == 0)
                        {
                            bytePos = 0;
                            i_data++; // We finished a byte!
                        }
                        break;
                    case BitDepth.nibble:
                        val = calculateValue(data, i_data, bytePos, bitDepth);
                        bytePos++;
                        if (bytePos % 2 == 0)
                        {
                            bytePos = 0;
                            i_data++; // We finished a byte!
                        }
                        break;
                    case BitDepth.byt:
                        val = calculateValue(data, i_data, bytePos, bitDepth);
                        i_data++; // We finished a byte!
                        break;
                    default:
                        throw new ApplicationException("Invalid bit depth! This shouldn't happen...");
                }

                fillSymbol(bmp, i_row, i_col, val);

                i_col++; // We are going to another column
                if (i_col == numCols - 1)
                {
                    // We are going to another row
                    i_col = 0;
                    i_row++;
                }
            }

            return bmp;
        }

        private static int calculateValue(byte[] data, int i_data, int bytePos, BitDepth bitDepth)
        {
            int val;
            switch(bitDepth)
            {
                case BitDepth.bit:
                    BitArray ba = new BitArray(new byte[] { data[i_data] });
                    val = ba[bytePos] ? 65535 : 0;
                    return val * 255;
                case BitDepth.nibble:
                    val = (int)data[i_data];
                    if (bytePos == 0)
                    {
                        val &= 0x0F;
                    }
                    else // bytePos == 1
                    {
                        val = val >> 4;
                    }
                    return val * 17;
                case BitDepth.byt:
                    val = (int)data[i_data];
                    return val;
                default:
                    throw new ApplicationException("Invalid bit depth! This shouldn't happen...");
            }
        }

        private static int calculateParity(byte[] data, int i_data)
        {
            throw new NotImplementedException();
        }

        private static void fillSymbol(Bitmap bmp, int i_row, int i_col, int val)
        {
            throw new NotImplementedException();
        }

        public static void Decode()
        {

        }
    }
}
