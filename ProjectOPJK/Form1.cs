using Google.Apis.Download;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectOPJK
{
    public partial class Form1 : Form
    {
        Google.Apis.Drive.v3.DriveService service;
        Dictionary<string, string> NamesAndID;
        int CountFiles;

        string PathForDownloads = @"ojpenhanced\";
        string PathForConfig = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\OpenJK\ojpenhanced\autoexec.cfg";

        delegate void EndPlease();
        //TODO ->{Доебаться к артему с папками}
        void ChangeProgress()
        {
            progressBar1.Value += 1;

            if (progressBar1.Value == CountFiles)
            {
                MessageBox.Show("Загрузка была завершена", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                richTextBox1.Text = "Можете начать игру!";
                PlayButton.Enabled = true;
                DownLoadButton.Enabled = true;
                CountFiles = 0;
            }
        }


        async void Start_Programm_check()
        {
            await Task.Run(() =>
            {
                service = new GetCredentialAndDriveService().GetDriveService();

                var FilesOnDisk = service.Files.List().Execute().Files;

                var fileNames = FilesOnDisk.Select(x => x.Name).ToList();
                var fileIds = FilesOnDisk.Select(x => x.Id).ToList();

                NamesAndID = new Dictionary<string, string>();

                for (int i = 0; i < fileNames.Count; i++)
                {
                    if (!File.Exists(PathForDownloads + fileNames[i]) && fileNames[i] != "RPC Setup.rar")
                    {
                        NamesAndID.Add(fileNames[i], fileIds[i]);

                        Invoke(new Action(() => richTextBox1.AppendText(fileNames[i] + "\n")));
                    }
                }
                CountFiles = NamesAndID.Count;

                if (CountFiles == 0)
                {
                    Invoke(new Action(() => richTextBox1.AppendText("Можете начать игру!")));
                }

            });

        }

        public async void DownLoad(string fileId, string FileName)
        {
            var request = service.Files.Get(fileId);

            using (var memoryStream = new MemoryStream())
            {
                request.MediaDownloader.ProgressChanged += (progress) => ReportProgress(progress, memoryStream, FileName);

                await Task.Run(() => request.Download(memoryStream));
            }
        }

        private void ReportProgress(IDownloadProgress progress, MemoryStream memoryStream, string FileName)
        {
            switch (progress.Status)
            {
                case DownloadStatus.Downloading:
                    {
                        Invoke(new Action(() =>
                        {
                            richTextBox2.AppendText(progress.Status + " " + FileName + " " + progress.BytesDownloaded + "\n");
                        }));

                        break;
                    }
                case DownloadStatus.Completed:
                    {
                        Console.WriteLine("Download complete.");

                        using (FileStream fs = new FileStream($"{PathForDownloads}{FileName}", FileMode.OpenOrCreate))
                        {
                            memoryStream.WriteTo(fs);
                            fs.Flush();
                        }

                        Invoke(new Action(() => richTextBox2.AppendText($"Download {FileName} complite \n")));
                        Invoke(new EndPlease(ChangeProgress));

                        break;
                    }
                case DownloadStatus.Failed:
                    {
                        Invoke(new Action(() => richTextBox2.AppendText($"Download {FileName} Failed (O_o) \n")));
                        break;
                    }
            }
        }

        void AsynDownLoadFiles()
        {
            if (CountFiles != 0)
            {
                progressBar1.Maximum = CountFiles;

                richTextBox2.AppendText("DOWNLOAD START!\n");
                richTextBox2.Select(0, "DOWNLOAD START!\n".Length);
                richTextBox2.SelectionColor = System.Drawing.Color.Green;

                PlayButton.Enabled = false;
                DownLoadButton.Enabled = false;

                foreach (var item in NamesAndID)
                {
                    DownLoad(item.Value, item.Key);
                }
            }
            else
                MessageBox.Show(
                    "У вас уже скачаны все нужные файлы\n" +
                    "Можете начать игру", "Предупреждение",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

        }

        void SetScreen()
        {
            if(Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\OpenJK"))
            {
                try
                {
                    var resolution = comboBox1.Text.Split(new char[5] { 'x', 'X', 'х', 'Х', ' ' }); // [0]->width [1]->height

                    using (StreamWriter stream = new StreamWriter(PathForConfig))
                    {
                        stream.WriteLine(
                            $"seta r_customwidth {resolution[0]}\n" +
                            $"seta r_customheight {resolution[1]}\n" +
                            $"seta r_fullscreen {Convert.ToInt32(checkBox1.Checked)}\n" +
                            $"seta r_mode {-1}");
                        stream.Close();
                    }

                    MessageBox.Show("Файл с настройками был создан", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception)
                {
                    MessageBox.Show(
                        "Возможно вы ввели неправельное разрешение\n" +
                        "Попробуйте написать числа с Английской x по середине", "Важная информация",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show(
                       "Сейчас откроется игра. Пожалуйста закройте её это нужно для корректной работы программы", "Важная информация",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Warning);
                Process.Start(@"Play_OJPEnhanced.bat");
            }
        }

        public Form1()
        {
            Start_Programm_check();
            InitializeComponent();
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (!File.Exists(PathForConfig))
            {
                MessageBox.Show("Может для начала создадите файлик с настройками?", "Предупреждение",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (CountFiles == 0 || progressBar1.Value == progressBar1.Maximum)
            {
                Process.Start(@"Play_OJPEnhanced.bat");
                this.Close();
            }
            else
            {
                MessageBox.Show("Скачайте файлы перед входом в игру!" +
                    "\nТак как без этих файлов вы не сможете зайти на сервер", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        void ResolutionButton_Click(object s, EventArgs e) => SetScreen();

        void DownLoadButton_Click(object s, EventArgs e) => AsynDownLoadFiles();

        private void comboBox1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Если вашего разрешения нет в списке, можете добавить его сами!",
                "Предупреждение",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
            comboBox1.Click -= comboBox1_Click;
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            richTextBox2.SelectionStart = richTextBox1.Text.Length;
            richTextBox2.ScrollToCaret();
        }
    }
}