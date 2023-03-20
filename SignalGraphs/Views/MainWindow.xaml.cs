using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using SignalGraphs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Application = Microsoft.Office.Interop.Excel.Application;
using Window = System.Windows.Window;

namespace SignalGraphs
{
    public partial class MainWindow : Window
    {
        private SignalProcess signalProcess;
        private List<(int, int)> tempUPList = new List<(int, int)>();
        private List<(int, int)> tempDNList = new List<(int, int)>();
        public MainWindow()
        {
            InitializeComponent();

            //Заголовки графиков (в xaml задать почему-то нельзя)
            PlotImportUP.Plot.Title("Сырой UP");
            PlotBidirUP.Plot.Title("Перевернутый UP");
            PlotWinUP.Plot.Title("Обрезанный UP");
            PlotSumUP.Plot.Title("Суммированный UP");
            PlotImportDN.Plot.Title("Сырой DN");
            PlotBidirDN.Plot.Title("Перевернутый DN");
            PlotWinDN.Plot.Title("Обрезанный DN");
            PlotSumDN.Plot.Title("Суммированный DN");
        }

        private void DisplayRaw(object sender, RoutedEventArgs e)
        {
            try
            {
                //Очищаем интерфейс
                PlotImportUP.Plot.Clear();
                PlotBidirUP.Plot.Clear();
                PlotWinUP.Plot.Clear();
                PlotSumUP.Plot.Clear();
                PlotImportDN.Plot.Clear();
                PlotBidirDN.Plot.Clear();
                PlotWinDN.Plot.Clear();
                PlotSumDN.Plot.Clear();
                maxAmplUPTB.Clear();
                maxAmplDNTB.Clear();
                maxAmplTimeUPTB.Clear();
                maxAmplTimeDNTB.Clear();
                maxAmplSumUPTB.Clear();
                maxAmplSumDNTB.Clear();
                maxAmplSumTimeUPTB.Clear();
                maxAmplSumTimeDNTB.Clear();
                minAmplUPTB.Clear();
                minAmplDNTB.Clear();
                minAmplTimeUPTB.Clear();
                minAmplTimeDNTB.Clear();
                minAmplSumUPTB.Clear();
                minAmplSumDNTB.Clear();
                minAmplSumTimeUPTB.Clear();
                minAmplSumTimeDNTB.Clear();

                //Считываем переменные с интерфейса
                int pairNumber = int.Parse(pairNumberTextBox.Text);
                pairNumber = pairNumber < 1 ? 1 : pairNumber;
                pairNumber = pairNumber > 32 ? 32 : pairNumber;
                int startTime = int.Parse(startTimeTextBox.Text);

                //Загружаем переменные
                signalProcess.MidlineRawUP = int.Parse(midline_raw_UPTB.Text);
                signalProcess.MidlineRawDN = int.Parse(midline_raw_DNTB.Text);
                signalProcess.WinLeft = int.Parse(winLeftTB.Text);
                signalProcess.WinRight = int.Parse(winRigthTB.Text);

                //Вычисляем
                signalProcess.RawUP_Import();
                signalProcess.RawDN_Import();
                signalProcess.RawUP_Bidir();
                signalProcess.RawDN_Bidir();
                signalProcess.BidirUP_win(pairNumber);
                signalProcess.BidirDN_win(pairNumber);
                signalProcess.SummUP();
                signalProcess.SummDN();

                var data_X = new List<double>(); //Список точек X на графиках
                var data_Y = new List<double>(); //Список точек Y на графиках

                #region Вывод сигнала UP
                if (pairNumber <= signalProcess.ArrayRawUP.GetLength(0))
                {
                    for (int i = 0; i < signalProcess.ArrayRawUP.GetLength(0); i++)
                    {
                        data_X = signalProcess.ArrayRawUP[pairNumber - 1].Select(s => (double)s.Item1).ToList();
                        data_Y = signalProcess.ArrayRawUP[pairNumber - 1].Select(s => (double)s.Item2).ToList();
                    }
                    data_X = data_X.Select(x => startTime + x / 10).ToList();
                    PlotImportUP.Plot.AddScatter(data_X.ToArray(), data_Y.ToArray());

                    UPloadTB.Text = signalProcess.WrongUP.Length.ToString();
                    UPwrongTB.Text = signalProcess.WrongUP.Where(x => x > 0).ToList().Count.ToString();
                    UPwrongNumbersTB.Text = string.Join(",", signalProcess.WrongUP.Select((b, i) => b > 0 ? i : -1).Where(i => i != -1).Select(n => (n+1).ToString()).ToArray());
                }
                PlotImportUP.Refresh();

                data_X.Clear();
                data_Y.Clear();

                if (pairNumber <= signalProcess.ArrayRawUPBidir.GetLength(0))
                {
                    for (int i = 0; i < signalProcess.ArrayRawUPBidir.GetLength(0); i++)
                    {
                        data_X = signalProcess.ArrayRawUPBidir[pairNumber - 1].Select(s => (double)s.Item1).ToList();
                        data_Y = signalProcess.ArrayRawUPBidir[pairNumber - 1].Select(s => (double)s.Item2).ToList();
                    }
                    data_X = data_X.Select(x => startTime + x / 10).ToList();
                    PlotBidirUP.Plot.AddScatter(data_X.ToArray(), data_Y.ToArray());
                }
                PlotBidirUP.Refresh();

                data_X.Clear();
                data_Y.Clear();

                if (pairNumber <= signalProcess.ArrayBidirUPWin.GetLength(0))
                {
                    for (int i = 0; i < signalProcess.ArrayBidirUPWin.GetLength(0); i++)
                    {
                        data_X = signalProcess.ArrayBidirUPWin[pairNumber - 1].Select(s => (double)s.Item1).ToList();
                        data_Y = signalProcess.ArrayBidirUPWin[pairNumber - 1].Select(s => (double)s.Item2).ToList();
                    }
                    data_X = data_X.Select(x => startTime + x / 10).ToList();
                    PlotWinUP.Plot.AddScatter(data_X.ToArray(), data_Y.ToArray());

                    maxAmplUPTB.Text = signalProcess.AmplMaxWinUP.Item2.ToString();
                    maxAmplTimeUPTB.Text = (startTime + (double)signalProcess.AmplMaxWinUP.Item1 / 10).ToString();
                    minAmplUPTB.Text = signalProcess.AmplMinWinUP.Item2.ToString();
                    minAmplTimeUPTB.Text = (startTime + (double)signalProcess.AmplMinWinUP.Item1 / 10).ToString();
                }
                PlotWinUP.Refresh();

                data_X.Clear();
                data_Y.Clear();

                if (pairNumber <= signalProcess.ArraySummUP.Count)
                {
                    for (int i = 0; i < signalProcess.ArraySummUP.Count; i++)
                    {
                        data_X = signalProcess.ArraySummUP.Select(s => (double)s.Item1).ToList();
                        data_Y = signalProcess.ArraySummUP.Select(s => (double)s.Item2).ToList();
                    }
                    data_X = data_X.Select(x => startTime + x / 10).ToList();
                    PlotSumUP.Plot.AddScatter(data_X.ToArray(), data_Y.ToArray());

                    maxAmplSumUPTB.Text = signalProcess.AmplMaxSumUP.Item2.ToString();
                    maxAmplSumTimeUPTB.Text = (startTime + (double)signalProcess.AmplMaxSumUP.Item1 / 10).ToString();
                    minAmplSumUPTB.Text = signalProcess.AmplMinSumUP.Item2.ToString();
                    minAmplSumTimeUPTB.Text = (startTime + (double)signalProcess.AmplMinSumUP.Item1 / 10).ToString();
                }
                PlotSumUP.Refresh();
                #endregion
                #region Вывод сигнала DN
                if (pairNumber <= signalProcess.ArrayRawDN.GetLength(0))
                {
                    for (int i = 0; i < signalProcess.ArrayRawDN.GetLength(0); i++)
                    {
                        data_X = signalProcess.ArrayRawDN[pairNumber - 1].Select(s => (double)s.Item1).ToList();
                        data_Y = signalProcess.ArrayRawDN[pairNumber - 1].Select(s => (double)s.Item2).ToList();
                    }
                    data_X = data_X.Select(x => startTime + x / 10).ToList();
                    PlotImportDN.Plot.AddScatter(data_X.ToArray(), data_Y.ToArray());

                    DNloadTB.Text = signalProcess.WrongDN.Length.ToString();
                    DNwrongTB.Text = signalProcess.WrongDN.Where(x => x > 0).ToList().Count.ToString();
                    DNwrongNumbersTB.Text = string.Join(",", signalProcess.WrongDN.Select((b, i) => b > 0 ? i : -1).Where(i => i != -1).Select(n => (n + 1).ToString()).ToArray());
                }
                PlotImportDN.Refresh();

                data_X.Clear();
                data_Y.Clear();

                if (pairNumber <= signalProcess.ArrayRawDNBidir.GetLength(0))
                {
                    for (int i = 0; i < signalProcess.ArrayRawDNBidir.GetLength(0); i++)
                    {
                        data_X = signalProcess.ArrayRawDNBidir[pairNumber - 1].Select(s => (double)s.Item1).ToList();
                        data_Y = signalProcess.ArrayRawDNBidir[pairNumber - 1].Select(s => (double)s.Item2).ToList();
                    }
                    data_X = data_X.Select(x => startTime + x / 10).ToList();
                    PlotBidirDN.Plot.AddScatter(data_X.ToArray(), data_Y.ToArray());
                }
                PlotBidirDN.Refresh();

                data_X.Clear();
                data_Y.Clear();

                if (pairNumber <= signalProcess.ArrayBidirDNWin.GetLength(0))
                {
                    for (int i = 0; i < signalProcess.ArrayBidirDNWin.GetLength(0); i++)
                    {
                        data_X = signalProcess.ArrayBidirDNWin[pairNumber - 1].Select(s => (double)s.Item1).ToList();
                        data_Y = signalProcess.ArrayBidirDNWin[pairNumber - 1].Select(s => (double)s.Item2).ToList();
                    }
                    data_X = data_X.Select(x => startTime + x / 10).ToList();
                    PlotWinDN.Plot.AddScatter(data_X.ToArray(), data_Y.ToArray());

                    maxAmplDNTB.Text = signalProcess.AmplMaxWinDN.Item2.ToString();
                    maxAmplTimeDNTB.Text = (startTime + (double)signalProcess.AmplMaxWinDN.Item1 / 10).ToString();
                    minAmplDNTB.Text = signalProcess.AmplMinWinDN.Item2.ToString();
                    minAmplTimeDNTB.Text = (startTime + (double)signalProcess.AmplMinWinDN.Item1 / 10).ToString();
                }
                PlotWinDN.Refresh();

                data_X.Clear();
                data_Y.Clear();

                if (pairNumber <= signalProcess.ArraySummDN.Count)
                {
                    for (int i = 0; i < signalProcess.ArraySummDN.Count; i++)
                    {
                        data_X = signalProcess.ArraySummDN.Select(s => (double)s.Item1).ToList();
                        data_Y = signalProcess.ArraySummDN.Select(s => (double)s.Item2).ToList();
                    }
                    data_X = data_X.Select(x => startTime + x / 10).ToList();
                    PlotSumDN.Plot.AddScatter(data_X.ToArray(), data_Y.ToArray());

                    maxAmplSumDNTB.Text = signalProcess.AmplMaxSumDN.Item2.ToString();
                    maxAmplSumTimeDNTB.Text = (startTime + (double)signalProcess.AmplMaxSumDN.Item1 / 10).ToString();
                    minAmplSumDNTB.Text = signalProcess.AmplMinSumDN.Item2.ToString();
                    minAmplSumTimeDNTB.Text = (startTime + (double)signalProcess.AmplMinSumDN.Item1 / 10).ToString();
                }
                PlotSumDN.Refresh();
                #endregion
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка!"); }
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
            {
                pathTextBlock.Text = openFileDialog.FileName;
                pairCountTextBox.IsEnabled = true;
                pointsCountTextBox.IsEnabled = true;
                openButton.IsEnabled = true;
            }
        }

        private async void LoadSignals(object sender, RoutedEventArgs e)
        {
            try
            {
                int pairCount = 0;
                int pointsCount = 0;
                int.TryParse(pairCountTextBox.Text, out pairCount);
                int.TryParse(pointsCountTextBox.Text, out pointsCount);

                if (pairCount > 0 && pairCount <= 32 &&
                    pointsCount > 0 &&
                    !string.IsNullOrEmpty(pathTextBlock.Text))
                {
                    openButton.IsEnabled = false;
                    pairCountTextBox.IsEnabled = false;
                    pointsCountTextBox.IsEnabled = false;
                    openFileBT.IsEnabled = false;
                    loadTB.Text = "Загрузка...";
                    signalProcess = new SignalProcess(pairCount, pointsCount);
                    try
                    {
                        await LoadExcelAsync(pathTextBlock.Text, pointsCount, pairCount);

                        signalProcess.tempUPList = tempUPList;
                        signalProcess.tempDNList = tempDNList;

                        midline_raw_UPTB.Text = signalProcess.MidlineRawUP.ToString();
                        midline_raw_DNTB.Text = signalProcess.MidlineRawDN.ToString();

                        winLeftTB.Text = signalProcess.WinLeft.ToString();
                        winRigthTB.Text = signalProcess.WinRight.ToString();

                        DisplayRaw(null, null);

                        rawDataTab.IsEnabled = true;
                        rawDataTab.IsSelected = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка!");
                    }
                    finally
                    {
                        openButton.IsEnabled = true;
                        pairCountTextBox.IsEnabled = true;
                        pointsCountTextBox.IsEnabled = true;
                        openFileBT.IsEnabled = true;
                        loadTB.Text = string.Empty;
                        progressBar.Value = 0;
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректные значения", "Ошибка!");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка!"); }
        }

        private async Task LoadExcelAsync(string dataPath, int pointsPerPair, int pairsCount)
        {
            tempUPList.Clear();
            tempDNList.Clear();

            Application excel = new Application();
            Workbook wb = excel.Workbooks.Open(Path.GetFullPath(dataPath));

            try
            {
                await Task.Run(() =>
                {
                    Worksheet excelSheet = wb.ActiveSheet;

                    for (int row = 0; row < pointsPerPair * pairsCount; row++)
                    {
                        Dispatcher.Invoke(() => progressBar.Value = (double)row / (pointsPerPair * pairsCount));

                        var Xvalue = (int)(excelSheet.Cells[row + 1, 1] as Range).Value;
                        var UPvalue = (int)(excelSheet.Cells[row + 1, 2] as Range).Value;
                        var DNvalue = (int)(excelSheet.Cells[row + 1, 3] as Range).Value;

                        if (UPvalue >= 1024 || UPvalue < 0)
                        {
                            Dispatcher.Invoke(() => signalProcess.WrongUP[row / pointsPerPair]++);
                        }

                        if (DNvalue >= 1024 || DNvalue < 0)
                        {
                            Dispatcher.Invoke(() => signalProcess.WrongDN[row / pointsPerPair]++);
                        }

                        tempUPList.Add((Xvalue, UPvalue));
                        tempDNList.Add((Xvalue, DNvalue));
                    }
                });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка!"); }
            finally
            {
                wb.Close();
                excel.Quit();
                Marshal.ReleaseComObject(excel);
            }
        }
    }
}
