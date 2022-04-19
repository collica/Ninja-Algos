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
	public class TheHolyGrail : Strategy
	{
		private ADX ADX1;
		private SMA SMA1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"https://www.tradingsetupsreview.com/the-holy-grail-trading-setup/";
				Name										= "TheHolyGrail";
				Calculate									= Calculate.OnEachTick;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.High;
				OrderFillResolutionType						= BarsPeriodType.Minute;
				OrderFillResolutionValue					= 1;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Day;
				TraceOrders									= true;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;
				ADXPer										= 14;
				SMAPer										= 20;
				Stop										= 10;
			}
			
			else if (State == State.Configure)
			{
			}
			
			else if (State == State.DataLoaded)
			{				
				ADX1										= ADX(Close, Convert.ToInt32(ADXPer));
				SMA1										= SMA(Close, Convert.ToInt32(SMAPer));
				ADX1.Plots[0].Brush 						= Brushes.DarkCyan;
				SMA1.Plots[0].Brush 						= Brushes.Goldenrod;
				AddChartIndicator(ADX1);
				AddChartIndicator(SMA1);
				
				SetTrailStop(CalculationMode.Ticks, Stop);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if ((Position.MarketPosition == MarketPosition.Flat)
				 &&	(ADX1[0] > 30)
				 && (IsRising(ADX1)
				 && (Close[0] > SMA1[0])))
			{
				EnterLongLimit(Convert.ToInt32(DefaultQuantity), SMA1[0], @"Long");
			}
			
			 // Set 2
			if ((Position.MarketPosition == MarketPosition.Flat)
				 && (ADX1[0] > 30)
				 && (IsRising(ADX1)
				 && (Close[0] < SMA1[0])))
			{
				EnterShortLimit(Convert.ToInt32(DefaultQuantity), SMA1[0], @"Short");
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADXPer", Order=1, GroupName="Parameters")]
		public int ADXPer
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SMAPer", Order=2, GroupName="Parameters")]
		public int SMAPer
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stop", Order=3, GroupName="Parameters")]
		public int Stop
		{ get; set; }
		#endregion

	}
}
