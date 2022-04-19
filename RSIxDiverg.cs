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
	public class RSIxDiverg : Strategy
	{
		private OBV OBV1;
		private OrderFlowCumulativeDelta OrderFlowCumulativeDelta1;
		
		private RSI RSI_Price;
		private RSI RSI_OBV;
		private RSI RSI_OFCD;
		
		private Order 	LongStop				= null; 
		private Order 	ShortStop				= null; 
		private Order 	LongPT					= null;
		private Order 	ShortPT					= null;
		
		private double AvgPxLong;
		private double AvgPxShort;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "RSIxDiverg";
				Calculate									= Calculate.OnBarClose;
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
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				// Bools
				Use_OBV_RSI									= true;
				Use_OFCD_RSI								= true;
				
				// Slope
				DivergThreshold								= 20;
				
				// RSI
				Per											= 14;
				OBThreshold									= 70;
				
				// Exits
				Stop										= 10;
				PT											= 10;
				
			}
			
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Tick, 1);
			}
			
			else if (State == State.DataLoaded)
			{
				OBV1										= OBV(BarsArray[0]);
				OrderFlowCumulativeDelta1					= OrderFlowCumulativeDelta(Close, NinjaTrader.NinjaScript.Indicators.CumulativeDeltaType.BidAsk, NinjaTrader.NinjaScript.Indicators.CumulativeDeltaPeriod.Session, 0);
				
				RSI_Price									= RSI(Close, Per, 3);
				RSI_Price.Plots[0].Brush 					= Brushes.DodgerBlue;
				RSI_Price.Plots[1].Brush 					= Brushes.Goldenrod;
				AddChartIndicator(RSI_Price);
				
				RSI_OBV										= RSI(OBV1, Per, 3);
				//RSI_OBV.Plots[0].Brush 						= Brushes.DodgerBlue;
				//RSI_OBV.Plots[1].Brush 						= Brushes.Goldenrod;
				//AddChartIndicator(RSI_OBV);
				
				RSI_OFCD									= RSI(OrderFlowCumulativeDelta1.DeltaClose, Per, 3);
				RSI_OFCD.Plots[0].Brush 					= Brushes.DodgerBlue;
				RSI_OFCD.Plots[1].Brush 					= Brushes.Goldenrod;
				AddChartIndicator(RSI_OFCD);
				
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < BarsRequiredToTrade)
				return;
			
			if (CurrentBars[0] < 1)
				return;
			
			//Set trading hours
			TimeSpan StartTime = new TimeSpan(17, 00, 00);
			DateTime Start = DateTime.Today + StartTime;
			
			TimeSpan EndTime = new TimeSpan(14, 59, 50);
			DateTime End = DateTime.Today + EndTime;
			
			// Active hours
			if (DateTime.Now > Start || DateTime.Now < End)
			{
				if (Position.MarketPosition == MarketPosition.Flat
					&& (RSI_OFCD[0] - RSI_Price[0]) > DivergThreshold)
					
					{
						EnterLong(Convert.ToInt32(DefaultQuantity), @"BullDiverg");
						Print(RSI_OFCD[0] - RSI_Price[0]);
					}
				
				if (Position.MarketPosition == MarketPosition.Flat
					&& (RSI_OFCD[0] - RSI_Price[0]) < -DivergThreshold)
					
					{
						EnterShort(Convert.ToInt32(DefaultQuantity), @"BearDiverg");
						Print(RSI_OFCD[0] - RSI_Price[0]);
					}
				
//				if (Position.MarketPosition == MarketPosition.Long
//					&& (RSI_OBV[0] - RSI_Price[0]) < DivergThreshold)
					
//					{
//						ExitLong();
//					}
					
//				if (Position.MarketPosition == MarketPosition.Short
//					&& (RSI_OBV[0] - RSI_Price[0]) > -DivergThreshold)
					
//					{
//						ExitShort();
//					}
					
			}
			
			// Close only
			if (DateTime.Now < Start && DateTime.Now > End)
			{
				if (Position.MarketPosition == MarketPosition.Long)
				{
					ExitLong();
				}
				
				if (Position.MarketPosition == MarketPosition.Short)
				{
					ExitShort();
				}
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
		
		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
		{

		    if (order.Name == "BullDiverg")
		      	AvgPxLong = order.AverageFillPrice;
		  	if (order.Name == "BearDiverg")
		      	AvgPxShort = order.AverageFillPrice;
			
//			if (order.Name == "LongStop")
//				LongStop = order;
//			if (order.Name == "ShortStop")
//				ShortStop = order;
//			if (order.Name == "LongPT")
//				LongPT = order;
//			if (order.Name == "ShortPT")
//				ShortPT = order;
									
		}
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (Position.MarketPosition == MarketPosition.Long)
			{
				ExitLongStopMarket(0, true, Convert.ToInt32(DefaultQuantity), AvgPxLong - (Stop * TickSize), @"LongStop", @"BullDiverg");
				ExitLongLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxLong + (PT * TickSize), @"LongPT", @"BullDiverg");
			}
			
			else if (Position.MarketPosition == MarketPosition.Short)
			{
				ExitShortStopMarket(0, true, Convert.ToInt32(DefaultQuantity), AvgPxShort + (Stop * TickSize), @"ShortStop", @"BearDiverg");	
				ExitShortLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxShort - (PT * TickSize), @"ShortPT", @"BearDiverg");
			}
			
		}
		
		#region Properties
		
		[NinjaScriptProperty]
		[Display(Name="Use_OBV_RSI", Order=3, GroupName="RSI")]
		public bool Use_OBV_RSI
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Use_OFCD_RSI", Order=4, GroupName="RSI")]
		public bool Use_OFCD_RSI
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="DivergThreshold", Order=6, GroupName="RSI")]
		public int DivergThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="OBThreshold", Order=5, GroupName="RSI")]
		public int OBThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Per", Order=1, GroupName="Universal")]
		public int Per
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stop", Order=4, GroupName="Params")]
		public int Stop
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PT", Order=4, GroupName="Params")]
		public int PT
		{ get; set; }
		
		#endregion
		
	}
}
