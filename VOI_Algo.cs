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
	public class VOI_Algo : Strategy
	{
		private VOI VOI1;
		private double DeltaBid;
		private double DeltaAsk;
		private double	AvgPxLong;
		private double	AvgPxShort;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "VOI_Algo";
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
				TimeInForce									= TimeInForce.Day;
				TraceOrders									= true;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 3;
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				VOIThreshold								= 100;
				PrimContract								= @"ES 12-20";
				Stop										= 8;
				PT											= 1;
				
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Tick, 1);												   // series 1
				AddDataSeries(Convert.ToString(PrimContract), BarsPeriodType.Tick, 1, MarketDataType.Bid); // series 2
				AddDataSeries(Convert.ToString(PrimContract), BarsPeriodType.Tick, 1, MarketDataType.Ask); // series 3
			}
		
			else if (State == State.DataLoaded)
			{				
				VOI1										= VOI(Close, Convert.ToString(PrimContract));
				VOI1.Plots[0].Brush 						= Brushes.DarkCyan;
				AddChartIndicator(VOI1);
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
			if (CurrentBars[0] >= BarsRequiredToTrade)
				
			{

				if (Position.MarketPosition == MarketPosition.Flat)
				{
					if (VOI1[0] > VOIThreshold)
						{
							EnterLong(1, Convert.ToInt32(DefaultQuantity), @"Long");
						}
			
					else if (VOI1[0] < -VOIThreshold)
						{
							EnterShort(1, Convert.ToInt32(DefaultQuantity), @"Short");
						}
						
				}
				
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
				ExitLongStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxLong - (Stop * TickSize), @"InitialStop", @"Long");
				ExitLongLimit(1, true, Convert.ToInt32(DefaultQuantity), AvgPxLong + (PT * TickSize), @"PT", @"Long");
			}			
			
			// Saftey stop long
			if ((Position.MarketPosition == MarketPosition.Long)
				&& GetCurrentBid() <= AvgPxLong - (Stop * TickSize))
			{
				ExitLong(1, @"Saftey", @"Long");
			}	
			
			// // // // // Exit short // // // // //
			
			// Initial stop short
			if ((Position.MarketPosition == MarketPosition.Short)
				&& GetCurrentAsk() < AvgPxShort + (Stop * TickSize))
			{
				ExitShortStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxShort + (Stop * TickSize), @"InitialStop", @"Short");
				ExitShortLimit(1, true, Convert.ToInt32(DefaultQuantity), AvgPxShort - (PT * TickSize), @"PT", @"Short");
			}
			
			// Saftey stop short
			if ((Position.MarketPosition == MarketPosition.Short)
				&& GetCurrentAsk() >= AvgPxShort + (Stop * TickSize))
			{
				ExitShort(1, @"Saftey", @"Short");
			}	
			
		}
			
		
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="VOIThreshold", Order=1, GroupName="Parameters")]
		public int VOIThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stop", Order=2, GroupName="Parameters")]
		public int Stop
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PT", Order=3, GroupName="Parameters")]
		public int PT
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="PrimContract", Order=4, GroupName="Parameters")]
		public string PrimContract
		{ get; set; }
		#endregion
		
	}
}
