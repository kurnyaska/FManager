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

namespace SPBU12._1MANAGER {
    public partial class DownloadFiles : Form {
        private CancellationTokenSource cancelToken;
        private CancellationToken cancellationToken;
        string[] theEBook;
        Form1 parent;

        public DownloadFiles(Form1 parent, int count) {
            cancelToken = new CancellationTokenSource();
            cancellationToken = cancelToken.Token;
            this.parent = parent;
            theEBook = new string[count];
            InitializeComponent();
        }

        public string[] EBOOK { get { return this.theEBook; } }
        
        private void button1_Click(object sender, EventArgs e) {
            cancelToken.Cancel();
            theEBook = null;
            parent.Download(this);
        }

        private async Task ReadFile(string path, HttpWebResponse myFWResp, int i, IProgress<int> progress) {
            try {
                string nameFile = Path.GetRandomFileName();
                await Task.Delay(1000, cancellationToken);
                await Task.Run(() => {
                    Stream receiveStream = myFWResp.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream);
                    char[] readBuffer = new Char[256];
                    int count = readStream.Read(readBuffer, 0, 256);

                    while (count > 0) {
                        String str = new String(readBuffer, 0, count);
                        theEBook[i] += str;
                        count = readStream.Read(readBuffer, 0, 256);
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    readStream.Close();
                    myFWResp.Close();
                    if (progress != null)
                        progress.Report((i + 1) * 100 / theEBook.Length);
                });
            } catch (WebException e) {
                Console.WriteLine("The WebException: " + e.Message);
            } catch (UriFormatException e) {
                Console.WriteLine("The UriFormatException: " + e.Message);
            }
        }

        public async void Download(HttpWebResponse[] myFileWebResponse, string path) {
            try {
                var progress = new Progress<int>(v => {
                    progressBar1.Value = v;
                    progressBar1.Update();
                });
                int i = 0;
                foreach (var myFWResp in myFileWebResponse) {
                    await ReadFile(path, myFWResp, i++, progress);
                }

                parent.Download(this);
            } catch (OperationCanceledException ex) {
                this.Invoke((Action)delegate {
                    this.Text = ex.Message;
                });
            }
        }
    }
}
