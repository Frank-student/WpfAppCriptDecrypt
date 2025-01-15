using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private bool isPaused = false;
        private CancellationTokenSource? cts;

        public MainWindow()
        {
            InitializeComponent();
            lbl_text_afis.Content = "Hello BackgroundWorker!";
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            isPaused = false;
            string processingType = ((ComboBoxItem)comboBoxProcessingType.SelectedItem).Content.ToString();
            if (processingType == "Task")
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
            }
            else if (processingType == "PLINQ")
            {
                cts = new CancellationTokenSource();
                var token = cts.Token;
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
            }
        }

        private void btn_Decrypt_Click(object sender, RoutedEventArgs e)
        {
            isPaused = false;
            string processingType = ((ComboBoxItem)comboBoxProcessingType.SelectedItem).Content.ToString();
            if (processingType == "Task")
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
            }
            else if (processingType == "PLINQ")
            {
                cts = new CancellationTokenSource();
                var token = cts.Token;
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
            }
        }

        private void btn_Freeze_Click(object sender, RoutedEventArgs e)
        {
            isPaused = true;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
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


