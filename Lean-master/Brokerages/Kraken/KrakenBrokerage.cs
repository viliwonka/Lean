﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Packets;
using QuantConnect.Securities;
using HistoryRequest = QuantConnect.Data.HistoryRequest;
using Order = QuantConnect.Orders.Order;

namespace QuantConnect.Brokerages.Kraken {
    /// <summary>
    /// Kraken Brokerage implementation
    /// </summary>
    public class KrakenBrokerage : Brokerage, IDataQueueHandler
    {
        private readonly KrakenSymbolMapper _symbolMapper;
        private readonly KrakenApi _api;

        /// <summary>
        /// The maximum number of bars per historical data request
        /// </summary>
        public const int MaxBarsPerRequest = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="KrakenBrokerage"/> class.
        /// </summary>
        /// <param name="accessToken">The Kraken access token (can be the user's personal access token or the access token obtained with OAuth by QC on behalf of the user)</param>
        /// <param name="accountId">The account identifier.</param>
        public KrakenBrokerage(string accessToken, string accountId)
            : base("Kraken Brokerage")
        {

            _api = new KrakenApi(_symbolMapper, accessToken, accountId);

            // forward events received from API
            _api.OrderStatusChanged += (sender, orderEvent) => OnOrderEvent(orderEvent);
            _api.AccountChanged += (sender, accountEvent) => OnAccountChanged(accountEvent);
            _api.Message += (sender, messageEvent) => OnMessage(messageEvent);
        }

        #region IBrokerage implementation

        /// <summary>
        /// Returns true if we're currently connected to the broker
        /// </summary>
        public override bool IsConnected
        {
            get { return _api.IsConnected; }
        }

        /// <summary>
        /// Connects the client to the broker's remote servers
        /// </summary>
        public override void Connect()
        {
            if (IsConnected) return;

            _api.Connect();
        }

        /// <summary>
        /// Disconnects the client from the broker's remote servers
        /// </summary>
        public override void Disconnect()
        {
            _api.Disconnect();
        }

        /// <summary>
        /// Gets all open orders on the account. 
        /// NOTE: The order objects returned do not have QC order IDs.
        /// </summary>
        /// <returns>The open orders returned from Kraken</returns>
        public override List<Order> GetOpenOrders()
        {
            return _api.GetOpenOrders();
        }

        /// <summary>
        /// Gets all holdings for the account
        /// </summary>
        /// <returns>The current holdings from the account</returns>
        public override List<Holding> GetAccountHoldings()
        {
            return new List<Holding>();
            /*
            // Set MarketPrice in each Holding
            var KrakenSymbols = holdings
                .Select(x => _symbolMapper.GetBrokerageSymbol(x.Symbol))
                .ToList();

            if (KrakenSymbols.Count > 0)
            {

                var quotes = _api.GetRates(KrakenSymbols);
                foreach (var holding in holdings)
                {
                    var KrakenSymbol = _symbolMapper.GetBrokerageSymbol(holding.Symbol);
                    Tick tick;
                    if (quotes.TryGetValue(KrakenSymbol, out tick))
                    {
                        holding.MarketPrice = (tick.BidPrice + tick.AskPrice) / 2;
                    }
                }
            }

            return holdings;*/
        }

        /// <summary>
        /// Gets the current cash balance for each currency held in the brokerage account
        /// </summary>
        /// <returns>The current cash balance for each currency available for trading</returns>
        public override List<Cash> GetCashBalance()
        {
            return _api.GetCashBalance();
        }

        /// <summary>
        /// Places a new order and assigns a new broker ID to the order
        /// </summary>
        /// <param name="order">The order to be placed</param>
        /// <returns>True if the request for a new order has been placed, false otherwise</returns>
        public override bool PlaceOrder(Order order)
        {
            return _api.PlaceOrder(order);
        }

        /// <summary>
        /// Updates the order with the same id
        /// </summary>
        /// <param name="order">The new order information</param>
        /// <returns>True if the request was made for the order to be updated, false otherwise</returns>
        public override bool UpdateOrder(Order order)
        {
            return _api.UpdateOrder(order);
        }

