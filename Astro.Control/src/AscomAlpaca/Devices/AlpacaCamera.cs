using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface for controlling cameras via ASCOM Alpaca
/// </summary>
public class AlpacaCamera : AlpacaDevice, ICamera {
        
    internal AlpacaCamera(AlpacaConnection conn, AlpacaConfiguredDevicesResponse.ConfiguredDevice rawDevice) : base(conn, rawDevice.DeviceName, rawDevice.DeviceType, rawDevice.DeviceNumber) {}
    public AlpacaCamera(AlpacaConnection conn, string deviceName, string deviceType, int deviceNumber) : base(conn, deviceName, deviceType, deviceNumber) {}

    public bool IsCoolingEnabled {
        get {
            var res = Get<AlpacaValueResponse<bool>>($"{Connection.Server.Host}:{Connection.Server.Port}/camera/{DeviceNumber}/cooleron");
            return res?.Value ?? false;
        }
        set {
           Put<AlpacaMethodResponse>($"{Connection.Server.Host}:{Connection.Server.Port}/camera/{DeviceNumber}/cooleron", new KeyValuePair<string, string>("CoolerOn", value.ToString())); 
        }
    }
    public void EnableCooling() {
        IsCoolingEnabled = true;
    }

    public void DisableCooling() {
        IsCoolingEnabled = false;
    }


    public Task<ICameraImage> ExposeAsync(Duration timespan, Binning binning) {
        return Task.Run(() => ExposeSync(timespan, binning));
    }

    private object shutter = new object();
    private ICameraImage ExposeSync(Duration timespan, Binning binning) {
        lock (shutter) {
            // Set binning
            Put<AlpacaMethodResponse>($"{Connection.Server.Host}:{Connection.Server.Port}/camera/{DeviceNumber}/binx", new KeyValuePair<string, string>("BinX ", binning.Horizontal.ToString())); 
            Put<AlpacaMethodResponse>($"{Connection.Server.Host}:{Connection.Server.Port}/camera/{DeviceNumber}/biny", new KeyValuePair<string, string>("BinY ", binning.Vertical.ToString()));

            // Expose
            Put<AlpacaMethodResponse>($"{Connection.Server.Host}:{Connection.Server.Port}/camera/{DeviceNumber}/startexposure", 
                new KeyValuePair<string, string>("Duration", ((double)timespan.TotalSeconds()).ToString()),
                new KeyValuePair<string, string>("Light", true.ToString())
            ); 

            // Wait until done
            while (true) {
                Task.Delay(500).Wait();
                var ready = Get<AlpacaValueResponse<bool>>($"{Connection.Server.Host}:{Connection.Server.Port}/camera/{DeviceNumber}/imageready");
                if (ready != null && ready.Value) {
                    break;
                }
            }

            // Get image
            var response = Get<AlpacaImageArrayResponse>($"{Connection.Server.Host}:{Connection.Server.Port}/camera/{DeviceNumber}/imagearray");
            if (response.IsError || (!response.HasColourData && !response.HasMonochromeData)) {
                throw new System.Exception("Image response contained no data");
            }
            
            if (response.HasColourData) {
                return new AlpacaColourImage(response.ColourPixels);    
            } else {
                return new AlpacaMonoImage(response.MonochromePixels);
            }
        }
    }
}

public abstract class AlpacaImage : ICameraImage {
    public int Width {get; private set;}
    public int Height {get; private set;}

    public AlpacaImage(int width, int height) {
        this.Width = width;
        this.Height = height;
    }

    /// <summary>
    /// Save this image data to a file
    /// </summary>
    /// <param name="path">filepath</param>
    /// <returns>name of the saved file</returns>
    public string SaveFile(string path) {
        if (!path.EndsWith(".bmp"))
            path += ".bmp";
        using (var fs = new FileStream(path, FileMode.Create)) 
        using (var writer = new BinaryWriter(fs)) {
            this.Bmp(writer);
        }
        return path;
    }

