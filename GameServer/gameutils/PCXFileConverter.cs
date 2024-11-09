// Author: Nydirac
// Date: 09/11/2016
// Description: Parsing the **** PCX files....

using System.Drawing;
using System.IO;

namespace DOL.GS
{
    public class PCXFile
    {
        public static Bitmap GetBitmap(string path)
        {
            // Simplified conversion, for full details about PCX file refer to full spec below
            // http://www.fileformat.info/format/pcx/spec/a10e75307b3a4cc49c3bbe6db4c41fa2/view.htm

            // FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);


            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] headerBuffer = new byte[128];
                stream.Read(headerBuffer, 0, 128);
               

                byte Manufacturer = headerBuffer[0]; // should always be 0Ah
                byte Version = headerBuffer[1];
                byte Encoding = headerBuffer[2];
                byte BitsPerPixel = headerBuffer[3];
                ushort XMin = (ushort)(headerBuffer[4] | headerBuffer[5] << 8);
                ushort YMin = (ushort)(headerBuffer[6] | headerBuffer[7] << 8);
                ushort XMax = (ushort)(headerBuffer[8] | headerBuffer[9] << 8);
                ushort YMax = (ushort)(headerBuffer[10] | headerBuffer[11] << 8);
                ushort HorDPI = (ushort)(headerBuffer[12] | headerBuffer[13] << 8);
                ushort VertDPI = (ushort)(headerBuffer[14] | headerBuffer[15] << 8);

                // Palettes: 16 RGB tribles... dont care for this

                byte Reserved = headerBuffer[64]; // MUST be 0
                                                  //could check integrity...

                byte NPlanes = headerBuffer[65];
                ushort BytesPerLine = (ushort)(headerBuffer[66] | headerBuffer[67] << 8);
                ushort PaletteType = (ushort)(headerBuffer[68] | headerBuffer[69] << 8);

                ushort imgWidth = (ushort)(XMax - XMin + 1);
                ushort imgHeight = (ushort)(YMax - YMin + 1);

                int ScanLineLenght = NPlanes * BytesPerLine;

                Bitmap image = new Bitmap(imgWidth, imgHeight);

                for (ushort cY = 0; cY < imgHeight; cY++) // Scan lines
                {
                    ushort cX = 0;
                    for (ushort clb = 0; clb < ScanLineLenght; clb++)
                    {
                        byte b = (byte)stream.ReadByte();

                        if ((b & 0xC0) == 0xC0)             /* 2-byte code */
                        {
                            byte replicateColor = (byte)(b & 0x3F);
                            b = (byte)stream.ReadByte(); //get actual color to replicate 

                            for (int pos = 0; pos < replicateColor; pos++)
                            {
                                image.SetPixel(cX, cY, Color.FromArgb(b, b, b));
                                cX++;
                            }
                        }
                        else                                   /* 1-byte code */
                        {
                            image.SetPixel(cX, cY, Color.FromArgb(b, b, b));
                            cX++;
                        }

                        if (cX == imgWidth)
                            break;
                    }
                }
                stream.Close();

                return image;
            }
        }
    }
}