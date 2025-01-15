using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
        private CancellationTokenSource? cts;

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
                    text_decrypted.Tag = result.Item2; 
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
            string processingType = ((ComboBoxItem)comboBoxProcessingType.SelectedItem).Content.ToString();
            if (processingType == "BackgroundWorker")
            {
                bkworkerEncrypt.RunWorkerAsync(text_originall.Text);
            }
            else if (processingType == "Task")
            {
                cts = new CancellationTokenSource();
                var token = cts.Token;
                Task.Run(async () =>
                {
                    string text = text_originall.Text;
                    string textCriptat = "";
                    string key = "";
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            Dispatcher.Invoke(() => text_crypted.Text += " (Invalid)");
                            return;
                        }

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
                        text_decrypted.Tag = key;
                        lbl_text_afis.Content = "Encryption Finalized!";
                    });
                }, token);
            }
            else if (processingType == "Parallel")
            {
                cts = new CancellationTokenSource();
                var token = cts.Token;
                Task.Run(() =>
                {
                    string text = text_originall.Text;
                    char[] textCriptat = new char[text.Length];
                    char[] key = new char[text.Length];
                    Parallel.For(0, text.Length, (i, state) =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            state.Stop();
                            Dispatcher.Invoke(() => text_crypted.Text += " (Invalid)");
                            return;
                        }

                        while (isPaused)
                        {
                            Thread.Sleep(100);
                        }

                        var (encryptedChar, keyChar) = Cripting.CriptareChar(text[i], i);
                        textCriptat[i] = encryptedChar;
                        key[i] = keyChar;

                        int progressPercentage = (i + 1) * 100 / text.Length;
                        Dispatcher.Invoke(() =>
                        {
                            this.progres.Value = progressPercentage;
                            text_crypted.Text = new string(textCriptat);
                            lbl_text_afis.Content = "Am ajuns la " + progressPercentage + "%";
                        });

                        Thread.Sleep(100);
                    });
                    Dispatcher.Invoke(() =>
                    {
                        text_crypted.Text = new string(textCriptat);
                        text_decrypted.Tag = new string(key); 
                        lbl_text_afis.Content = "Encryption Finalized!";
                    });
                }, token);
            }
            else if (processingType == "PLINQ")
            {
                cts = new CancellationTokenSource();
                var token = cts.Token;
                Task.Run(() =>
                {
                    string text = text_originall.Text;
                    var textCriptat = new ConcurrentQueue<char>();
                    var key = new ConcurrentQueue<char>();

                    text.AsParallel().WithCancellation(token).Select((ch, i) =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            Dispatcher.Invoke(() => text_crypted.Text += " (Invalid)");
                            return (default(char), default(char));
                        }

                        while (isPaused)
                        {
                            Thread.Sleep(100);
                        }

                        var (encryptedChar, keyChar) = Cripting.CriptareChar(ch, i);
                        textCriptat.Enqueue(encryptedChar);
                        key.Enqueue(keyChar);

                        int progressPercentage = (i + 1) * 100 / text.Length;
                        Dispatcher.Invoke(() =>
                        {
                            this.progres.Value = progressPercentage;
                            text_crypted.Text = new string(textCriptat.ToArray());
                            lbl_text_afis.Content = "Am ajuns la " + progressPercentage + "%";
                        });

                        Thread.Sleep(100);
                        Debug.Print("In thread-ul de criptare: " + Thread.CurrentThread.ManagedThreadId);
                        return (encryptedChar, keyChar);
                    }).ToList();

                    Dispatcher.Invoke(() =>
                    {
                        text_crypted.Text = new string(textCriptat.ToArray());
                        text_decrypted.Tag = new string(key.ToArray()); 
                        lbl_text_afis.Content = "Encryption Finalized!";
                    });
                }, token);
            }
        }

        private void btn_Decrypt_Click(object sender, RoutedEventArgs e)
        {
            isPaused = false;
            string processingType = ((ComboBoxItem)comboBoxProcessingType.SelectedItem).Content.ToString();
            if (processingType == "BackgroundWorker")
            {
                var data = new Tuple<string, string>(text_crypted.Text, (string)text_decrypted.Tag);
                bkworkerDecrypt.RunWorkerAsync(data);
            }
            else if (processingType == "Task")
            {
                cts = new CancellationTokenSource();
                var token = cts.Token;
                Task.Run(async () =>
                {
                    string text = text_crypted.Text;
                    string key = (string)text_decrypted.Tag;
                    string textDecriptat = "";
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            Dispatcher.Invoke(() => text_decrypted.Text += " (Invalid)");
                            return;
                        }

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
                }, token);
            }
            else if (processingType == "Parallel")
            {
                cts = new CancellationTokenSource();
                var token = cts.Token;
                Task.Run(() =>
                {
                    string text = text_crypted.Text;
                    string key = (string)text_decrypted.Tag;
                    char[] textDecriptat = new char[text.Length];
                    Parallel.For(0, text.Length, (i, state) =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            state.Stop();
                            Dispatcher.Invoke(() => text_decrypted.Text += " (Invalid)");
                            return;
                        }

                        while (isPaused)
                        {
                            Thread.Sleep(100);
                        }

                        char decryptedChar = Cripting.DecriptareChar(text[i], key[i], i);
                        textDecriptat[i] = decryptedChar;

                        int progressPercentage = (i + 1) * 100 / text.Length;
                        Dispatcher.Invoke(() =>
                        {
                            this.progres.Value = progressPercentage;
                            text_decrypted.Text = new string(textDecriptat);
                            lbl_text_afis.Content = "Am ajuns la " + progressPercentage + "%";
                        });

                        Thread.Sleep(100);
                    });
                    Dispatcher.Invoke(() =>
                    {
                        text_decrypted.Text = new string(textDecriptat);
                        lbl_text_afis.Content = "Decryption Finalized!";
                    });
                }, token);
            }
            else if (processingType == "PLINQ")
            {
                cts = new CancellationTokenSource();
                var token = cts.Token;
                Task.Run(() =>
                {
                    string text = text_crypted.Text;
                    string key = (string)text_decrypted.Tag;
                    var textDecriptat = new ConcurrentQueue<char>();

                    text.AsParallel().WithCancellation(token).Select((ch, i) =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            Dispatcher.Invoke(() => text_decrypted.Text += " (Invalid)");
                            return default(char);
                        }

                        while (isPaused)
                        {
                            Thread.Sleep(100);
                        }

                        char decryptedChar = Cripting.DecriptareChar(ch, key[i], i);
                        textDecriptat.Enqueue(decryptedChar);

                        int progressPercentage = (i + 1) * 100 / text.Length;
                        Dispatcher.Invoke(() =>
                        {
                            this.progres.Value = progressPercentage;
                            text_decrypted.Text = new string(textDecriptat.ToArray());
                            lbl_text_afis.Content = "Am ajuns la " + progressPercentage + "%";
                        });

                        Thread.Sleep(100);
                        Debug.Print("In thread-ul de decriptare: " + Thread.CurrentThread.ManagedThreadId);
                        return decryptedChar;
                    }).ToList();

                    Dispatcher.Invoke(() =>
                    {
                        text_decrypted.Text = new string(textDecriptat.ToArray());
                        lbl_text_afis.Content = "Decryption Finalized!";
                    });
                }, token);
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
            cts?.Cancel();
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
