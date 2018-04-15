/*
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
using System.Linq;

//https://api.kraken.com/0/public/AssetPairs // API FOR ASSET PAIRS
namespace QuantConnect.Brokerages.Kraken {
    /// <summary>
    /// Provides the mapping between Lean symbols and Kraken symbols.
    /// </summary>
    public class KrakenSymbolMapper : ISymbolMapper
    {
        private bool HasLoadedSymbolsFromApi = false;
        private KrakenRestApi _restApi = null;

        public void UpdateSymbols(KrakenRestApi restApi) {

            Dictionary<string, DataType.AssetPair> dict = restApi.GetAssetPairs();

            foreach(KeyValuePair<string, DataType.AssetPair> pair in dict) {

                // clear previous symbols
                KnownSymbolStrings.Clear();
                KrakenCurrencies.Clear();

                string currencyPair = pair.Key;

                DataType.AssetPair assetPair = pair.Value;

                string baseName = assetPair.AclassBase;
                string quoteName = assetPair.AclassQuote;

                KnownSymbolStrings.Add(currencyPair);

                KrakenCurrencies.Add(baseName);
                KrakenCurrencies.Add(quoteName);
            }
        }

        /// <summary>
        /// The list of known Kraken symbols.
        /// </summary>
        public static readonly HashSet<string> KnownSymbolStrings = new HashSet<string> 
        {
            
        };
        
        /// <summary>
        /// The list of known Kraken currencies.
        /// </summary>
        private static readonly HashSet<string> KrakenCurrencies = new HashSet<string>() {           
        
        };


        private static readonly Dictionary<string, string> LeanToKrakenMap = new Dictionary<string, string>() {

            { "CAD" , "ZCAD" },
            { "EUR" , "ZEUR" },
            { "JPY" , "ZJPY" },
            { "USD" , "ZUSD" }

        };
        
        /// <summary>
        /// Returns array with length of 2. 0 = base currency, 1 = quote currency
        /// </summary>
        /// <param name="symbolString">Source symbol</param>
        /// <param name="length">Length of one currency</param>
        /// <returns></returns>
        private static string[] SymbolStringTo2Currencies(string symbolString, int length) {

            return new string[] {

                symbolString.Substring(0, length),
                symbolString.Substring(length, length)
            };
        }

        /// <summary>
        /// Converts a Lean symbol instance to an Oanda symbol
        /// </summary>
        /// <param name="symbol">A Lean symbol instance</param>
        /// <returns>The Kraken symbol</returns>
        public string GetBrokerageSymbol(Symbol symbol)
        {
            if (symbol == null || string.IsNullOrWhiteSpace(symbol.Value))
                throw new ArgumentException("Invalid symbol: " + (symbol == null ? "null" : symbol.ToString()));

            if (symbol.ID.SecurityType != SecurityType.Forex && symbol.ID.SecurityType != SecurityType.Cfd)
                throw new ArgumentException("Invalid security type: " + symbol.ID.SecurityType);

            var brokerageSymbol = ConvertLeanSymbolToKrakenSymbol(symbol.Value);

            if (!IsKnownBrokerageSymbol(brokerageSymbol))
                throw new ArgumentException("Unknown symbol: " + symbol.Value);

            return brokerageSymbol;
        }

        /// <summary>
        /// Converts an Kraken symbol to a Lean symbol instance
        /// </summary>
        /// <param name="brokerageSymbol">The Kraken symbol</param>
        /// <param name="securityType">The security type</param>
        /// <param name="market">The market</param>
        /// <param name="expirationDate">Expiration date of the security(if applicable)</param>
        /// <param name="strike">The strike of the security (if applicable)</param>
        /// <param name="optionRight">The option right of the security (if applicable)</param>
        /// <returns>A new Lean Symbol instance</returns>
        public Symbol GetLeanSymbol(string brokerageSymbol, SecurityType securityType, string market, DateTime expirationDate = default(DateTime), decimal strike = 0, OptionRight optionRight = 0)
        {
            if (string.IsNullOrWhiteSpace(brokerageSymbol))
                throw new ArgumentException("Invalid Kraken symbol: " + brokerageSymbol);

            if (!IsKnownBrokerageSymbol(brokerageSymbol))
                throw new ArgumentException("Unknown Kraken symbol: " + brokerageSymbol);

            if (securityType != SecurityType.Crypto)
                throw new ArgumentException("Invalid security type: " + securityType);

            if (market != Market.Kraken)
                throw new ArgumentException("Invalid market: " + market);

            return Symbol.Create(ConvertKrakenSymbolToLeanSymbol(brokerageSymbol), GetBrokerageSecurityType(brokerageSymbol), Market.Kraken);
        }

        /// <summary>
        /// Returns the security type for an Kraken symbol
        /// </summary>
        /// <param name="brokerageSymbol">The Kraken symbol</param>
        /// <returns>The security type</returns>
        public SecurityType GetBrokerageSecurityType(string brokerageSymbol)
        {
            var tokens = brokerageSymbol.Split('_');
            if (tokens.Length != 2)
                throw new ArgumentException("Unable to determine SecurityType for Kraken symbol: " + brokerageSymbol);

            return KnownCurrencies.Contains(tokens[0]) && KnownCurrencies.Contains(tokens[1])
                ? SecurityType.Forex : SecurityType.Cfd;

        }

        /// <summary>
        /// Returns the security type for a Lean symbol
        /// </summary>
        /// <param name="leanSymbol">The Lean symbol</param>
        /// <returns>The security type</returns>
        public SecurityType GetLeanSecurityType(string leanSymbol)
        {
            return GetBrokerageSecurityType(ConvertLeanSymbolToKrakenSymbol(leanSymbol));
        }

        /// <summary>
        /// Checks if the symbol is supported by Kraken
        /// </summary>
        /// <param name="brokerageSymbol">The Kraken symbol</param>
        /// <returns>True if Kraken supports the symbol</returns>
        public bool IsKnownBrokerageSymbol(string brokerageSymbol)
        {
            return KnownSymbolStrings.Contains(brokerageSymbol);
        }

        /// <summary>
        /// Checks if the symbol is supported by Kraken
        /// </summary>
        /// <param name="symbol">The Lean symbol</param>
        /// <returns>True if Kraken supports the symbol</returns>
        public bool IsKnownLeanSymbol(Symbol symbol)
        {
            if (symbol == null || string.IsNullOrWhiteSpace(symbol.Value) || symbol.Value.Length <= 3) 
                return false;

            var krakenSymbol = ConvertLeanSymbolToKrakenSymbol(symbol.Value);

            return IsKnownBrokerageSymbol(krakenSymbol) && GetBrokerageSecurityType(krakenSymbol) == symbol.ID.SecurityType;
        }

        /// <summary>
        /// Converts an Kraken symbol to a Lean symbol string
        /// </summary>
        private static string ConvertKrakenSymbolToLeanSymbol(string krakenSymbol)
        {
            string[] krakenPair = SymbolStringTo2Currencies(krakenSymbol, 4);


            return krakenSymbol;

            // Lean symbols are equal to Kraken symbols with underscores removed
            // return krakenSymbol.Replace("_", "");
        }


        /// <summary>
        /// Converts a Lean symbol string to an Kraken symbol
        /// </summary>
        private static string ConvertLeanSymbolToKrakenSymbol(string leanSymbol)
        {

            string[] leanPair = SymbolStringTo2Currencies(leanSymbol, 3);

            // All Kraken symbols end with '_XYZ', where XYZ is the quote currency

            // return leanSymbol.Insert(leanSymbol.Length - 3, "_");
        }
    }
}
