#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.WorkingStrategies
{
	public class MACD_RSI : Strategy
	{
		private RSI RSI1;
		private MACD MACD1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "MACD_RSI";
				Calculate									= Calculate.OnPriceChange;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 14;
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
				SetTrailStop(CalculationMode.Ticks, 5);
				SetProfitTarget(CalculationMode.Ticks, 10);
			}
			
			else if (State == State.DataLoaded)
			{				
				RSI1										= RSI(Close, 12, 3);
				RSI1.Plots[0].Brush 						= Brushes.DodgerBlue;
				RSI1.Plots[1].Brush 						= Brushes.Goldenrod;
				AddChartIndicator(RSI1);
				
				MACD1										= MACD(Close, 14, 26, 9);
				MACD1.Plots[0].Brush 						= Brushes.DarkCyan;
				MACD1.Plots[1].Brush 						= Brushes.Crimson;
				MACD1.Plots[2].Brush 						= Brushes.DodgerBlue;
				AddChartIndicator(MACD1);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if ((RSI1.Default[0] <= 30)
				 && (CrossAbove(MACD1.Default, MACD1.Avg, 1)))
			{
				EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
			}
			
			 // Set 2
			if ((RSI1.Default[0] >= 70)
				 && (CrossBelow(MACD1.Default, MACD1.Avg, 1)))
			{
				EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
			}
			
			
			//Highlighting
			if (Position.MarketPosition == MarketPosition.Long)
			{
				BackBrushAll = Brushes.MediumAquamarine;
			}
			
			if (Position.MarketPosition == MarketPosition.Short)
			{
				BackBrushAll = Brushes.Thistle;
			}
			
		}
	}
}
