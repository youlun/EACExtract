using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EACExtract
{
    enum TrackType {
        Unknown,
        H264_AVC,
        RAW_PCM,
        DTSMA,
        DTS,
        PGS,
        Chapter
    }

    struct TrackInfo
    {
        public TrackType Type { get; set; }
        public int Number { get; set; }

        public bool DefaultSelected
        {
            get
            {
                if (Type == TrackType.RAW_PCM) return true;
                if (Type == TrackType.DTSMA) return true;
                if (Type == TrackType.DTS) return true;
                if (Type == TrackType.PGS) return true;
                if (Type == TrackType.Chapter) return true;
                return false;
            }
        }

        public string Extension
        {
            get
            {
                switch (this.Type) {
                    case TrackType.H264_AVC:
                        return "h264";
                    case TrackType.RAW_PCM:
                    case TrackType.DTSMA:
                        return "flac";
                    case TrackType.DTS:
                        return "dts";
                    case TrackType.PGS:
                        return "sup";
                    case TrackType.Chapter:
                        return "txt";
                }
                return null;
            }
        }
    }

    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (1 == args.Length) {
                Application.Run(new Form1(args[0]));
            } else {
                List<string> files = new List<string>(args);
                Application.Run(new Form1(files));
            }
        }
    }
}
