using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SignalGraphs.Models
{
    public class SignalProcess
    {
        //Необработанные точки
        public List<(int, int)> tempUPList = new List<(int, int)>();
        public List<(int, int)> tempDNList = new List<(int, int)>();

        #region Массивы
        private List<(int, int)>[] array_rawUP;
        public List<(int, int)>[] ArrayRawUP { get { return array_rawUP; } }

        private List<(int, int)>[] array_rawUP_bidir;
        public List<(int, int)>[] ArrayRawUPBidir { get { return array_rawUP_bidir; } }

        private List<(int, int)>[] array_bidirUP_win;
        public List<(int, int)>[] ArrayBidirUPWin { get { return array_bidirUP_win; } }

        private List<(int, int)> array_summUP;
        public List<(int, int)> ArraySummUP { get { return array_summUP; } }

        private List<(int, int)>[] array_rawDN;
        public List<(int, int)>[] ArrayRawDN { get { return array_rawDN; } }

        private List<(int, int)>[] array_rawDN_bidir;
        public List<(int, int)>[] ArrayRawDNBidir { get { return array_rawDN_bidir; } }

        private List<(int, int)>[] array_bidirDN_win;
        public List<(int, int)>[] ArrayBidirDNWin { get { return array_bidirDN_win; } }

        private List<(int, int)> array_summDN;
        public List<(int, int)> ArraySummDN { get { return array_summDN; } }
        #endregion

        #region Переменные
        private int pairsCount;
        private int pointsPerPair;
        public int MidlineRawUP { get; set; } = 512;
        public int MidlineRawDN { get; set; } = 512;
        public int WinLeft { get; set; } = 300;
        public int WinRight { get; set; } = 250;
        public int[] WrongUP { get; set; } //Хранит колличество бракованных точек UP в каждой группе
        public int[] WrongDN { get; set; } //Хранит колличество бракованных точек DN в каждой группе
        public (int,int) AmplMaxWinUP { get; private set; }
        public (int, int) AmplMinWinUP { get; private set; }
        public (int, int) AmplMaxWinDN { get; private set; }
        public (int, int) AmplMinWinDN { get; private set; }
        public (int, int) AmplMaxSumUP { get; private set; }
        public (int, int) AmplMinSumUP { get; private set; }
        public (int, int) AmplMaxSumDN { get; private set; }
        public (int, int) AmplMinSumDN { get; private set; }
        #endregion

        public SignalProcess(int pairsCount, int pointsPerPair)
        {
            this.pairsCount = pairsCount;
            this.pointsPerPair = pointsPerPair;

            WrongUP = new int[pairsCount];
            WrongDN = new int[pairsCount];
        }

        #region UP методы
        public void RawUP_Import()
        {
            if (array_rawUP == null)
            {
                array_rawUP = new List<(int, int)>[pairsCount - WrongUP.Where(x => x > 0).ToList().Count];
                for (int group = 0, clearGroup = 0; group < WrongUP.Length; group++)
                {
                    if (WrongUP[group] == 0)
                    {
                        array_rawUP[clearGroup] = new List<(int, int)>();
                        var t = tempUPList.GetRange(group * pointsPerPair, pointsPerPair);
                        for (int i = 0; i < t.Count; i++)
                        {
                            array_rawUP[clearGroup].Add((t[i].Item1 % pointsPerPair, t[i].Item2));
                        }
                        clearGroup++;
                    }
                }
            }
        }
        public void RawUP_Bidir()
        {
            array_rawUP_bidir = new List<(int, int)>[array_rawUP.Length];
            for (int pair = 0; pair < array_rawUP.Length; pair++)
            {
                array_rawUP_bidir[pair] = new List<(int, int)>();
                for (int i = 0; i < array_rawUP[pair].Count; i++)
                {
                    array_rawUP_bidir[pair].Add((array_rawUP[pair][i].Item1, array_rawUP[pair][i].Item2 - MidlineRawUP));
                }
            }
        }
        public void BidirUP_win(int pairNumber)
        {
            array_bidirUP_win = new List<(int, int)>[array_rawUP_bidir.Length];
            for (int pair = 0; pair < array_rawUP_bidir.Length; pair++)
            {
                array_bidirUP_win[pair] = new List<(int, int)>();
                var max = array_rawUP_bidir[pair].Max(s => s.Item2);
                var min = array_rawUP_bidir[pair].Min(s => s.Item2);
                var maxIdx = array_rawUP_bidir[pair].FindIndex(s => s.Item2 == max);
                var minIdx = array_rawUP_bidir[pair].FindIndex(s => s.Item2 == min);

                if (pair == pairNumber - 1)
                {
                    AmplMaxWinUP = array_rawUP_bidir[pair][maxIdx];
                    AmplMinWinUP = array_rawUP_bidir[pair][minIdx];
                }

                var centerIdx = maxIdx;
                var startIdx = (centerIdx - WinLeft) < 0 ? 0 : (centerIdx - WinLeft);
                var endIdx = (centerIdx + WinRight) > array_rawUP_bidir[pair].Count - 1 ? array_rawUP_bidir[pair].Count - 1 : (centerIdx + WinRight);

                for (int i = startIdx; i < endIdx; i++)
                {
                    array_bidirUP_win[pair].Add(array_rawUP_bidir[pair][i]);
                }
            }
        }
        public void SummUP()
        {
            array_summUP = new List<(int, int)>();
            //Временный словарь чтобы гарантировать уникальность X для каждого прохода по группам
            var dic = new Dictionary<int, int>();
            for (int pair = 0; pair < array_bidirUP_win.Length; pair++)
            {
                for (int i = 0; i < array_bidirUP_win[pair].Count; i++)
                {
                    if (dic.ContainsKey(array_bidirUP_win[pair][i].Item1))
                    {
                        dic[array_bidirUP_win[pair][i].Item1] += array_bidirUP_win[pair][i].Item2;
                    }
                    else { dic.Add(array_bidirUP_win[pair][i].Item1, array_bidirUP_win[pair][i].Item2); }
                }
            }

            foreach (var item in dic)
            {
                array_summUP.Add((item.Key, item.Value));
            }
            array_summUP = array_summUP.OrderBy(s => s.Item1).ToList();

            var max = array_summUP.Max(s => s.Item2);
            var min = array_summUP.Min(s => s.Item2);
            var maxIdx = array_summUP.FindIndex(s => s.Item2 == max);
            var minIdx = array_summUP.FindIndex(s => s.Item2 == min);

            AmplMaxSumUP = array_summUP[maxIdx];
            AmplMinSumUP = array_summUP[minIdx];
        }
        #endregion

        #region DN методы
        public void RawDN_Import()
        {
            if (array_rawDN == null)
            {
                array_rawDN = new List<(int, int)>[pairsCount - WrongDN.Where(x => x > 0).ToList().Count];
                for (int group = 0, clearGroup = 0; group < WrongDN.Length; group++)
                {
                    if (WrongDN[group] == 0)
                    {
                        array_rawDN[clearGroup] = new List<(int, int)>();
                        var t = tempDNList.GetRange(group * pointsPerPair, pointsPerPair);
                        for (int i = 0; i < t.Count; i++)
                        {
                            array_rawDN[clearGroup].Add((t[i].Item1 % pointsPerPair, t[i].Item2));
                        }
                        clearGroup++;
                    }
                }
            }
        }
        public void RawDN_Bidir()
        {
            array_rawDN_bidir = new List<(int, int)>[array_rawDN.Length];
            for (int pair = 0; pair < array_rawDN.Length; pair++)
            {
                array_rawDN_bidir[pair] = new List<(int, int)>();
                for (int i = 0; i < array_rawDN[pair].Count; i++)
                {
                    array_rawDN_bidir[pair].Add((array_rawDN[pair][i].Item1, array_rawDN[pair][i].Item2 - MidlineRawDN));
                }
            }
        }
        public void BidirDN_win(int pairNumber)
        {
            array_bidirDN_win = new List<(int, int)>[array_rawDN_bidir.Length];
            for (int pair = 0; pair < array_rawDN_bidir.Length; pair++)
            {
                array_bidirDN_win[pair] = new List<(int, int)>();
                var max = array_rawDN_bidir[pair].Max(s => s.Item2);
                var min = array_rawDN_bidir[pair].Min(s => s.Item2);
                var maxIdx = array_rawDN_bidir[pair].FindIndex(s => s.Item2 == max);
                var minIdx = array_rawDN_bidir[pair].FindIndex(s => s.Item2 == min);

                if (pair == pairNumber - 1)
                {
                    AmplMaxWinDN = array_rawDN_bidir[pair][maxIdx];
                    AmplMinWinDN = array_rawDN_bidir[pair][minIdx];
                }

                var centerIdx = maxIdx;
                var startIdx = (centerIdx - WinLeft) < 0 ? 0 : (centerIdx - WinLeft);
                var endIdx = (centerIdx + WinRight) > array_rawDN_bidir[pair].Count - 1 ? array_rawDN_bidir[pair].Count - 1 : (centerIdx + WinRight);

                for (int i = startIdx; i < endIdx; i++)
                {
                    array_bidirDN_win[pair].Add(array_rawDN_bidir[pair][i]);
                }
            }
        }
        public void SummDN()
        {
            array_summDN = new List<(int, int)>();
            //Временный словарь чтобы гарантировать уникальность X для каждого прохода по группам
            var dic = new Dictionary<int, int>();
            for (int pair = 0; pair < array_bidirDN_win.Length; pair++)
            {
                for (int i = 0; i < array_bidirDN_win[pair].Count; i++)
                {
                    if (dic.ContainsKey(array_bidirDN_win[pair][i].Item1))
                    {
                        dic[array_bidirDN_win[pair][i].Item1] += array_bidirDN_win[pair][i].Item2;
                    }
                    else { dic.Add(array_bidirDN_win[pair][i].Item1, array_bidirDN_win[pair][i].Item2); }
                }
            }

            foreach (var item in dic)
            {
                array_summDN.Add((item.Key, item.Value));
            }
            array_summDN = array_summDN.OrderBy(s => s.Item1).ToList();

            var max = array_summDN.Max(s => s.Item2);
            var min = array_summDN.Min(s => s.Item2);
            var maxIdx = array_summDN.FindIndex(s => s.Item2 == max);
            var minIdx = array_summDN.FindIndex(s => s.Item2 == min);

            AmplMaxSumDN = array_summDN[maxIdx];
            AmplMinSumDN = array_summDN[minIdx];
        }
        #endregion

    }
}
