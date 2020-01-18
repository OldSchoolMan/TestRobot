using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Media;
using System.Windows;
using System.Windows.Media;
using QuikSharp;
using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using Condition = QuikSharp.DataStructures.Condition;

namespace TestRobot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Quik _quik;

        string secCode = "SBER";    //  GAZP SBER RIH0 SiH0
        string classCode = "";
        string clientCode;
        private Tool _tool;
        OrderBook toolOrderBook;
        List<Candle> _candlesList = new List<Candle>(); // список свечек

        private MyTick _sber, _RI;
        List<MyTick> ttt = new List<MyTick>();

        private bool _flagRobot = false;
        private bool _flagRobotTick = false;

        private bool _isSubscribedToolOrderBook = false;
        private bool _isServerConnected = false;

        private bool _flag = false;

        private decimal Up, Centr, Low;

        private Position _pos = new Position();
        List<Position> listPos = new List<Position>();

        private DepoLim _dep = new DepoLim();
        List<DepoLim> listDep = new List<DepoLim>();

        /*
         private DepoLim _depBind = new DepoLim();
        ObservableCollection<DepoLim> listDepBind = new ObservableCollection<DepoLim>();
        */

        public MainWindow()
        {
            InitializeComponent();
            LogTextBox.Text = "";
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e) // подключение к Quik
        {
            try
            {
                Log("Подключаемся к терминалу Quik...");

                // инициализируем объект Quik с использованием локального расположения терминала (по умолчанию)                
                _quik = new Quik(Quik.DefaultPort, new InMemoryStorage());
            }
            catch
            {
                Log("Ошибка инициализации объекта Quik...");
            }
            if (_quik != null)
            {
                Log("Экземпляр Quik создан.");
                try
                {
                    Log("Получаем статус соединения с сервером....");
                    _isServerConnected = _quik.Service.IsConnected().Result;
                    if (_isServerConnected)
                    {
                        Log("Соединение с сервером установлено.");
                        ButtonConnect.Content = "OK";
                        ButtonConnect.Background = Brushes.Aqua;
                        ButtonConnect.IsEnabled = false;

                        //  buttonRun.Enabled = true;
                        //  buttonStart.Enabled = false;
                    }
                    else
                    {
                        Log("Соединение с сервером НЕ установлено.");
                        //  buttonRun.Enabled = false;
                        //  buttonStart.Enabled = true;
                    }
                }
                catch
                {
                    Log("Неудачная попытка получить статус соединения с сервером.");
                }
            }
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            Run();
        }
        void Run()
        {
            try
            {
                //secCode = textBoxSecCode.Text;
                Log("Определяем код класса инструмента " + secCode + ", по списку классов" + "...");
                try
                {
                    classCode = _quik.Class.GetSecurityClass("SPBFUT,TQBR,TQBS,TQNL,TQLV,TQNE,TQOB,QJSIM", secCode).Result;
                }
                catch
                {
                    Log("Ошибка определения класса инструмента. Убедитесь, что тикер указан правильно");
                }
                if (classCode != null && classCode != "")
                {
                    //textBoxClassCode.Text = classCode;
                    Log("Определяем код клиента...");
                    clientCode = _quik.Class.GetClientCode().Result;
                    //textBoxClientCode.Text = clientCode;
                    Log("Создаем экземпляр инструмента " + secCode + "|" + classCode + "...");
                    _tool = new Tool(_quik, secCode, classCode);
                    if (_tool != null && _tool.Name != null && _tool.Name != "")
                    {
                        Log("Инструмент " + _tool.Name + " создан.");
                        //Log("Подписываемся на стакан...");
                        /*
                        _quik.OrderBook.Subscribe(_tool.ClassCode, _tool.SecurityCode).Wait();
                        _isSubscribedToolOrderBook = _quik.OrderBook.IsSubscribed(_tool.ClassCode, _tool.SecurityCode).Result;
                        */
                        /*
                        if (_isSubscribedToolOrderBook)
                        {
                            toolOrderBook = new OrderBook();
                            Log("Подписка на стакан прошла успешно.");
                            Log("Подписываемся на колбэк 'OnQuote'...");
                            _quik.Events.OnQuote += Events_OnQuote;

                        }
                        else
                        {
                            Log("Подписка на стакан не удалась.");
                        }
                        */
                        Log("Подписываемся на колбэк 'Events_OnOrder'...");
                        _quik.Events.OnOrder += Events_OnOrder;
                        /*
                        _quik.Candles.Subscribe(classCode, secCode, CandleInterval.M1);
                        _quik.Candles.NewCandle += Candles_NewCandle;
                        */

                        //_quik.Events.OnAllTrade += Events_OnAllTrade;
                        //_quik.Events.OnParam += Events_OnParam;

                        /*
                        Log("Подписываемся на колбэк 'OnFuturesClientHolding'...");
                        _quik.Events.OnFuturesClientHolding += Events_OnFuturesClientHolding;

                        Log("Подписываемся на колбэк 'OnFuturesLimitChange'...");
                        _quik.Events.OnFuturesLimitChange += Events_OnFuturesLimitChange;
                        */
                        /*
                        Log("Подписываемся на колбэк 'OnDepoLimit'...");
                        _quik.Events.OnDepoLimit += Events_OnDepoLimit;

                        Log("Подписываемся на колбэк 'OnMoneyLimit'...");
                        _quik.Events.OnMoneyLimit += Events_OnMoneyLimit;
                        */
                    }
                    ButtonRun.IsEnabled = false;
                }
            }
            catch
            {
                Log("Ошибка получения данных по инструменту.");
            }
        }

        private void Events_OnParam(Param par)
        {
            //NUMCONTRACTS  NUMERIC  Количество открытых позиций
            //NUMBIDS       NUMERIC  Количество заявок на покупку 
            //NUMOFFERS     NUMERIC  Количество заявок на продажу 
            //BIDDEPTHT     NUMERIC  Суммарный спрос 
            //OFFERDEPTHT   NUMERIC  Суммарное предложение 

            if (par.SecCode == "SBER") //  secCode
            {
                //var OI = _quik.Trading.GetParamEx(par.ClassCode, par.SecCode, "NUMCONTRACTS").Result.ParamImage;
                var Kol_pok = _quik.Trading.GetParamEx(par.ClassCode, par.SecCode, "NUMBIDS").Result.ParamImage;
                var Kol_pro = _quik.Trading.GetParamEx(par.ClassCode, par.SecCode, "NUMOFFERS").Result.ParamImage;

                // string str=String.Format("{0} {1} {2}", OI, Kol_pok, Kol_pro);
                // Log(OI + " " + Kol_pok + " " + Kol_pro);

                try
                {
                    if (!string.IsNullOrEmpty(Kol_pok)) _sber.KolPok = Int32.Parse(Kol_pok);
                    if (!string.IsNullOrEmpty(Kol_pro)) _sber.KolPrd = Int32.Parse(Kol_pro);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                this.Dispatcher.Invoke(() =>
                {
                    DataGrid.Items.Refresh();
                });
            }
            if (par.SecCode == "RIH0") //  secCode
            {
                //var OI = _quik.Trading.GetParamEx(par.ClassCode, par.SecCode, "NUMCONTRACTS").Result.ParamImage;
                var Kol_pok = _quik.Trading.GetParamEx(par.ClassCode, par.SecCode, "NUMBIDS").Result.ParamImage;
                var Kol_pro = _quik.Trading.GetParamEx(par.ClassCode, par.SecCode, "NUMOFFERS").Result.ParamImage;

                // string str=String.Format("{0} {1} {2}", OI, Kol_pok, Kol_pro);
                // Log(OI + " " + Kol_pok + " " + Kol_pro);

                try
                {
                    if (!string.IsNullOrEmpty(Kol_pok)) _RI.KolPok = Int32.Parse(Kol_pok);
                    if (!string.IsNullOrEmpty(Kol_pro)) _RI.KolPrd = Int32.Parse(Kol_pro);
                    //if (!string.IsNullOrEmpty(OI)) _RI.OI = OI;  //  Int32.Parse(OI); перенесено в AllTrades
                    //_RI.OI = OI;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                this.Dispatcher.Invoke(() =>
                {
                    DataGrid.Items.Refresh();
                });
            }
        }

        private void Events_OnAllTrade(AllTrade allTrade)
        {
            // Log(allTrade.TradeNum + " " + allTrade.SecCode + " " + allTrade.Price + "" + allTrade.Qty + " " +
            //    allTrade.Flags + " " + allTrade.OpenInterest);

            if (allTrade.SecCode == "SBER")    // SBER secCode
            {
                _sber.Name = allTrade.SecCode;
                string str = allTrade.Datetime.hour.ToString("00") + ":" + allTrade.Datetime.min.ToString("00") + ":" +
                                allTrade.Datetime.sec.ToString("00");
                _sber.Time = str;
                _sber.Price = (decimal)allTrade.Price;
                _sber.Volume = (int)allTrade.Qty;
                _sber.Id = allTrade.TradeNum;
                if (allTrade.Flags == AllTradeFlags.Buy) _sber.Direction = "Buy";
                if (allTrade.Flags == AllTradeFlags.Sell) _sber.Direction = "Sell";

                this.Dispatcher.Invoke(() =>
                {
                    DataGrid.Items.Refresh();
                });

                if (_flagRobotTick)
                {
                    try
                    {
                        var pos = GetOpenPositions(secCode);
                        if ((decimal)allTrade.Price >= Up && Up != 0 && pos <= 0 && _flag) // лонгуем, если 0 или шорт
                        {
                            // покупаем
                            _flag = false; // флаг входа в позицию
                            _quik.Orders.SendMarketOrder(classCode, secCode, _tool.AccountID, Operation.Buy, Math.Abs(pos) + 1); // и сразу переворачиваемся
                            Log("Buy Price= " + allTrade.Price + " Откр. поз " + pos);
                        }

                        if ((decimal)allTrade.Price <= Low && Low != 0 && pos >= 0 && _flag) // и наоборот
                        {
                            // продаем
                            _flag = false;
                            _quik.Orders.SendMarketOrder(classCode, secCode, _tool.AccountID, Operation.Sell, Math.Abs(pos) + 1);
                            Log("Sell Price= " + allTrade.Price + " Откр. поз " + pos);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        // throw;
                    }

                }
            }
            if (allTrade.SecCode == "RIH0")    //  secCode
            {
                _RI.Name = allTrade.SecCode;
                string str = allTrade.Datetime.hour.ToString("00") + ":" + allTrade.Datetime.min.ToString("00") + ":" +
                             allTrade.Datetime.sec.ToString("00");
                _RI.Time = str;
                _RI.Price = (decimal)allTrade.Price;
                _RI.Volume = (int)allTrade.Qty;
                _RI.Id = allTrade.TradeNum;
                if (allTrade.Flags == AllTradeFlags.Buy) _RI.Direction = "Buy";
                if (allTrade.Flags == AllTradeFlags.Sell) _RI.Direction = "Sell";
                _RI.OI = (int)allTrade.OpenInterest;

                this.Dispatcher.Invoke(() =>
                {
                    DataGrid.Items.Refresh();
                });
            }
        }

        private int GetOpenPositions(string secCode)
        {
            var pos = _quik.Trading.GetDepoLimits(secCode).Result; //   [1].CurrentBalance / _tool.Lot;
            if (pos != null && pos.Count > 0)
                return (int)pos[1].CurrentBalance / _tool.Lot;

            return 0;
        }

        private void Events_OnOrder(Order order)
        {
            Log("OrderNum: " + order.OrderNum + ", TransID: " + order.TransID + ", состояние: " + order.State);

            if (order.TransID > 0)
            {
                //  SellStopLimit(order.Price - 10*_tool.Step);
                _flag = true;
                Log("Все ОК. Флаг поднят.");
            }
        }

        private void Candles_NewCandle(Candle candle) // было раньше (по новой свечке открываем позицию)
        {
            if (candle.SecCode == secCode)
            {
                _candlesList.Insert(0, candle);
                if (_candlesList.Count > 2)
                {
                    if (_candlesList[0].Close > _candlesList[1].Close)
                    {
                        //  что-то делаем
                    }
                }

                var ttt = TimeToString(candle.Datetime);
                Log(ttt + " " + candle.SecCode + " " + candle.ToString());

                if (candle.Open == candle.Close && candle.High == candle.Close && candle.Low != candle.Close)
                {
                    Log("Пришла свеча Висельник");
                }

                if ((candle.Close - candle.Open) > 0 && (candle.Close - candle.Open) < 2 * _tool.Step && candle.High == candle.Close && candle.Low != candle.Close)
                {
                    Log("Пришла свеча ... Эскимо");
                }

                //Тег для которого будем считать количество линий
                string tag = "IND";
                // Получаем количество линий (свечек ??)
                var N = _quik.Candles.GetNumCandles(tag).Result;

                Up = _quik.Candles.GetCandles("IND", 0, N - 2, 2).Result[0].Close; // -2 : предыдущ свеча
                Centr = _quik.Candles.GetCandles("IND", 1, N - 2, 2).Result[0].Close;
                Low = _quik.Candles.GetCandles("IND", 2, N - 2, 2).Result[0].Close;
                // Выводим в окно сообщений
                Log(tag + " " + N + " " + Up + " " + Centr + " " + Low);

                if (_flagRobot && InMarket())
                {
                    // запуск робота
                    ShortStopLimit(Low);
                    LongStopLimit(Up);
                }
            }
        }

        private string TimeToString(QuikDateTime t)
        {
            string str = t.day.ToString("00") + ":" + t.month.ToString("00") + ":" + t.year.ToString() + " " +
                         t.hour.ToString("00") + ":" + t.min.ToString("00") + ":" + t.sec.ToString("00") + "." +
                         t.mcs.ToString("000");
            return str;
        }

        private void SellStopLimit(decimal price)
        {
            // decimal priceIn = Math.Round(_tool.LastPrice + 1 * _tool.Step, _tool.PriceAccuracy);
            decimal priceIn = Math.Round(price, _tool.PriceAccuracy);

            StopOrder orderNew = new StopOrder()
            {
                Account = _tool.AccountID,
                ClassCode = _tool.ClassCode,
                ClientCode = clientCode,
                SecCode = secCode,

                StopOrderType = StopOrderType.StopLimit,
                Condition = Condition.LessOrEqual,
                Price = priceIn - 30 * _tool.Step,
                ConditionPrice = Math.Round(priceIn - 20 * _tool.Step, _tool.PriceAccuracy),
                Operation = Operation.Sell,

                Quantity = 1
            };

            try
            {
                var res = _quik.StopOrders.CreateStopOrder(orderNew).Result;
                Log("Результат выставления SellStopLimit: TransID" + res);
            }
            catch (Exception exception)
            {
                Log("Ошибка выставления заявки " + exception.ToString());
            }
        }
        private void TakeProfit_Click(object sender, RoutedEventArgs e)
        {
            decimal priceIn = Math.Round(_tool.LastPrice + 1 * _tool.Step, _tool.PriceAccuracy);
            //decimal priceIn = Math.Round(price, _tool.PriceAccuracy);

            StopOrder orderNew = new StopOrder()
            {
                Account = _tool.AccountID,
                ClassCode = _tool.ClassCode,
                ClientCode = clientCode,
                SecCode = secCode,

                Spread = 10,
                Offset = 10,
                SpreadUnits = OffsetUnits.PERCENTS,
                OffsetUnits =OffsetUnits.PRICE_UNITS,

                StopOrderType = StopOrderType.TakeProfit,
                Condition = Condition.LessOrEqual,
                Price = priceIn - 30 * _tool.Step,
                ConditionPrice = Math.Round(priceIn + 20 * _tool.Step, _tool.PriceAccuracy),
                Operation = Operation.Sell,

                Quantity = 1
            };

            

            try
            {
                var res = _quik.StopOrders.CreateStopOrder(orderNew).Result;
                Log("Результат выставления TakeProfit: TransID" + res);
            }
            catch (Exception exception)
            {
                Log("Ошибка выставления заявки " + exception.ToString());
            }
        }
        private void StopOrder_Click(object sender, RoutedEventArgs e)
        {
            decimal priceIn = Math.Round(_tool.LastPrice - 20 * _tool.Step, _tool.PriceAccuracy);
            SellStopLimit(priceIn);
        }

        private void RunRobot_Click(object sender, RoutedEventArgs e)
        {
            if (!_flagRobot)
            {
                _flagRobot = true;
                //RunRobot.IsEnabled = false;
                RunRobot.Content = "Остановить";
                RunRobot.Background = Brushes.Aqua;
                Log("Робот запущен");
            }
            else
            {
                Log("Робот остановлен");
                KillAll(); //   все закрываем при остановке
                RunRobot.Content = "Новости";
                var bc = new BrushConverter();
                RunRobot.Background = (Brush)bc.ConvertFrom("#FFDDDDDD"); // "#FFDDDDDD"
                _flagRobot = false;
            }
        }

        private void Events_OnQuote(OrderBook orderbook)
        {
            /*
            if (orderbook.sec_code == secCode)
            {
                var bestBid = orderbook.bid[orderbook.bid.Length - 1].price;
                var bestAsk = orderbook.offer[0].price;
                var bestBidQty = orderbook.bid[orderbook.bid.Length - 1].quantity;
                var bestAskQty = orderbook.offer[0].quantity;
                string output = String.Format("{0}---{1}***{2}---{3}", bestBid, bestBidQty, bestAsk, bestAskQty);
                Log(output);
                EnterLong((decimal)bestBid + _tool.Step);
                EnterShort((decimal)bestAsk - _tool.Step);
            }
            */
            //throw new NotImplementedException();
        }

        private void CloseAll_Click(object sender, RoutedEventArgs e)
        {
            KillAll();
        }

        private void KillAll()
        {
            try
            {
                var orders = _quik.Orders.GetOrders(classCode, secCode).Result;
                foreach (var order in orders)
                {
                    if (order.State == State.Active)
                        _quik.Orders.KillOrder(order);
                }
                Log("Kill All Orders");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // throw;
            }

            try
            {
                var stopOrders = _quik.StopOrders.GetStopOrders(classCode, secCode).Result;
                foreach (var stopOrder in stopOrders)
                {
                    if (stopOrder.State == State.Active)
                        _quik.StopOrders.KillStopOrder(stopOrder);
                }
                Log("Kill All StopOrders");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // throw;
            }

            // закрыть открытые позиции?
            try
            {
                var pos = _quik.Trading.GetDepoLimits(secCode).Result[1].CurrentBalance / _tool.Lot;
                Log("Текущая позиция в лотах: " + pos.ToString());
                if (pos != 0)
                {
                    if (pos > 0)
                    {
                        _quik.Orders.SendMarketOrder(classCode, secCode, _tool.AccountID, Operation.Sell, (int)pos);
                    }

                    if (pos < 0)
                    {
                        _quik.Orders.SendMarketOrder(classCode, secCode, _tool.AccountID, Operation.Buy, (int)-pos);
                    }
                    Log("Close All Positions");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // throw;
            }
        }

        private bool InMarket() // возвращает true, если не в рынке
        {
            bool result = true;

            var orders = _quik.Orders.GetOrders(classCode, secCode).Result;
            foreach (var order in orders)
            {
                if (order.State == State.Active)
                {
                    Log("Есть активный ордер");
                    return false;
                }
            }

            var stopOrders = _quik.StopOrders.GetStopOrders(classCode, secCode).Result;
            foreach (var stopOrder in stopOrders)
            {
                if (stopOrder.State == State.Active)
                {
                    Log("Есть активный стоп-ордер");
                    return false;
                }
            }

            var temp = _quik.Trading.GetDepoLimits(secCode).Result;

            var pos = _quik.Trading.GetDepoLimits(secCode).Result[1].CurrentBalance;
            if (pos != 0)
            {
                Log("Есть открытая позиция");
                return false;
            }

            Log("Все ОК, мы не в рынке");
            return result;
        }

        private void Limit_Click(object sender, RoutedEventArgs e)
        {
            // ниже рынка
            var price = Math.Round(_tool.LastPrice - 20 * _tool.Step, _tool.PriceAccuracy);
            EnterLong(price);
            // выше рынка
            price = Math.Round(_tool.LastPrice + 20 * _tool.Step, _tool.PriceAccuracy);
            EnterShort(price);
        }

        private void StopLimit_Click(object sender, RoutedEventArgs e)
        {
            // ниже рынка
            var price = Math.Round(_tool.LastPrice + 20 * _tool.Step, _tool.PriceAccuracy);
            LongStopLimit(price);
            // выше рынка
            price = Math.Round(_tool.LastPrice - 20 * _tool.Step, _tool.PriceAccuracy);
            ShortStopLimit(price);
        }
        private void ShortStopLimit(decimal price)
        {
            decimal priceIn = Math.Round(price, _tool.PriceAccuracy);
            StopOrder orderNew = new StopOrder()
            {
                Account = _tool.AccountID,
                ClassCode = _tool.ClassCode,
                ClientCode = clientCode,
                SecCode = secCode,

                StopOrderType = StopOrderType.StopLimit,
                Condition = Condition.LessOrEqual,
                Price = Math.Round(priceIn - 50 * _tool.Step, _tool.PriceAccuracy),
                ConditionPrice = priceIn,
                Operation = Operation.Sell,

                Quantity = 1
            };

            _quik.StopOrders.CreateStopOrder(orderNew);

        }
        private void LongStopLimit(decimal price)
        {
            decimal priceIn = Math.Round(price, _tool.PriceAccuracy);
            StopOrder orderNew = new StopOrder()
            {
                Account = _tool.AccountID,
                ClassCode = _tool.ClassCode,
                ClientCode = clientCode,
                SecCode = secCode,

                StopOrderType = StopOrderType.StopLimit,
                Condition = Condition.MoreOrEqual,
                Price = Math.Round(priceIn + 50 * _tool.Step, _tool.PriceAccuracy),
                ConditionPrice = priceIn,
                Operation = Operation.Buy,

                Quantity = 1
            };

            _quik.StopOrders.CreateStopOrder(orderNew);

        }

        private void RobotTick_Click(object sender, RoutedEventArgs e)
        {
            //  проигрываем звук
            SoundPlayer simpleSound = new SoundPlayer(@"C:\Windows\Media\chimes.wav"); // C:\Users\Papa\Music\атака.wav
            simpleSound.Play();

            if (!_flagRobotTick)
            {
                _flagRobotTick = true;
                _flag = true;
                //RobotTick.IsEnabled = false;
                RobotTick.Content = "Остановить";
                RobotTick.Background = Brushes.Aqua;
                Log("Робот Тики запущен");
            }
            else
            {
                Log("Робот остановлен");
                KillAll(); //   все закрываем при остановке
                RobotTick.Content = "НовостиТики";
                var bc = new BrushConverter();
                RobotTick.Background = (Brush)bc.ConvertFrom("#FFDDDDDD"); // "#FFDDDDDD"
                _flagRobotTick = false;
                _flag = false;
            }
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            _sber = new MyTick();
            _RI = new MyTick();
            ttt.Add(_sber);
            ttt.Add(_RI);
            DataGrid.ItemsSource = ttt;


            DataGrid.Columns[0].Header = "Id";
            DataGrid.Columns[1].Header = "Инстр.";
            DataGrid.Columns[2].Header = "Время";
            DataGrid.Columns[3].Header = "Цена";
            DataGrid.Columns[4].Header = "Объем";
            DataGrid.Columns[5].Header = "Направ";

            DataGrid.Items.Refresh();
        }

        private void Events_OnMoneyLimit(MoneyLimitEx mLimit)
        {
            Log("Изменилась позиция по деньгам: " + mLimit.CurrentBal);

            _pos.OpenBal = mLimit.OpenBal;
            _pos.CurrentBal = mLimit.CurrentBal;
            _pos.Locked = mLimit.Locked;

            //this.Dispatcher.Invoke(() =>
            //{
            //    PositionGrid.Items.Refresh();
            //});
        }
        private void Events_OnDepoLimit(DepoLimitEx dLimit)
        {
            if (dLimit.LimitKindInt == 0)
            {
                Log("Изменилась позиция по бумагам: " + dLimit.SecCode + " " + dLimit.CurrentBalance);
            }
            if (dLimit.SecCode == secCode && dLimit.LimitKindInt == 0) // T0
            {
                _dep.CurrentBalance = dLimit.CurrentBalance;
                _dep.AweragePositionPrice = dLimit.AweragePositionPrice;
            }

            //this.Dispatcher.Invoke(() =>
            //{
            //    DepoGrid.Items.Refresh();
            //});
        }

        private void Events_OnFuturesClientHolding(FuturesClientHolding futPos) // Позиции по клиентским счетам (фьючерсы)
        {
            Log("Изменилась таблица Позиций по клиентским счетам (фьючерсы): " + futPos.ToString());
            //throw new NotImplementedException();
        }

        private void Events_OnFuturesLimitChange(FuturesLimits futLimit)
        {
            Log("Изменилась таблица Ограничений по клиентским счетам: " + futLimit.CbpLimit);

            //throw new NotImplementedException();

            //var pos = _quik.Trading.GetFuturesHolding("SPBFUT000000", "SPBFUT001h6", "RIH0", 1).Result;
            // var pos = _quik.Trading.GetFuturesLimits
        }
        private void Log(string str)    // потоки
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    LogTextBox.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") + " " + str + Environment.NewLine);
                    LogTextBox.ScrollToLine(LogTextBox.LineCount - 1);  // прокрутка scroll
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (_quik != null)
                _quik.StopService();
            base.OnClosing(e);
        }
        private void ButtonBuy_Click(object sender, RoutedEventArgs e)
        {
            var price = Math.Round(_tool.LastPrice + 10 * _tool.Step, _tool.PriceAccuracy);
            EnterLong(price);
        }

        private void PositionGrid_Loaded(object sender, RoutedEventArgs e)
        {
            listPos.Add(_pos);
            //PositionGrid.ItemsSource = listPos;
        }

        private void DepoGrid_Loaded(object sender, RoutedEventArgs e)
        {
            _dep.SecCode = secCode;
            listDep.Add(_dep);
            //DepoGrid.ItemsSource = listDep;
        }

        private void DataGridBind_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            _depBind.SecCode = secCode;
            _depBind.CurrentBalance = 9999;
            listDepBind.Add(_depBind);
            */
            //DataGridBind.ItemsSource = listDepBind;
        }

        private void TakeProfitStopLimit_Click(object sender, RoutedEventArgs e)
        {
            decimal priceIn = Math.Round(_tool.LastPrice + 1 * _tool.Step, _tool.PriceAccuracy);
            //decimal priceIn = Math.Round(price, _tool.PriceAccuracy);

            StopOrder orderNew = new StopOrder()
            {
                Account = _tool.AccountID,
                ClassCode = _tool.ClassCode,
                ClientCode = clientCode,
                SecCode = secCode,

                Spread = 10,
                Offset = 10,
                SpreadUnits = OffsetUnits.PERCENTS,
                OffsetUnits = OffsetUnits.PRICE_UNITS,

                StopOrderType = StopOrderType.TakeProfitStopLimit,
                Condition = Condition.LessOrEqual,
                Price = priceIn - 30 * _tool.Step,
                ConditionPrice = Math.Round(priceIn + 20 * _tool.Step, _tool.PriceAccuracy),
                Stopprice2 = Math.Round(priceIn - 20 * _tool.Step, _tool.PriceAccuracy),

                Operation = Operation.Sell,

                Quantity = 1
            };



            try
            {
                var res = _quik.StopOrders.CreateStopOrder(orderNew).Result;
                Log("Результат выставления TakeProfitStopLimit: TransID" + res);
            }
            catch (Exception exception)
            {
                Log("Ошибка выставления заявки " + exception.ToString());
            }
        }

        private void ButtonSell_Click(object sender, RoutedEventArgs e)
        {
            var price = Math.Round(_tool.LastPrice - 10 * _tool.Step, _tool.PriceAccuracy);
            EnterShort(price);
            /*
            try
            {
                //var ttt = _quik.Orders.SendMarketOrder(classCode, secCode, _tool.AccountID, Operation.Sell, 1);
                decimal priceIn = Math.Round(_tool.LastPrice + 10 * _tool.Step, _tool.PriceAccuracy);
                var ttt = _quik.Orders.SendLimitOrder(classCode, secCode, _tool.AccountID, Operation.Sell, priceIn, 1);

                Order orderNew = new Order()
                {
                    ClassCode = _tool.ClassCode,
                    SecCode = _tool.SecurityCode,
                    Operation = Operation.Sell,
                    Price = priceIn,
                    Quantity = 1,
                    Account = _tool.AccountID
                };

                try
                {
                    var res = _quik.Orders.CreateOrder(orderNew);
                    Log("Результат " + res.ToString());
                }
                catch
                {
                    Log("Неудачная попытка отправки заявки");
                }

                Log("OrderNum: " + ttt.Result.OrderNum + ", Идентификатор транзакции: " + ttt.Result.TransID);
            }
            catch (Exception exception)
            {
                Log("Ошибка выставления заявки " + exception);
            }
            */
        }
        private void EnterLong(decimal priceIn)
        {
            //Log(priceIn.ToString());
            try
            {
                _quik.Orders.SendLimitOrder(classCode, secCode, _tool.AccountID, Operation.Buy, priceIn, 1); // лимитная
                // var res = _quik.Orders.SendMarketOrder(classCode, secCode, _tool.AccountID, Operation.Buy, 1);
                //Log("OrderNum: " + res.Result.OrderNum + ", Идентификатор транзакции: " + res.Result.TransID); // перенесено в OnOrder
            }
            catch (Exception exception)
            {
                Log("Ошибка выставления заявки " + exception);
            }
        }
        private void EnterShort(decimal priceIn)
        {
            Log(priceIn.ToString());
            try
            {
                _quik.Orders.SendLimitOrder(classCode, secCode, _tool.AccountID, Operation.Sell, priceIn, 1); // лимитная
                //var ttt = _quik.Orders.SendMarketOrder(classCode, secCode, _tool.AccountID, Operation.Buy, 1);
                //Log("OrderNum: " + ttt.Result.OrderNum + ", Идентификатор транзакции: " + ttt.Result.TransID); // перенесено в OnOrder
            }
            catch (Exception exception)
            {
                Log("Ошибка выставления заявки " + exception);
            }
        }

    }
}
