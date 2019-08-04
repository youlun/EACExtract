using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;

namespace EACExtract
{
    enum TrackType
    {
        Unknown,
        H264_AVC,
        RAW_PCM,
        DTSMA,
        DTS,
        TrueHD_AC3,
        PGS,
        Chapter,
        MPEGH_ISO_HEVC,
        FLAC,
        AAC
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
                if (Type == TrackType.TrueHD_AC3) return true;
                if (Type == TrackType.MPEGH_ISO_HEVC) return true;
                if (Type == TrackType.FLAC) return true;
                if (Type == TrackType.AAC) return true;
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
                    case TrackType.TrueHD_AC3:
                        return "thd";
                    case TrackType.PGS:
                        return "sup";
                    case TrackType.Chapter:
                        return "txt";
                    case TrackType.MPEGH_ISO_HEVC:
                        return "hevc";
                    case TrackType.FLAC:
                        return "flac";
                    case TrackType.AAC:
                        return "m4a";
                }
                return null;
            }
        }
    }

    static class Settings
    {
        private static string _Eac3toFilePath = Path.Combine(Environment.CurrentDirectory, @"tools\eac3to\eac3to.exe");

        private static string RegistryPath { get; } = @"SOFTWARE\EACExtract";

        public static string Eac3toFilePath
        {
            get
            {
                return _Eac3toFilePath;
            }
            set
            {
                _Eac3toFilePath = value;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath, true)) {
                    if (null == key) {
                        return;
                    }
                    key.SetValue("Eac3toFilePath", value);
                }
            }
        }

        static Settings()
        {
            RegistryKey key = null;

            try {
                key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                if (null == key) {
                    key = Registry.CurrentUser.CreateSubKey(RegistryPath);
                }

                if (!File.Exists(_Eac3toFilePath)) {
                    _Eac3toFilePath = key.GetValue("Eac3toFilePath", string.Empty).ToString();
                }
            } catch {
                Console.WriteLine("注册表打开失败");
            } finally {
                if (key != null) {
                    key.Close();
                    key.Dispose();
                }
            }

            SelectBinariesPath();
        }

        private static string ShowSelectFileDialog(string title)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "eac3to.exe|eac3to.exe";
            dlg.Title = title;
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == DialogResult.OK) {
                return dlg.FileName;
            }
            return null;
        }

        private static void SelectBinariesPath()
        {
            string newFilePath = _Eac3toFilePath;
            while (!File.Exists(newFilePath)) {
                newFilePath = ShowSelectFileDialog($"选择 eac3to.exe");

                if (MessageBox.Show($"确定选对了？选错了可不好改哟\r\n{newFilePath}", "确认",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                    Eac3toFilePath = newFilePath;
                    continue;
                }
            }
        }
    }
}