        /// <summary>
        /// Cancels the order with the specified ID
        /// </summary>
        /// <param name="order">The order to cancel</param>
        /// <returns>True if the request was made for the order to be canceled, false otherwise</returns>
        public override bool CancelOrder(Order order)
        {
            return _api.CancelOrder(order);
        }

        /// <summary>
        /// Gets the history for the requested security
        /// </summary>
        /// <param name="request">The historical data request</param>
        /// <returns>An enumerable of bars covering the span specified in the request</returns>
        /*public override IEnumerable<BaseData> GetHistory(HistoryRequest request)
        {
            return _api.GetHistory(req);

            if (!_symbolMapper.IsKnownLeanSymbol(request.Symbol))
            {
                Log.Trace("KrakenBrokerage.GetHistory(): Invalid symbol: {0}, no history returned", request.Symbol.Value);
                yield break;
            }

            var exchangeTimeZone = MarketHoursDatabase.FromDataFolder().GetExchangeHours(Market.Kraken, request.Symbol, request.Symbol.SecurityType).TimeZone;

            // Kraken only has 5-second bars, we return these for Resolution.Second
            var period = request.Resolution == Resolution.Second ? TimeSpan.FromSeconds(5) : request.Resolution.ToTimeSpan();

            // set the starting date/time
            var startDateTime = request.StartTimeUtc;

            // loop until last date
            while (startDateTime <= request.EndTimeUtc)
            {

                // request blocks of bars at the requested resolution with a starting date/time
                var quoteBars = _api.DownloadQuoteBars(request.Symbol, startDateTime, request.EndTimeUtc, request.Resolution, exchangeTimeZone).ToList();
                if (quoteBars.Count == 0)
                    break;

                foreach (var quoteBar in quoteBars)
                {
                    yield return quoteBar;
                }

                // calculate the next request datetime
                startDateTime = quoteBars[quoteBars.Count - 1].Time.ConvertToUtc(exchangeTimeZone).Add(period);
            }
        }*/

        #endregion

        #region IDataQueueHandler implementation

        /// <summary>
        /// Get the next ticks from the live trading data queue
        /// </summary>
        /// <returns>IEnumerable list of ticks since the last update.</returns>
        public IEnumerable<BaseData> GetNextTicks()
        {
            return _api.GetNextTicks();
        }

        /// <summary>
        /// Adds the specified symbols to the subscription
        /// </summary>
        /// <param name="job">Job we're subscribing for:</param>
        /// <param name="symbols">The symbols to be added keyed by SecurityType</param>
        public void Subscribe(LiveNodePacket job, IEnumerable<Symbol> symbols)
        {
            _api.Subscribe(job, symbols);
        }

        /// <summary>
        /// Removes the specified symbols from the subscription
        /// </summary>
        /// <param name="job">Job we're processing.</param>
        /// <param name="symbols">The symbols to be removed keyed by SecurityType</param>
        public void Unsubscribe(LiveNodePacket job, IEnumerable<Symbol> symbols)
        {
            _api.Unsubscribe(job, symbols);
        }

        #endregion

        /// <summary>
        /// Returns a DateTime from an RFC3339 string (with microsecond resolution)
        /// </summary>
        /// <param name="time">The time string</param>
        public static DateTime GetDateTimeFromString(string time)
        {
            return DateTime.ParseExact(time, "yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Downloads a list of TradeBars at the requested resolution
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="startTimeUtc">The starting time (UTC)</param>
        /// <param name="endTimeUtc">The ending time (UTC)</param>
        /// <param name="resolution">The requested resolution</param>
        /// <param name="requestedTimeZone">The requested timezone for the data</param>
        /// <returns>The list of bars</returns>
        public IEnumerable<TradeBar> DownloadTradeBars(Symbol symbol, DateTime startTimeUtc, DateTime endTimeUtc, Resolution resolution, DateTimeZone requestedTimeZone)
        {
            return _api.DownloadTradeBars(symbol, startTimeUtc, endTimeUtc, resolution, requestedTimeZone);
        }
        
    }
}
