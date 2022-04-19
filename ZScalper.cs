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
	public class ZScalper : Strategy
	{
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaZScore amaZScore1;
		private OIR 	OIR1;
		
		private double	AvgPxLong;
		private double	AvgPxShort;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "ZScalper";
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
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 30;
				IsInstantiatedOnEachOptimizationIteration	= true;
				//
				OIR_Threshold								= .6;
				Z_Threshold									= 3;
				Per											= 30;
				PT											= 1;
				Stop										= 10;
				
			}
			
			else if (State == State.Configure)
			{
			}
			
			else if (State == State.DataLoaded)
			{
				OIR1										= OIR();
				OIR1.Plots[0].Brush 						= Brushes.DarkCyan;
				AddChartIndicator(OIR1);
				
				amaZScore1				  					= amaZScore(Close, Convert.ToInt32(Per), 3, -3);
				amaZScore1.Plots[0].Brush 					= Brushes.Gray;
				AddChartIndicator(amaZScore1);
			}
			
		}
		
		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
		{
			if (order.Name == "Long")
				AvgPxLong = order.AverageFillPrice;
			
			if (order.Name == "Short")
				AvgPxShort = order.AverageFillPrice;
		}

		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			if (CurrentBar > 0)
				
			// // // // // Entries // // // // //
			if (Position.MarketPosition == MarketPosition.Flat)
				
				// Long
				if ((amaZScore1.ZScore[0] > Z_Threshold))
					//&& (OIR1[0] > OIR_Threshold))
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
					Print("ZScore: " + amaZScore1.ZScore[0]);
					Print("OIR: " + OIR1[0]);
				}
			
			 	// Short
				else if ((amaZScore1.ZScore[0] < -Z_Threshold))
					//&& (OIR1[0] < -OIR_Threshold))
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
					Print("ZScore: " + amaZScore1.ZScore[0]);
					Print("OIR: " + OIR1[0]);
				}
				
			else
				return;
			
		}
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{

			// // // // // Exit long // // // // //
				
			// Initial stop long
			if ((Position.MarketPosition == MarketPosition.Long)
				&& GetCurrentBid() > AvgPxLong - (Stop * TickSize))
			{
				ExitLongStopMarket(0, true, Convert.ToInt32(DefaultQuantity), AvgPxLong - (Stop * TickSize), @"InitialStop", @"Long");
				ExitLongLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxLong + (PT * TickSize), @"PT", @"Long");
			}			
			
			// Saftey stop long
			if ((Position.MarketPosition == MarketPosition.Long)
				&& GetCurrentBid() <= AvgPxLong - (Stop * TickSize))
			{
				ExitLong(0, @"Saftey", @"Long");
			}	
			
			// // // // // Exit short // // // // //
			
			// Initial stop short
			if ((Position.MarketPosition == MarketPosition.Short)
				&& GetCurrentAsk() < AvgPxShort + (Stop * TickSize))
			{
				ExitShortStopMarket(0, true, Convert.ToInt32(DefaultQuantity), AvgPxShort + (Stop * TickSize), @"InitialStop", @"Short");
				ExitShortLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxShort - (PT * TickSize), @"PT", @"Short");
			}
			
			// Saftey stop short
			if ((Position.MarketPosition == MarketPosition.Short)
				&& GetCurrentAsk() >= AvgPxShort + (Stop * TickSize))
			{
				ExitShort(0, @"Saftey", @"Short");
			}	
			
		}
		

		#region Properties
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="OIR_Threshold", Order=1, GroupName="Parameters")]
		public double OIR_Threshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Z_Threshold", Order=2, GroupName="Parameters")]
		public double Z_Threshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Per", Order=3, GroupName="Parameters")]
		public int Per
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PT", Order=4, GroupName="Parameters")]
		public int PT
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stop", Order=5, GroupName="Parameters")]
		public int Stop
		{ get; set; }
		#endregion

	}
}