    private byte[] toLittleEndian(int value) {
        byte[] bytes = BitConverter.GetBytes(value);
        //Then, if we need big endian for our protocol for instance,
        //Just check if you need to convert it or not:
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes); //reverse it so we get big endian.
        return bytes;
    }
    private byte[] toLittleEndianUnsigned(ushort value) {
        byte[] bytes = BitConverter.GetBytes(value);
        //Then, if we need big endian for our protocol for instance,
        //Just check if you need to convert it or not:
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes); //reverse it so we get big endian.
        return bytes;
    }
    private void writeLittleEndian(byte[] array, int offset, int value) {
        var size = toLittleEndian(value);
        array[offset + 0] = size[0]; 
        array[offset + 1] = size[1]; 
        array[offset + 2] = size[2]; 
        array[offset + 3] = size[3];
    }
    protected void Bmp(BinaryWriter writer) {
        // File Type Data
        {
            var ftd = new byte[14];

            // BM in ASCII (2 bytes)
            ftd[0] = 0x42; ftd[1] = 0x4D;
            // File size; total number of bytes in file (4 bytes)
            var size = toLittleEndian(0);
            ftd[2] = size[0]; ftd[3] = size[1]; ftd[4] = size[2]; ftd[5] = size[3];
            // Reserved (2 bytes)
            ftd[6] = 0; ftd[7] = 0;
            // Reserved (2 bytes)
            ftd[8] = 0; ftd[9] = 0;
            // Pixel data offset (4 bytes)
            var offset = toLittleEndian(54); // 54 is the total sum of 14byte file  type data header + 40 byte image data header
            ftd[10] = offset[0]; ftd[11] = offset[1]; ftd[12] = offset[2]; ftd[13] = offset[3];

            writer.Write(ftd);
        }

        // Image Information Data
        {
            var info = new byte[40];

            // Header size (4 bytes)
            writeLittleEndian(info, 0, info.Length); //0-3

            // Image width (4 bytes)
            writeLittleEndian(info, 4, this.Width);  //4-7

            // Image height (4 bytes)
            writeLittleEndian(info, 8, this.Height); //8-11

            // Number of planes (2 bytes)
            info[12] = 0x01; info[13] = 0x00;        // 1 plane

            // Bits per pixel (2 bytes)
            info[14] = 0x18; info[15] = 0x00;        // 24 bits per pixel (rgb)

            // Compression (4 bytes)
            // Skip (leave at 0)

            // Image size (4 bytes)
            // Skip (leave at 0)

            // x pixels per meter (4 bytes)
            // Skip (leave at 0)

            // y pixels per meter (4 bytes)
            // Skip (leave at 0)

            // total colours (4 bytes)
            // Skip (leave at 0)

            // important colours (4 bytes)
            // Skip (leave at 0)

            writer.Write(info);
        }

        // Colour Pallet
        // Skip

        // Raw Pixel Data
        // Since bit-depth is 24, 3 bytes are used to represent BGR color in order
        // Data is ordered by row, bottom up scanning
        for (var row = this.Height - 1; row >= 0; row--) {
            for (var column = 0; column < this.Width; column++) {
                var pixel = this.GetPixel(row, column);
                writer.Write(pixel.B);
                writer.Write(pixel.G);
                writer.Write(pixel.R);
            }
        }
    }

    protected struct Pixel {
        public byte R;
        public byte G;
        public byte B;
    }
    protected abstract Pixel GetPixel(int row, int column);
}

public class AlpacaMonoImage : AlpacaImage {
    
    private double[][] columnMajorPixels;

    public AlpacaMonoImage(double[][] columnMajorPixels) : base(columnMajorPixels.Length, columnMajorPixels[0].Length) {
        this.columnMajorPixels = columnMajorPixels;
    }

    protected override Pixel GetPixel(int rowIdx, int columnIdx) {
        if (columnIdx < 0 || columnIdx >= columnMajorPixels.Length) {
            return new Pixel {R = 0, G = 0, B = 0};
        }
        double[] column = columnMajorPixels[columnIdx];
        if (rowIdx < 0 || rowIdx >= column.Length) {
            return new Pixel {R = 0, G = 0, B = 0};
        }
        else {
            // In mono, use the same colour for every pixel (not super compressed but it is still greyscale)
            return new Pixel {R = (byte)column[rowIdx], G = (byte)column[rowIdx], B = (byte)column[rowIdx]};
        }
    }
}

public class AlpacaColourImage : AlpacaImage {
    
    private double[][][] columnMajorPixels;

    public AlpacaColourImage(double[][][] columnMajorPixels) : base(columnMajorPixels.Length, columnMajorPixels[0].Length) {
        this.columnMajorPixels = columnMajorPixels;
    }

    protected override Pixel GetPixel(int rowIdx, int columnIdx) {
        if (columnIdx < 0 || columnIdx >= columnMajorPixels.Length) {
            return new Pixel {R = 0, G = 0, B = 0};
        }
        double[][] column = columnMajorPixels[columnIdx];
        if (rowIdx < 0 || rowIdx >= column.Length) {
            return new Pixel {R = 0, G = 0, B = 0};
        }
        else {
            return new Pixel {R = (byte)column[rowIdx].ElementAtOrDefault(0), G = (byte)column[rowIdx].ElementAtOrDefault(1), B = (byte)column[rowIdx].ElementAtOrDefault(2)};
        }
    }
}

}