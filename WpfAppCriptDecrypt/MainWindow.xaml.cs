using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WpfAppCriptDecrypt
{
    public partial class MainWindow : Window
    {
        BackgroundWorker bkworker = new BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
            bkworker.WorkerReportsProgress = true; //declanseaza eveniment ProgresChanged
            bkworker.WorkerSupportsCancellation = true; //va suporta intrerupere - Cancel
                                                        //se asociaza handler pt evenimentului DoWork executat de BackgroundWorker
            bkworker.DoWork += bkworker_DoWork;
            //se asociaza metoda handler a evenimentului ProgressChanged
            //care se va declansa cand se raporteaza progresul cu ReportProgress
            bkworker.ProgressChanged += bkworker_ProgressChanged;
            //se asociaza metoda handler a evenimentului RunWorkerCompleted
            //se va declansa atunci cand BackgroundWorker isi termina executia
            bkworker.RunWorkerCompleted += bkworker_RunWorkerCompleted;
            lbl_text_afis.Content = "Hello BackgroudWorker!";
        }
        //metoda (procesarea) pe care o executa BackgroundWorker
        void bkworker_DoWork(object? sender, DoWorkEventArgs e)
        {
            int total = 0;
            // for (int i = 0; i <= 100; i += 10)
            for (int i = 0; i <= 100; i++)
            {
                //daca s-a solicitat Cancel
                if (bkworker.CancellationPending)
                { e.Cancel = true; return; }
                //raporteaza progresul
                //se declanseaza un eveniment ProgressChanged
                bkworker.ReportProgress(i);
                total += i;
                //continua prelucrarea...simulam o intarziere….
                Thread.Sleep(100);
            }
            //s-a finalizat prelucrarea
            //se transmite rezultatul catre RunWorkerCompleted
            e.Result = total;
        }
        //metoda executata atunci cand BackgroundWorker isi finalizeaza executia
        void bkworker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            //daca a fost Cancel
            if (e.Cancelled) lbl_text_afis.Content += " Canceled!";
            //daca a aparut o exceptie
            else if (e.Error != null)
                lbl_text_afis.Content = "Exceptie BackgoroudWorker : " + e.Error.ToString();
            else
                //daca s-a terminat complet se afiseaza rezultatul transmis de DoWork
                lbl_text_afis.Content = "Finalizat! Total = " + e.Result;
        }
        //metoda care se declanseaza la apelul ReportProgress
        void bkworker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            this.progres.Value = e.ProgressPercentage;
            lbl_text_afis.Content = "Am ajuns la " + e.ProgressPercentage + "%";
        }
        //metoda handler buton Start Cripting
        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            bkworker.RunWorkerAsync(100);
            string[] args = { "one", "two" };
            Procesare(args);
        }
        //metoda handler buton Cancel
        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            text_crypted.Text = string.Empty; 
            text_decrypted.Text = string.Empty;
            if (bkworker.IsBusy) bkworker.CancelAsync();
        }
        public void Procesare(string[] args)
        {
            Task<String> t1 = new Task<String>(() => Cripting.Criptare(text_originall.Text));
            //Task<String> t2 = new Task<String>(() => AfisCuvinte());
            //Task<String> t3 = new Task<String>(() => AfisCulori());
            //pornire task-uri utilizand Start
            t1.Start();
            //t2.Start();
            //t3.Start();

            ////alternativa cu Parallel.Invoke
            //Parallel.Invoke
            // (
            // new Action(),
            // new Action(),
            // new Action()
            // );


            //afisare valori returnate de task - proprietatea Result
            //.NET blocheaza thread-ul main pentru a astepta finalizarea task-urilor
            System.Diagnostics.Debug.WriteLine(t1.Result);
            //System.Diagnostics.Debug.WriteLine(t2.Result);
            //System.Diagnostics.Debug.WriteLine(t3.Result);
            Console.ReadLine();
        }
        //static String AfisNumere()
        //{
        //    Thread.CurrentThread.Name = "Thread-ul 1";
        //    for (int i = 0; i < 5; i++)
        //    {
        //        System.Diagnostics.Debug.WriteLine(String.Format("Numele thread-ului {0} numarul curent {1}", Thread.CurrentThread.Name, i));
        //        Thread.Sleep(1000);
        //    }
        //    return String.Format("Task-ul cu numele {0} si-a finalizat executia!",
        //    Thread.CurrentThread.Name);
        //}
        //static String AfisCulori()
        //{
        //    Thread.CurrentThread.Name = "Thread-ul 3";
        //    String[] culori = { "rosu", "galben", "verde", "albastru", "portocaliu" };
        //    foreach (String c in culori)
        //    {
        //        System.Diagnostics.Debug.WriteLine(String.Format("Numele thread-ului {0} numarul curent {1}", Thread.CurrentThread.Name, c));
        //        Thread.Sleep(1000);
        //    }
        //    return String.Format("Task-ul cu numele {0} si-a finalizat executia!",
        //    Thread.CurrentThread.Name);
        //}
        //static String AfisCuvinte()
        //{
        //    Thread.CurrentThread.Name = "Thread-ul 2";
        //    String text = "Exemplu de sir utilizat pentru crearea taskurilor concurente";
        //    String[] cuvinte = text.Split(' ');
        //    foreach (String s in cuvinte)
        //    {
        //        System.Diagnostics.Debug.WriteLine(String.Format("Numele thread-ului {0} numarul curent {1}", Thread.CurrentThread.Name, s));
        //        Thread.Sleep(1000);
        //    }
        //    return String.Format("Task-ul cu numele {0} si-a finalizat executia!",
        //    Thread.CurrentThread.Name);
        //}
    }
}
