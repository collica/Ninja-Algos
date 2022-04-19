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
	public class OIR_VOI_Algo : Strategy
	{
		private OIR OIR1;
		private VOI VOI1;
		
		private double	AvgPxLong;
		private double	AvgPxShort;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "OIR_VOI_Algo";
				Calculate									= Calculate.OnEachTick;
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
				BarsRequiredToTrade							= 1;
				IsInstantiatedOnEachOptimizationIteration	= true;
				VOI_Threshold								= 50;
				OIR_Threshold								= .7;
				PT											= 1;
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Tick, 1);
				AddDataSeries("ES 12-20", BarsPeriodType.Tick, 1, MarketDataType.Bid); // series 1
				AddDataSeries("ES 12-20", BarsPeriodType.Tick, 1, MarketDataType.Ask); // series 2
			}
			
			else if (State == State.DataLoaded)
			{				
//				VOI1										= VOI();
//				VOI1.Plots[0].Brush 						= Brushes.DarkCyan;
//				AddChartIndicator(VOI1);
				
				OIR1										= OIR();
				OIR1.Plots[0].Brush 						= Brushes.DarkCyan;
				AddChartIndicator(OIR1);
				
			}
			
		}
		
		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
		{
			if (order.Name == "Long")
				AvgPxLong = order.AverageFillPrice;
			
			if (order.Name == "Short")
				AvgPxShort = order.AverageFillPrice;
		}
		
		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < BarsRequiredToTrade)
				return;
			
			if (Position.MarketPosition == MarketPosition.Flat)
			{
					if ((VOI1[0] > VOI_Threshold)
						&& (OIR1[0] > OIR_Threshold))
						{
							EnterLongLimit(0, false, Convert.ToInt32(DefaultQuantity), GetCurrentBid(), @"Long");
							Print("VOI: " + VOI1[0]);
							Print("OIR: " + OIR1[0]);
						}
			
					else if ((VOI1[0] < -VOI_Threshold)
						&& (OIR1[0] < -OIR_Threshold))
						{
							EnterShortLimit(0, false, Convert.ToInt32(DefaultQuantity), GetCurrentAsk(), @"Short");
							Print("VOI: " + VOI1[0]);
							Print("OIR: " + OIR1[0]);
						}
						
			}
			
		}
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
				
			// PT long
			if (Position.MarketPosition == MarketPosition.Long)
			{
				ExitLongLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxLong + (PT * TickSize), @"PT", @"Long");
			}			

			// PT short
			if (Position.MarketPosition == MarketPosition.Short)
			{
				ExitShortLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxShort - (PT * TickSize), @"PT", @"Short");
			}
			
		}
		
		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			// Stop long
			if ((Position.MarketPosition == MarketPosition.Long)
				&& GetCurrentAsk() < AvgPxLong)
			{
				ExitLong(0, @"Stop", @"Long");
			}	
			
			// Stop Short
			if ((Position.MarketPosition == MarketPosition.Short)
				&& GetCurrentBid() > AvgPxShort)
			{
				ExitShort(0, @"Stop", @"Short");
			}	
		}
		
		
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="VOI_Threshold", Order=1, GroupName="Parameters")]
		public int VOI_Threshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="OIR_Threshold", Order=2, GroupName="Parameters")]
		public double OIR_Threshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PT", Order=3, GroupName="Parameters")]
		public int PT
		{ get; set; }
		#endregion
		
	}
}
