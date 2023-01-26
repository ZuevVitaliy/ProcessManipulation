using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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

namespace ProcessManipulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region ExampleCode

        //константа, идентифицирующая сообщение WM _ SETTEXT
        const uint WM_SETTEXT = 0x0C;

        //импортируем функцию SendMEssage из библиотеки user32.dll
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd,
        uint Msg, int wParam,
        [MarshalAs(UnmanagedType.LPStr)] string lParam);

        //список, в котором будут храниться объекты, описывающие дочерние процессы приложения
        List<Process> _processes = new List<Process>();

        //счётчик запущенных процессов
        int Counter = 0;

        // метод, загружающий доступные исполняемые файлы из домашней директории проекта
        void LoadAvailableAssemblies()
        {
            //название файла сборки текущего приложения
            string except = Assembly.GetExecutingAssembly().GetName().Name;

            //получаем все *.exe файлы из домашней директории
            var pathProc1 = System.IO.Path.GetFullPath("../../../../Process1/bin/Debug/net6.0-windows");
            var pathProc2 = System.IO.Path.GetFullPath("../../../../Process2/bin/Debug/net6.0-windows");
            var files = new List<string>(Directory.GetFiles(pathProc1, "*.exe"));
            files.AddRange(Directory.GetFiles(pathProc2, "*.exe"));
            foreach (var file in files)
            {
                //получаем имя файла
                string fileName = new FileInfo(file).Name;

                //если имя файла не содержит имени исполняемого файла проекта, то оно добавляется в 
                if (fileName.IndexOf(except) == -1)
                    _availableAssembliesList.Items.Add(fileName);
            }
        }

        //метод, запускающий процесс на исполнение и сохраняющий объект, который его описывает
        void RunProcess(string AssemblyName)
        {
            //запускаем процесс на соновании исполняемого
            //файла
            Process proc = Process.Start(AssemblyName);

            //добавляем процесс в список
            _processes.Add(proc);

            /*проверяем, стал ли созданный процесс дочерним,
             *по отношению к текущему и, если стал, выводим
             *MessageBox*/
            if (Process.GetCurrentProcess().Id == GetParentProcessId(proc.Id))
                MessageBox.Show(proc.ProcessName +
                " действительно дочерний процесс текущего процесса!");

            /*указываем, что процесс должен генерировать события*/
            proc.EnableRaisingEvents = true;

            //добавляем обработчик на событие завершения процесса
            proc.Exited += proc_Exited;

            /*устанавливаем новый текст главному окну
             *дочернего процесса*/
            SetChildWindowText(proc.MainWindowHandle, "Childprocess #" + (++Counter));

            /*проверяем, запускали ли мы экземпляр такого
             *приложения и, если нет, то добавляем в список
             *запущенных приложений*/
            if (!_runningProcessesList.Items.Contains(proc.
            ProcessName))
                _runningProcessesList.Items.Add(proc.ProcessName);
            /*убираем приложение из списка доступных
             *приложений*/
            _availableAssembliesList.Items.
            Remove(_availableAssembliesList.SelectedItem);
        }

        //метод обёртывания для отправки сообщения WM _ SETTEXT
        void SetChildWindowText(IntPtr Handle, string text)
        {
            SendMessage(Handle, WM_SETTEXT, 0, text);
        }

        //метод, получающий PID родительского процесса (использует WMI)
        int GetParentProcessId(int Id)
        {
            int parentId = 0;
            using (ManagementObject obj = new ManagementObject("win32 _ process.handle=" +
             Id.ToString()))
            {
                obj.Get();
                parentId = Convert.
                 ToInt32(obj["ParentProcessId"]);
            }
            return parentId;
        }

        /*обработчик события Exited класса Process*/
        void proc_Exited(object sender, EventArgs e)
        {
            Process proc = sender as Process;
            //убираем процесс из списка запущенных //приложений
            _runningProcessesList.Items.Remove(proc.ProcessName);
            //добавляем процесс в список доступных //приложений
            _availableAssembliesList.Items.Add(proc.ProcessName);
            //убираем процесс из списка дочерних процессов
            _processes.Remove(proc);
            //уменьшаем счётчик дочерних процессов на 1
            Counter--;
            int index = 0;
            /*меняем текст для главных окон всех дочерних *процессов*/
            foreach (var p in _processes)
                SetChildWindowText(p.MainWindowHandle, "Child process #" + ++index);
        }

        //объявление делегата, принимающего параметр типа
        //Process
        delegate void ProcessDelegate(Process proc);

        /*метод, который выполняет проход по всем дочерним
        *процессам с заданым именем и выполняющий для этих
        *процессов заданый делегатом метод*/
        void ExecuteOnProcessesByName(string ProcessName,
         ProcessDelegate func)
        {
            /*получаем список, запущенных в операционной
             *системе процессов*/
            Process[] processes = Process.GetProcessesByName(ProcessName);
            foreach (var process in processes)
                /*если PID родительского процесса равен PID
                 *текущего процесса*/
                if (Process.GetCurrentProcess().Id == GetParentProcessId(process.Id))
                    //запускаем метод
                    func(process);
        }

        #endregion ExampleCode

        public MainWindow()
        {
            InitializeComponent();
            LoadAvailableAssemblies();
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            RunProcess(_availableAssembliesList.SelectedItem.ToString());
        }

        void Kill(Process proc)
        {
            proc.Kill();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            ExecuteOnProcessesByName(_runningProcessesList.SelectedItem.ToString(), Kill);
            _runningProcessesList.Items.Remove(_runningProcessesList.SelectedItem);
        }

        void CloseMainWindow(Process proc)
        {
            proc.CloseMainWindow();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            ExecuteOnProcessesByName(_runningProcessesList.SelectedItem.ToString(), CloseMainWindow);
            _runningProcessesList.Items.Remove(_runningProcessesList.SelectedItem);
        }

        void Refresh(Process proc)
        {
            proc.Refresh();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            ExecuteOnProcessesByName(_runningProcessesList.SelectedItem.ToString(), Refresh);

        }

        private void RunCalc(object sender, RoutedEventArgs e)
        {
            RunProcess("calc.exe");
        }

        private void AvailableAssemblies_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_availableAssembliesList.SelectedItems.Count == 0)
                _startButton.IsEnabled = false;
            else
                _startButton.IsEnabled = true;
        }

        private void StartedAssemblies_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_runningProcessesList.SelectedItems.Count == 0)
            {
                _stopButton.IsEnabled = false;
                _closeWindowButton.IsEnabled = false;
            }
            else
            {
                _stopButton.IsEnabled = true;
                _closeWindowButton.IsEnabled = true;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var proc in _processes)
                proc.Kill();
        }
    }
}
