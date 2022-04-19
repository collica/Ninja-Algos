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
	public class BandRider : Strategy
	{
		private Order 	LongStop				= null; 
		private Order 	ShortStop				= null; 
		
		private double Last;
		private double Bid;
		private double Ask;
		private double AvgPxLong;
		private double AvgPxShort;
		
		private double CurrentBarHigh;
		private double CurrentBarLow;
		private double MFELong;
		private double MFEShort;
		
		private Bollinger Bollinger1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Make a two sided market on the top and bottom bollinger bands";
				Name										= "BandRider";
				Calculate									= Calculate.OnEachTick;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= true;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.High;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Day;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 30;
				IsInstantiatedOnEachOptimizationIteration	= true;
				PrintTo                                     = PrintTo.OutputTab2;
				//
				Stop										= 10;
				TriggerBE									= 2;
				TriggerTrail								= 5;
				MinProfit									= 1;
				Std 										= 3;
				Per 										= 30;
				
			}
			
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Tick, 1);
			}
			
			else if (State == State.DataLoaded)
			{				
				Bollinger1				  					= Bollinger(Close, Std, Convert.ToInt32(Per));
				Bollinger1.Plots[0].Brush 					= Brushes.Goldenrod;
				Bollinger1.Plots[1].Brush 					= Brushes.Goldenrod;
				Bollinger1.Plots[2].Brush 					= Brushes.Goldenrod;
				AddChartIndicator(Bollinger1);
			}
			
		}
		
		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
		{

		    if (order.Name == "LowerBand")
		      	AvgPxLong = order.AverageFillPrice;
		  	if (order.Name == "UpperBand")
		      	AvgPxShort = order.AverageFillPrice;
			
			if (order.Name == "LongStop")
				LongStop = order;
			if (order.Name == "ShortStop")
				ShortStop = order;
									
		}
		
		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;
			
			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
				{
					Last = marketDataUpdate.Price;
				}

				
				
			// entries
			if (Last < (Bollinger1.Lower[0] - (2 * TickSize))) // enter long
				{
					EnterLongStopMarket(1, true, Convert.ToInt32(DefaultQuantity), Bollinger1.Lower[0], @"LowerBand");
				}
					
			else if (Last > (Bollinger1.Upper[0] + ( 2 * TickSize))) // enter short
				{
					EnterShortStopMarket(1, true, Convert.ToInt32(DefaultQuantity), Bollinger1.Upper[0], @"UpperBand");
				}
					
			// modify stops	
			if (Position.MarketPosition == MarketPosition.Long)
				{
					if (Last >= (AvgPxLong + (TriggerBE * TickSize))
						&& (Last < (AvgPxLong + (TriggerTrail * TickSize))))
					{
						ChangeOrder(LongStop, LongStop.Quantity, 0, AvgPxLong + (MinProfit * TickSize)); //Math.Max( (AvgPxLong + (MinProfit * TickSize)), (High[0] - 4 * TickSize))
					}
					
					else if (Last >= (AvgPxLong + (TriggerTrail * TickSize)))
					{
						ChangeOrder(LongStop, LongStop.Quantity, 0, High[0] - (4 * TickSize));
					}
				}
				
				
			if (Position.MarketPosition == MarketPosition.Short)
				{
					if (Last <= (AvgPxShort - (TriggerBE * TickSize))
						&& (Last > (AvgPxShort - (TriggerTrail * TickSize))))
					{
						ChangeOrder(ShortStop, ShortStop.Quantity, 0, AvgPxShort - (MinProfit * TickSize)); //Math.Min( (AvgPxShort - (MinProfit * TickSize)), (Low[0] + 4 * TickSize))
					}
					
					else if (Last <= (AvgPxShort - (TriggerTrail * TickSize)))
					{
						ChangeOrder(ShortStop, ShortStop.Quantity, 0, Low[0] + (4 * TickSize));
					}
					
				}		

				
		}	
				
			
				
				

				 // // // // // Enter // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //
				
				
//					if (Close[0] < Bollinger1.Middle[0])
//						
//					{
//						EnterLongLimit(1, true, Convert.ToInt32(DefaultQuantity), Bollinger1.Lower[0], @"LowerBand");
//					}
//				
//					else if (Close[0] > Bollinger1.Middle[0])
//					{
//						EnterShortLimit(1, true, Convert.ToInt32(DefaultQuantity), Bollinger1.Upper[0], @"UpperBand");
//					}
				
			
			
			
//			else
//				return;
			
			
			
			// Modify stops	
