using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace EACExtract
{
    public partial class Form1 : Form
    {
        private Dictionary<string, TrackType> m_TrackTypes = new Dictionary<string, TrackType>() {
            { "h264/AVC", TrackType.H264_AVC },
            { "RAW/PCM", TrackType.RAW_PCM },
            { "DTS Master Audio", TrackType.DTSMA },
            { "DTS", TrackType.DTS },
            { "Subtitle (PGS)", TrackType.PGS },
            { "TrueHD/AC3", TrackType.TrueHD_AC3 },
            { "AC3", TrackType.AC3 },
            { "EAC3", TrackType.EAC3 },
            { "MPEGH/ISO/HEVC", TrackType.MPEGH_ISO_HEVC },
            { "FLAC", TrackType.FLAC },
            { "AAC", TrackType.AAC },
            { "Subtitle (SRT)", TrackType.SRT },
        };

        private ManualResetEvent Event = new ManualResetEvent(false);

        public Form1(List<string> filenames)
        {
            InitializeComponent();

            foreach (string file in filenames) {
                this.lstFiles.Items.Add(file);
            }
        }

        public Form1(string filename = "")
        {
            InitializeComponent();

            if (string.Empty != filename) {
                this.lstFiles.Items.Add(filename);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine(Settings.Eac3toFilePath);
            if (this.lstFiles.Items.Count > 0) {
                this.button1.PerformClick();
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) {
                this.lstFiles.Items.Add(file);
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void CallEAC(bool hidden, bool redirectStdout, string arguments, Action<string> exitedCallback, Action<object, DataReceivedEventArgs> stdoutCallback = null)
        {
            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = Settings.Eac3toFilePath,
                Arguments = arguments,
                UseShellExecute = false
            };

            if (hidden) {
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
            }
            if (redirectStdout) {
                psi.RedirectStandardOutput = true;
            }

            Process p = new Process() {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            if (stdoutCallback != null) {
                p.StartInfo.RedirectStandardError = true;
                p.OutputDataReceived += (s, e) => stdoutCallback(s, e);
                p.ErrorDataReceived += (s, e) => stdoutCallback(s, e);
            }

            p.Exited += (s, e) =>
            {
                string stdout = null;
                if (redirectStdout && stdoutCallback == null) {
                    stdout = p.StandardOutput.ReadToEnd().Replace("", "\n");
                }
                exitedCallback(stdout);
            };
            p.Start();
            if (stdoutCallback != null) { p.BeginOutputReadLine(); p.BeginErrorReadLine(); }
        }

        private TrackType StringToTrackType(string info)
        {

            string[] rawInfo = info.Trim().Split(',');
            foreach (var type in m_TrackTypes) {
                if (rawInfo[0] == type.Key) {
                    return type.Value;
                }
            }

            return TrackType.Unknown;
        }

        private void LoadFile(string filename)
        {
            if (string.Empty == filename) return;

            for (int i = this.Controls.Count - 1; i >= 0; i--) {
                Control c = this.Controls[i];
                if (!(c is CheckBox)) continue;
                if (!c.Name.Contains("chkTrack")) continue;
                this.Invoke(new Action(() => this.Controls.RemoveAt(i)));
            }

            this.CallEAC(true, true, string.Format("\"{0}\"", filename), (stdout) => {
                List<string> stdoutList = new List<string>();
                string[] stdoutSplit = stdout.Split('\n');
                for (int i = 1; i < stdoutSplit.Length; i++) {
                    string line = stdoutSplit[i].Trim();
                    if (string.Empty == line) continue;

                    if (Regex.IsMatch(line, @"^\d*?: .*$")) {
                        stdoutList.Add(line);
                    } else {
                        if (stdoutList.Count > 0) {
                            stdoutList[stdoutList.Count - 1] += " " + line;
                        } else {
                            stdoutList.Add(line);
                        }
                    }
                }

                this.lblInfo.Invoke(new Action (() => this.lblInfo.Text = stdoutList[0]));

                int x = 12, y = this.lblInfo.Location.Y + 23;

                for (int i = 1; i < stdoutList.Count; i++) {
                    string strTrackInfo = stdoutList[i];
                    string number = strTrackInfo.Substring(0, strTrackInfo.IndexOf(":"));
                    string info = strTrackInfo.Substring(strTrackInfo.IndexOf(":") + 1);

                    TrackInfo trackInfo = new TrackInfo();
                    trackInfo.Number = Convert.ToInt32(number);
                    trackInfo.Type = this.StringToTrackType(info);
                    if (TrackType.Unknown == trackInfo.Type) {
                        MessageBox.Show(string.Format("不明类型：{0}\r\n文件：{1}", info, filename), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    CheckBox chk = new CheckBox() {
                        Location = new Point(x, y),
                        Name = string.Format("chkTrack{0}", number),
                        Text = string.Format("{0}: {1}", number, info),
                        Tag = trackInfo,

                        Size = new Size(100, 24),
                        AutoSize = true,
                        UseVisualStyleBackColor = true
                    };

                    chk.Checked = trackInfo.DefaultSelected;

                    this.Invoke(new Action(() => this.Controls.Add(chk)));
                    
                    y += 30;
                }

                this.Invoke(new Action(() => { this.Height = y + 50; }));

                if (this.lstFiles.Items.Count > 0) {
                    this.Extract(filename);
                }
            });
        }

        private void LoadFileWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Extract(string filename)
        {
            List<string> args = new List<string>();

            List<Tuple<string, string>> afterExtract = new List<Tuple<string, string>>();

            string filenameWithoutExtension = filename.Substring(0, filename.LastIndexOf('.'));

            foreach (Control c in this.Controls) {
                if (!(c is CheckBox)) continue;
                if (!c.Name.Contains("chkTrack")) continue;
                if (!(c as CheckBox).Checked) continue;

                var trackInfo = (TrackInfo)c.Tag;

                string trackFilename = string.Format("{0}.{1}.{2}", filenameWithoutExtension, trackInfo.Number, trackInfo.Extension);
                args.Add(string.Format("{0}:\"{1}\"", trackInfo.Number, trackFilename));
            }

            this.CallEAC(false, false, string.Format("\"{0}\" {1}", filename, string.Join(" ", args)), (stdout) => {
                ProcessStartInfo psi = null;
                Process p = null;
                foreach (var task in afterExtract) {
                    psi = new ProcessStartInfo();
                    psi.FileName = task.Item1;
                    psi.Arguments = task.Item2;
                    p = new Process();
                    p.StartInfo = psi;
                    p.Start();
                    p.WaitForExit();
                }
                this.Event.Set();
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;
            this.lstFiles.Enabled = false;
            Task.Run(() => {
                for (int i = 0; i < this.lstFiles.Items.Count; i++) {
                    this.Event.Reset();
                    var item = this.lstFiles.Items[i];
                    string filename = item.ToString();
                    this.lstFiles.Invoke(new Action(() => this.lstFiles.SelectedIndex = i));
                    this.LoadFile(filename);
                    this.Event.WaitOne();
                }
                this.button1.Invoke(new Action(() => this.button1.Enabled = true));
                this.lstFiles.Invoke(new Action(() => this.lstFiles.Enabled = true));
            });
        }
    }
}
