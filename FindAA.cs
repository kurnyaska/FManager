using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace SPBU12._1MANAGER {
    public partial class FindAA : Form {
        private CancellationTokenSource cancelToken;
        private CancellationToken cancellationToken;
        string personData;
        Form1 parent;
        int count;
        string dName;

        public FindAA(Form1 parent, string path) {
            cancelToken = new CancellationTokenSource();
            cancellationToken = cancelToken.Token;
            this.parent = parent;
            this.dName = path;
            count = 0;
            CountIni(dName);

            InitializeComponent();
        }

        private void CountIni(string path) {
            try {
                DirectoryInfo di = new DirectoryInfo(path);
                DirectoryInfo[] directories = di.GetDirectories();
                FileInfo[] files = di.GetFiles();

                foreach (DirectoryInfo info in directories) {
                    CountIni(path + Path.DirectorySeparatorChar + info.Name);
                }

                count += files.Length;
            } catch { }
        }

        public string DATA {
            get {
                return personData;
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            cancelToken.Cancel();
            personData = null;
            parent.SearchAA(this, dName);
        }

        int k = 0;

        public async void Search(string path) {
            try {
                var progress = new Progress<int>(v => {
                    progressBar1.Value = v;
                    progressBar1.Update();
                });

                DirectoryInfo di = new DirectoryInfo(path);
                DirectoryInfo[] directories = di.GetDirectories();
                FileInfo[] files = di.GetFiles();

                foreach (DirectoryInfo info in directories) {
                    Search(path + Path.DirectorySeparatorChar + info.Name);
                }

                foreach (FileInfo info in files) {
                    k++;
                    await SearchFileAsync(path + Path.DirectorySeparatorChar + info.Name, progress, k);
                }

                parent.SearchAA(this, dName);
            } catch { }
        }

        private async Task SearchFileAsync(string path, IProgress<int> progress, int k) {
            try {
                await Task.Run(() => {
                    using (FileStream fileS = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        byte[] b = new byte[1024];
                        UTF8Encoding temp = new UTF8Encoding(true);
                        Regex[] r = new Regex[2];
                        r[0] = new Regex(@"[-a-f0-9_.]+@{1}[-0-9a-z]+\.[a-z]{2,5}");
                        r[1] = new Regex(@"(\+7|8)-\([0-9]{3}\)-[0-9]{3}-[0-9]{2}-[0-9]{2}");

                        while (fileS.Read(b, 0, b.Length) > 0) {
                            for (int i = 0; i < 2; i++)
                                foreach (Match m in r[i].Matches(temp.GetString(b))) {
                                    personData += m.ToString();
                                    cancellationToken.ThrowIfCancellationRequested();
                                }
                        }
                        if (progress != null)
                            progress.Report(k * 100 / count);
                    }
                });
            } catch { }
        }
    }
}
