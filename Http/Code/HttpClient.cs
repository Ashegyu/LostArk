﻿using LostArkAction.Code;
using LostArkAction.Model;
using LostArkAction.View;
using LostArkAction.viewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LostArkAction.Code
{
   

    public class HttpClient2
    {
        public static List<string> APIkeys { get; set; } = new List<string>();
   
        public static HttpClient SharedClient { get; set; } = new HttpClient();
        public HttpClient2() {
            SharedClient = new HttpClient();
            SharedClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", APIkeys[0]);
            SharedClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            SharedClient.DefaultRequestHeaders.Add("ContentType","application/json");
        }
        public static async void GetAsync(List<List<SearchAblity>> searchAblitie, Accesories accesory)
        {
            (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.NeckAcc = new List<AccVM>();
           
            int cnt = 0;
            int apiKeyidx = 0;
            int searchTotal =0;
            int searchCnt = 0;
            for(int i = 0; i < searchAblitie.Count; i++)
            {
                searchTotal += searchAblitie[i].Count;
            }
            searchTotal = searchTotal * 3;
            for (int o = 0; o < searchAblitie.Count; o++)
            {
                (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.RingAcc1 = new List<AccVM>();
                (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.EarAcc1 = new List<AccVM>();
                for (int k = 0; k < 5; k += 2)
                {
                    string AcceccesoryType = Ablity.AccessoryCode.Keys.ToList()[k];

                    for (int i = 0; i < searchAblitie[o].Count; i++)
                    {
                        int pageNo = 1;
                        SearchItem item = new SearchItem();
                        item.Sort = "ITEM_QUALITY ";
                        if (Ablity.selectClass == 0)
                        {
                            item.ItemGrade = "유물";
                        }
                        else if (Ablity.selectClass == 1)
                        {
                            item.ItemGrade = "고대";
                        }
                        else
                        {
                            item.ItemGrade = "";
                        }
                        item.CategoryCode = Ablity.AccessoryCode[AcceccesoryType];
                        item.ItemGradeQuality = (int)(accesory[AcceccesoryType].Qulity / 10) * 10;
                        item.EtcOptions.Add(new EtcOption()
                        {
                            FirstOption = 3,
                            SecondOption = Ablity.AblityCode[searchAblitie[o][i].FirstAblity.Keys.ToList()[0]],
                            MinValue = searchAblitie[o][i].FirstAblity[searchAblitie[o][i].FirstAblity.Keys.ToList()[0]],
                            MaxValue = searchAblitie[o][i].FirstAblity[searchAblitie[o][i].FirstAblity.Keys.ToList()[0]],

                        });
                        if (searchAblitie[o][i].SecondAblity.Keys.ToList()[0] != "random")
                        {
                            item.EtcOptions.Add(new EtcOption()
                            {
                                FirstOption = 3,
                                SecondOption = Ablity.AblityCode[searchAblitie[o][i].SecondAblity.Keys.ToList()[0]],
                                MinValue = searchAblitie[o][i].SecondAblity[searchAblitie[o][i].SecondAblity.Keys.ToList()[0]],

                            });
                        }
                        int code = Ablity.CharactericsCode[accesory[AcceccesoryType].Characteristic[0]];
                        item.EtcOptions.Add(new EtcOption()
                        {
                            FirstOption = 2,
                            SecondOption = code,
                            MaxValue = 500
                        });
                        if (AcceccesoryType == "목걸이")
                        {
                            code = Ablity.CharactericsCode[accesory[AcceccesoryType].Characteristic[1]];
                            item.EtcOptions.Add(new EtcOption()
                            {
                                FirstOption = 2,
                                SecondOption = code,
                                MaxValue = 500
                            });
                        }
                        AuctionItem auctionItem = new AuctionItem();
                        auctionItem.Name = "";

                        while (true)
                        {
                            Thread.Sleep(1);
                            cnt++;
                            if (cnt >= 100)
                            {
                                SharedClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", APIkeys[apiKeyidx]);
                                apiKeyidx++;
                                if (apiKeyidx > APIkeys.Count - 1)
                                {
                                    apiKeyidx = 0;
                                }
                                cnt = 0;

                            }
                            item.PageNo = pageNo;
                            ResultItem tmp;
                            StringContent a = new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8, "application/json");
                            using (HttpResponseMessage response = await SharedClient.PostAsync("https://developer-lostark.game.onstove.com/auctions/items", a))
                            {
                                string jsonResponse = await response.Content.ReadAsStringAsync();
                                tmp = JsonConvert.DeserializeObject<ResultItem>(jsonResponse);
                            }

                            if (tmp != null)
                            {
                                if (tmp.TotalCount == 0)
                                {
                                    break;
                                }
                                if (tmp.Items != null)
                                {
                                    bool isQuality = false;

                                    for (int j = 0; j < tmp.Items.Count; j++)
                                    {

                                        if (tmp.Items[j].GradeQuality < (accesory[AcceccesoryType].Qulity))
                                        {
                                            isQuality = true;
                                            break;
                                        }
                                        if (tmp.Items[j].AuctionInfo.BuyPrice != 0 && tmp.Items[j].AuctionInfo.BuyPrice != null)
                                        {
                                            bool isSame = false;
                                            AccVM tmp2 = (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.ConvertAuctionItemToAcc(tmp.Items[j], AcceccesoryType);

                                            if (AcceccesoryType == "목걸이")
                                            {
                                                for (int p = 0; p < (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.NeckAcc.Count; p++)
                                                {
                                                    if ((App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.NeckAcc[p].Contain(tmp2))
                                                    {
                                                        if ((App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.NeckAcc[p].Price > tmp2.Price)
                                                        {
                                                            (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.NeckAcc[p] = tmp2;
                                                        }
                                                        isSame = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            else if (AcceccesoryType == "반지2")
                                            {
                                                for (int p = 0; p < (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.RingAcc1.Count; p++)
                                                {
                                                    if ((App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.RingAcc1[p].Contain(tmp2))
                                                    {
                                                        if ((App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.RingAcc1[p].Price > tmp2.Price)
                                                        {
                                                            (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.RingAcc1[p] = tmp2;
                                                        }
                                                        isSame = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            else if (AcceccesoryType == "귀걸이2")
                                            {
                                                for (int p = 0; p < (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.EarAcc1.Count; p++)
                                                {
                                                    if ((App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.EarAcc1[p].Contain(tmp2))
                                                    {
                                                        if ((App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.EarAcc1[p].Price > tmp2.Price)
                                                        {
                                                            (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.EarAcc1[p] = tmp2;
                                                        }
                                                        isSame = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (!isSame)
                                            {
                                                (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.SetAcc(tmp.Items[j], AcceccesoryType);
                                            }
                                        }
                                    }
                                    if (isQuality)
                                    {
                                        break;
                                    }
                                }
                                pageNo++;
                            }
                        }
                        if (auctionItem.Name != "")
                        {
                        }
                        searchCnt++;
                        (App.Current.MainWindow.DataContext as MainWinodwVM).SearchProgressValue = (float)searchCnt / searchTotal * 100;
                    }
                    // (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.SetAcc();
                }
                Console.WriteLine("검색완료");
                (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.SetNeck();
                (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.combination((App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.RingAcc1, 0);
                (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.combination((App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.EarAcc1, 1);
            }
            (App.Current.MainWindow.DataContext as MainWinodwVM).Ablity.Start();
        }

    }

}
