using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfAppCriptDecrypt
{
    public partial class MainWindow : Window
    {
        BackgroundWorker bkworkerEncrypt = new BackgroundWorker();
        BackgroundWorker bkworkerDecrypt = new BackgroundWorker();
        private bool isPaused = false;

        public MainWindow()
        {
            InitializeComponent();
            bkworkerEncrypt.WorkerReportsProgress = true;
            bkworkerEncrypt.WorkerSupportsCancellation = true;
            bkworkerEncrypt.DoWork += bkworkerEncrypt_DoWork;
            bkworkerEncrypt.ProgressChanged += bkworker_ProgressChanged;
            bkworkerEncrypt.RunWorkerCompleted += bkworker_RunWorkerCompleted;

            bkworkerDecrypt.WorkerReportsProgress = true;
            bkworkerDecrypt.WorkerSupportsCancellation = true;
            bkworkerDecrypt.DoWork += bkworkerDecrypt_DoWork;
            bkworkerDecrypt.ProgressChanged += bkworker_ProgressChanged;
            bkworkerDecrypt.RunWorkerCompleted += bkworker_RunWorkerCompleted;

            lbl_text_afis.Content = "Hello BackgroudWorker!";
        }

        void bkworkerEncrypt_DoWork(object? sender, DoWorkEventArgs e)
        {
            string text = (string)e.Argument;
            string textCriptat = "";
            string key = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (bkworkerEncrypt.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                while (isPaused)
                {
                    Thread.Sleep(100);
                }

                var (encryptedChar, keyChar) = Cripting.CriptareChar(text[i], i);
                textCriptat += encryptedChar;
                key += keyChar;

                int progressPercentage = (i + 1) * 100 / text.Length;
                bkworkerEncrypt.ReportProgress(progressPercentage, new Tuple<string, string>(textCriptat, key));

                Thread.Sleep(100);
            }
            e.Result = new Tuple<string, string>(textCriptat, key);
        }

        void bkworkerDecrypt_DoWork(object? sender, DoWorkEventArgs e)
        {
            var data = (Tuple<string, string>)e.Argument;
            string text = data.Item1;
            string key = data.Item2;
            string textDecriptat = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (bkworkerDecrypt.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                while (isPaused)
                {
                    Thread.Sleep(100);
                }

                char decryptedChar = Cripting.DecriptareChar(text[i], key[i], i);
                textDecriptat += decryptedChar;

                int progressPercentage = (i + 1) * 100 / text.Length;
                bkworkerDecrypt.ReportProgress(progressPercentage, textDecriptat);

                Thread.Sleep(100);
            }
            e.Result = textDecriptat;
        }

        void bkworker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                lbl_text_afis.Content += " Canceled!";
                if (sender == bkworkerEncrypt)
                {
                    text_crypted.Text += " (Invalid)";
                }
                else if (sender == bkworkerDecrypt)
                {
                    text_decrypted.Text += " (Invalid)";
                }
            }
            else if (e.Error != null)
            {
                lbl_text_afis.Content = "Exceptie BackgoroudWorker : " + e.Error.ToString();
            }
            else
            {
                if (sender == bkworkerEncrypt)
                {
                    var result = (Tuple<string, string>)e.Result;
                    text_crypted.Text = result.Item1;
                    text_decrypted.Tag = result.Item2; // Store the key in the Tag property
                    lbl_text_afis.Content = "Encryption Finalized!";
                }
                else if (sender == bkworkerDecrypt)
                {
                    text_decrypted.Text = (string)e.Result;
                    lbl_text_afis.Content = "Decryption Finalized!";
                }
            }
        }

        void bkworker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            this.progres.Value = e.ProgressPercentage;
            if (e.UserState is Tuple<string, string> encryptState)
            {
                text_crypted.Text = encryptState.Item1;
            }
            else if (e.UserState is string decryptState)
            {
                text_decrypted.Text = decryptState;
            }
            lbl_text_afis.Content = "Am ajuns la " + e.ProgressPercentage + "%";
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            isPaused = false;
            if (((ComboBoxItem)comboBoxProcessingType.SelectedItem).Content.ToString() == "BackgroundWorker")
            {
                bkworkerEncrypt.RunWorkerAsync(text_originall.Text);
            }
            else
            {
                Task.Run(async () =>
                {
                    string text = text_originall.Text;
                    string textCriptat = "";
                    string key = "";
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (isPaused)
                        {
                            await Task.Delay(100);
                            i--;
                            continue;
                        }

                        var (encryptedChar, keyChar) = Cripting.CriptareChar(text[i], i);
                        textCriptat += encryptedChar;
                        key += keyChar;

                        int progressPercentage = (i + 1) * 100 / text.Length;
                        Dispatcher.Invoke(() =>
                        {
                            this.progres.Value = progressPercentage;
                            text_crypted.Text = textCriptat;
                            lbl_text_afis.Content = "Am ajuns la " + progressPercentage + "%";
                        });

                        await Task.Delay(100);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        text_crypted.Text = textCriptat;
                        text_decrypted.Tag = key; // Store the key in the Tag property
                        lbl_text_afis.Content = "Encryption Finalized!";
                    });
                });
            }
        }

        private void btn_Decrypt_Click(object sender, RoutedEventArgs e)
        {
            isPaused = false;
            if (((ComboBoxItem)comboBoxProcessingType.SelectedItem).Content.ToString() == "BackgroundWorker")
            {
                var data = new Tuple<string, string>(text_crypted.Text, (string)text_decrypted.Tag);
                bkworkerDecrypt.RunWorkerAsync(data);
            }
            else
            {
                Task.Run(async () =>
                {
                    string text = text_crypted.Text;
                    string key = (string)text_decrypted.Tag;
                    string textDecriptat = "";
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (isPaused)
                        {
                            await Task.Delay(100);
                            i--;
                            continue;
                        }

                        char decryptedChar = Cripting.DecriptareChar(text[i], key[i], i);
                        textDecriptat += decryptedChar;

                        int progressPercentage = (i + 1) * 100 / text.Length;
                        Dispatcher.Invoke(() =>
                        {
                            this.progres.Value = progressPercentage;
                            text_decrypted.Text = textDecriptat;
                            lbl_text_afis.Content = "Am ajuns la " + progressPercentage + "%";
                        });

                        await Task.Delay(100);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        text_decrypted.Text = textDecriptat;
                        lbl_text_afis.Content = "Decryption Finalized!";
                    });
                });
            }
        }

        private void btn_Freeze_Click(object sender, RoutedEventArgs e)
        {
            isPaused = true;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (bkworkerEncrypt.IsBusy) bkworkerEncrypt.CancelAsync();
            if (bkworkerDecrypt.IsBusy) bkworkerDecrypt.CancelAsync();
            isPaused = false;
        }

        private void btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            text_crypted.Text = string.Empty;
            text_decrypted.Text = string.Empty;
            lbl_text_afis.Content = "Cleared!";
            progres.Value = 0;
        }
    }
}
