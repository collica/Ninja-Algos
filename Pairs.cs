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
	public class Pairs : Strategy
	{
		private Spread Spread1;
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaZScore amaZScore1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "Pairs";
				Calculate									= Calculate.OnEachTick;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.UniqueEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Day;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.IgnoreAllErrors;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;
				PrintTo                                     = PrintTo.OutputTab2;
				//
				Z_Threshold									= 3;
				Per											= 1320;
				Contract2									= @"ES 03-21";
			}
			
			else if (State == State.Configure)
			{
				AddDataSeries(Contract2, Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);
			}
			
			else if (State == State.DataLoaded)
			{				
				Spread1										= Spread(Close, 1, -1, false, Contract2);
				Spread1.Plots[0].Brush 						= Brushes.ForestGreen;
				AddChartIndicator(Spread1);
				
				amaZScore1									= amaZScore(Spread1, Per, Z_Threshold, -Z_Threshold);
				amaZScore1.Plots[0].Brush 					= Brushes.Gray;
				AddChartIndicator(amaZScore1);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade) 
				return;

			 // Long spread
			if (CrossBelow(amaZScore1.ZScore, -Z_Threshold, 1))
			{
				EnterLong(0, Convert.ToInt32(DefaultQuantity), @"BuyPair");
				EnterShort(1, Convert.ToInt32(DefaultQuantity), @"BuyPairL2");
				
				Print(DateTime.Now.ToString("hh:mm:ss") + " -- Pairs -- ZScore: " + amaZScore1.ZScore[0]);
			}
			
			 // Short spread
			if (CrossAbove(amaZScore1.ZScore, Z_Threshold, 1))
			{
				EnterShort(0, Convert.ToInt32(DefaultQuantity), @"SellPair");
				EnterLong(1, Convert.ToInt32(DefaultQuantity), @"SellPairL2");
				
				Print(DateTime.Now.ToString("hh:mm:ss") + " -- Pairs -- ZScore: " + amaZScore1.ZScore[0]);
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Z_Threshold", Order=1, GroupName="Parameters")]
		public double Z_Threshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Per", Order=2, GroupName="Parameters")]
		public int Per
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Contract2", Order=3, GroupName="Parameters")]
		public string Contract2
		{ get; set; }
		#endregion

	}
}