//			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
//				
//				if (Position.MarketPosition == MarketPosition.Long)
//					
//					if (marketDataUpdate.Price >= (AvgPxLong + (TriggerBE * TickSize))
//						&& marketDataUpdate.Price < (AvgPxLong + (TriggerTrail * TickSize)))
//					{
//						ChangeOrder(LongStop, LongStop.Quantity, 0, AvgPxLong + 1 * TickSize);
//					}
//					
//					else if (marketDataUpdate.Price >= (AvgPxLong + (TriggerTrail * TickSize)))
//					{
//						ChangeOrder(LongStop, LongStop.Quantity, 0, MFELong - ((TriggerTrail * TickSize) - (MinProfit * TickSize)));
//					}
//				
//				else if (Position.MarketPosition == MarketPosition.Short)
//					
//					if (marketDataUpdate.Price <= (AvgPxShort - (TriggerBE * TickSize))
//						&& marketDataUpdate.Price > (AvgPxShort - (TriggerTrail * TickSize)))
//					{
//						ChangeOrder(ShortStop, ShortStop.Quantity, 0, AvgPxShort - 1 * TickSize);
//					}
//					
//					else if (marketDataUpdate.Price <= (AvgPxShort - (TriggerTrail * TickSize)))
//					{
//						ChangeOrder(ShortStop, ShortStop.Quantity, 0, MFEShort + ((TriggerTrail * TickSize) - (MinProfit * TickSize)));
//					}
				
		
			
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (Position.MarketPosition == MarketPosition.Long)
			{
				ExitLongStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxLong - (Stop * TickSize), @"LongStop", @"LowerBand");
				ExitLongLimit(1, true, Convert.ToInt32(DefaultQuantity), AvgPxLong + (20 * TickSize), @"PT", @"LowerBand");
			}
			
			else if (Position.MarketPosition == MarketPosition.Short)
			{
				ExitShortStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxShort + (Stop * TickSize), @"ShortStop", @"UpperBand");	
				ExitShortLimit(1, true, Convert.ToInt32(DefaultQuantity), AvgPxShort - (20 * TickSize), @"PT", @"UpperBand");
			}
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stop", Order=1, GroupName="Parameters")]
		public int Stop
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TriggerBE", Order=2, GroupName="Parameters")]
		public int TriggerBE
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TriggerTrail", Order=2, GroupName="Parameters")]
		public int TriggerTrail
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MinProfit", Order=3, GroupName="Parameters")]
		public int MinProfit
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Std", Order=5, GroupName="Parameters")]
		public double Std
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Per", Order=6, GroupName="Parameters")]
		public int Per
		{ get; set; }
		#endregion

	}
}

//check highs/lows https://ninjatrader.com/support/helpGuides/nt8/?onmarketdepth.htm
//move exits to on ex and add logic to modify a single stop... order objects are longstop/shortstop


				 // // // // // Exit long // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //
			
//				if (Position.MarketPosition == MarketPosition.Long)
//				
//					// Saftey stop
//					if (GetCurrentBid() <= AvgPxLong - (Stop * TickSize))
//					{
//						ExitLong(1, @"Saftey", @"LowerBand");
//					}
//				
//					// Inital stop
//					else if (GetCurrentBid() <= AvgPxLong + (MinProfit * TickSize))
//					{
//						ExitLongStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxLong - (Stop * TickSize), @"Stop", @"LowerBand");
//					}
//				
//					// Breakeven
//					else if (GetCurrentBid() > AvgPxLong + (MinProfit * TickSize) && (GetCurrentBid() < AvgPxLong + (TriggerTrail * TickSize)))
//					{
//						ExitLongStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxLong + (MinProfit * TickSize), @"BreakEven", @"LowerBand");
//					}	
//			
//					// Trail
//			 		else if (GetCurrentBid() >= (AvgPxLong + (TriggerTrail * TickSize)))
//					{
//						ExitLongStopMarket(1, true, Convert.ToInt32(DefaultQuantity), MFELong - ((TriggerTrail * TickSize) - (MinProfit * TickSize)), @"Trail", @"LowerBand");
//					}
//			
//				 // // // // // Exit short // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //
//			
//			 
//				if (Position.MarketPosition == MarketPosition.Short)
//
//					// Saftey stop
//					if (GetCurrentAsk() >= AvgPxShort + (Stop * TickSize))
//					{
//						ExitShort(1, @"Saftey", @"UpperBand");
//					}
//				
//					// Initial stop
//					else if (GetCurrentAsk() >= AvgPxShort - (MinProfit * TickSize))
//					{
//						ExitShortStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxShort + (Stop * TickSize), @"Stop", @"UpperBand");
//					}
//			
//			 		// Breakeven
//					else if ((GetCurrentAsk() < AvgPxShort - (MinProfit * TickSize)) && (GetCurrentAsk() > AvgPxShort - (TriggerTrail * TickSize)))
//					{
//						ExitShortStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxShort - (MinProfit * TickSize), @"BreakEven", @"UpperBand");
//					}	
//			
//			 		// Trail
//					else if (GetCurrentAsk() <= (AvgPxShort - (TriggerTrail * TickSize)))
//					{
//						ExitShortStopMarket(1, true, Convert.ToInt32(DefaultQuantity), MFEShort + ((TriggerTrail * TickSize) - (MinProfit * TickSize)), @"Trail", @"UpperBand");	
//					}








			//CurrentBarHigh = High[0];
			//CurrentBarLow = Low[0];
			
			// Calc MFE long
//			if (BarsSinceEntryExecution(1, @"LowerBand", 0) >= 1)
//			{
//				MFELong = Math.Max((MAX(High, BarsSinceEntryExecution(1, @"LowerBand", 0))[0]), (High[0]));
//				Print("MFE Long: " + MFELong);
//			}
//				
//			else
//			{
//				MFELong = 0;	
//			}
//					
//			// Calc MFE short
//			if (BarsSinceEntryExecution(1, @"UpperBand", 0) >= 1)
//			{
//				MFEShort = Math.Min((MIN(Low, BarsSinceEntryExecution(1, @"UpperBand", 0))[0]), (Low[0]));
//				Print("MFE Short: " + MFEShort);
//			}
//				
//			else
//			{
//				MFEShort = 999999;	
//			}