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
namespace NinjaTrader.NinjaScript.Strategies
{
	public class EMAxStochxSAR : Strategy
	{
		private EMA EMA1;
		private Stochastics Stochastics1;
		private ParabolicSAR ParabolicSAR1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "EMAxStochxSAR";
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
				TimeInForce									= TimeInForce.Day;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				EMA1				= EMA(Close, 7);
				Stochastics1				= Stochastics(Close, 7, 14, 3);
				ParabolicSAR1				= ParabolicSAR(Close, 0.02, 0.2, 0.02);
				EMA1.Plots[0].Brush = Brushes.Goldenrod;
				Stochastics1.Plots[0].Brush = Brushes.DodgerBlue;
				Stochastics1.Plots[1].Brush = Brushes.Goldenrod;
				ParabolicSAR1.Plots[0].Brush = Brushes.Goldenrod;
				AddChartIndicator(EMA1);
				AddChartIndicator(Stochastics1);
				AddChartIndicator(ParabolicSAR1);
				
				SetStopLoss("", CalculationMode.Ticks, 5, true);
				SetProfitTarget("", CalculationMode.Ticks, 10);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if ((Close[0] > EMA1[0])
				 && (CrossAbove(Stochastics1.K, 20, 1))
				 && (Close[0] > ParabolicSAR1[0]))
			{
				EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
			}
			
			 // Set 2
			if ((Close[0] < EMA1[0])
				 && (CrossBelow(Stochastics1.K, 80, 1))
				 && (Close[0] < ParabolicSAR1[0]))
			{
				EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
			}
			
		}
	}
}
