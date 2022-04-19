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
	public class BBxRSI : Strategy
	{
		private OBV OBV1;
		private OrderFlowCumulativeDelta OrderFlowCumulativeDelta1;
		
		private RSI RSI_Price;
		private RSI RSI_OBV;
		private RSI RSI_OFCD;
		
		private Bollinger Bollinger1;
		private ADX ADX1;
		
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
				Name										= "BBxRSI";
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
				RealtimeErrorHandling						= RealtimeErrorHandling.IgnoreAllErrors;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				// Bools
				Use_Price_RSI								= true;
				Use_OBV_RSI									= true;
				Use_OFCD_RSI								= true;
				
				// BB
				Std											= 3;
				Per											= 14;

				// RSI
				OBThreshold									= 70;
				Smooth										= 3;
				
				// ADX
				ADXThreshold								= 30;
				
				// Exits
//				Stop										= 10;

				
			}
			
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Tick, 1);
			}
			
			else if (State == State.DataLoaded)
			{
				OBV1										= OBV(Close);
				
				OrderFlowCumulativeDelta1					= OrderFlowCumulativeDelta(Close, NinjaTrader.NinjaScript.Indicators.CumulativeDeltaType.BidAsk, NinjaTrader.NinjaScript.Indicators.CumulativeDeltaPeriod.Session, 0);
				
				RSI_Price									= RSI(Close, Per, Smooth);
				//RSI_Price.Plots[0].Brush 					= Brushes.DodgerBlue;
				//RSI_Price.Plots[1].Brush 					= Brushes.Goldenrod;
				//AddChartIndicator(RSI_Price);
				
				RSI_OBV										= RSI(OBV1, Per, Smooth);
				//RSI_OBV.Plots[0].Brush 					= Brushes.DodgerBlue;
				//RSI_OBV.Plots[1].Brush 					= Brushes.Goldenrod;
				//AddChartIndicator(RSI_OBV);
				
				RSI_OFCD									= RSI(OrderFlowCumulativeDelta1.DeltaClose, Per, Smooth);
				//RSI_OFCD.Plots[0].Brush 					= Brushes.DodgerBlue;
				//RSI_OFCD.Plots[1].Brush 					= Brushes.Goldenrod;
				//AddChartIndicator(RSI_OFCD);
				
				Bollinger1									= Bollinger(Close, Std, Convert.ToInt32(Per));
				Bollinger1.Plots[0].Brush					= Brushes.Goldenrod;
				Bollinger1.Plots[1].Brush 					= Brushes.Goldenrod;
				Bollinger1.Plots[2].Brush 					= Brushes.Goldenrod;
				AddChartIndicator(Bollinger1);
				
				ADX1										= ADX(Close, Convert.ToInt32(Per));
				ADX1.Plots[0].Brush 						= Brushes.DarkCyan;
				AddChartIndicator(ADX1);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
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
				if(Position.MarketPosition == MarketPosition.Flat && Use_Price_RSI == true && Use_OBV_RSI == true && Use_OFCD_RSI == true)
				{
					// Enter long
					if ((Close[0] < Bollinger1.Middle[0])
						&& RSI_Price[0] <= (100 - OBThreshold)
						&& RSI_OBV[0] <= (100 - OBThreshold)
						&& RSI_OFCD[0] <= (100 - OBThreshold)
						&& ADX1[0] <= ADXThreshold)
					{
						EnterLongLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Lower[0], @"LowerBand");
					}
					
					// Enter short
					if ((Close[0] > Bollinger1.Middle[0])
						&& RSI_Price[0] >= OBThreshold
						&& RSI_OBV[0] >= OBThreshold
						&& RSI_OFCD[0] >= OBThreshold
						&& ADX1[0] <= ADXThreshold)
					{
						EnterShortLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Upper[0], @"UpperBand");
					} 
				}
				
				if(Position.MarketPosition == MarketPosition.Flat && Use_Price_RSI == true && Use_OBV_RSI == true && Use_OFCD_RSI == false)
				{
					// Enter long
					if ((Close[0] < Bollinger1.Middle[0])
						&& RSI_Price[0] <= (100 - OBThreshold)
						&& RSI_OBV[0] <= (100 - OBThreshold)
						&& ADX1[0] <= ADXThreshold)
					{
						EnterLongLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Lower[0], @"LowerBand");
					}
					
					// Enter short
					if ((Close[0] > Bollinger1.Middle[0])
						&& RSI_Price[0] >= OBThreshold
						&& RSI_OBV[0] >= OBThreshold
						&& ADX1[0] <= ADXThreshold)
					{
						EnterShortLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Upper[0], @"UpperBand");
					} 
				}
				
				if(Position.MarketPosition == MarketPosition.Flat && Use_Price_RSI == true && Use_OBV_RSI == false && Use_OFCD_RSI == false)
				{
					// Enter long
					if ((Close[0] < Bollinger1.Middle[0])
						&& RSI_Price[0] <= (100 - OBThreshold)
						&& ADX1[0] <= ADXThreshold)
					{
						EnterLongLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Lower[0], @"LowerBand");
					}
					
					// Enter short
					if ((Close[0] > Bollinger1.Middle[0])
						&& RSI_Price[0] >= OBThreshold
						&& ADX1[0] <= ADXThreshold)
					{
						EnterShortLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Upper[0], @"UpperBand");
					} 
				}
				
				if(Position.MarketPosition == MarketPosition.Flat && Use_Price_RSI == false && Use_OBV_RSI == true && Use_OFCD_RSI == true)	
				{
					// Enter long
					if ((Close[0] < Bollinger1.Middle[0])
						&& RSI_OBV[0] <= (100 - OBThreshold)
						&& RSI_OFCD[0] <= (100 - OBThreshold)
						&& ADX1[0] <= ADXThreshold)
					{
						EnterLongLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Lower[0], @"LowerBand");
					}
					
					// Enter short
					if ((Close[0] > Bollinger1.Middle[0])
						&& RSI_OBV[0] >= OBThreshold
						&& RSI_OFCD[0] >= OBThreshold
						&& ADX1[0] <= ADXThreshold)
					{
						EnterShortLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Upper[0], @"UpperBand");
					} 
				}
				
				if(Position.MarketPosition == MarketPosition.Flat && Use_Price_RSI == false && Use_OBV_RSI == true && Use_OFCD_RSI == false)
				{
					// Enter long
					if ((Close[0] < Bollinger1.Middle[0])
						&& RSI_OBV[0] <= (100 - OBThreshold)
						&& ADX1[0] <= ADXThreshold)
					{
						EnterLongLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Lower[0], @"LowerBand");
					}
					
					// Enter short
					if ((Close[0] > Bollinger1.Middle[0])
						&& RSI_OBV[0] >= OBThreshold
						&& ADX1[0] <= ADXThreshold)
					{
						EnterShortLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Upper[0], @"UpperBand");
					} 
				}	
				
				if(Position.MarketPosition == MarketPosition.Flat && Use_Price_RSI == false && Use_OBV_RSI == false && Use_OFCD_RSI == true)
				{
					// Enter long
					if ((Close[0] < Bollinger1.Middle[0])
						&& RSI_OFCD[0] <= (100 - OBThreshold)
						&& ADX1[0] <= ADXThreshold)
					{
						EnterLongLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Lower[0], @"LowerBand");
					}
					
					// Enter short
					if ((Close[0] > Bollinger1.Middle[0])
						&& RSI_OFCD[0] >= OBThreshold
						&& ADX1[0] <= ADXThreshold)
					{
						EnterShortLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Upper[0], @"UpperBand");
					} 
				}
				
					
					
				// Adjust PT
				if ((Position.MarketPosition == MarketPosition.Long)
					&& LongPT != null)
				{
					ChangeOrder(LongPT, LongPT.Quantity, Bollinger1.Middle[0], 0);
				}
					
				if ((Position.MarketPosition == MarketPosition.Short)
					&& ShortPT != null)
				{
					ChangeOrder(ShortPT, ShortPT.Quantity, Bollinger1.Middle[0], 0);	
				}	
				
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

		    if (order.Name == "LowerBand")
		      	AvgPxLong = order.AverageFillPrice;
		  	if (order.Name == "UpperBand")
		      	AvgPxShort = order.AverageFillPrice;
			
			if (order.Name == "LongStop")
				LongStop = order;
			if (order.Name == "ShortStop")
				ShortStop = order;
			if (order.Name == "LongPT")
				LongPT = order;
			if (order.Name == "ShortPT")
				ShortPT = order;
									
		}
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (Position.MarketPosition == MarketPosition.Long)
			{
				//ExitLongStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxLong - (Stop * TickSize), @"LongStop", @"LowerBand");
				ExitLongLimit(1, true, Convert.ToInt32(DefaultQuantity), Bollinger1.Middle[0], @"LongPT", @"LowerBand");
			}
			
			else if (Position.MarketPosition == MarketPosition.Short)
			{
				//ExitShortStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxShort + (Stop * TickSize), @"ShortStop", @"UpperBand");	
				ExitShortLimit(1, true, Convert.ToInt32(DefaultQuantity), Bollinger1.Middle[0], @"ShortPT", @"UpperBand");
			}
		}
				
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Per", Order=1, GroupName="Universal")]
		public int Per
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Use_Price_RSI", Order=2, GroupName="RSI")]
		public bool Use_Price_RSI
		{ get; set; }
		
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
		[Display(Name="OBThreshold", Order=5, GroupName="RSI")]
		public int OBThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smooth", Order=6, GroupName="RSI")]
		public int Smooth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Std", Order=7, GroupName="Bands")]
		public double Std
		{ get; set; }



//		[NinjaScriptProperty]
//		[Range(1, int.MaxValue)]
//		[Display(Name="Stop", Order=4, GroupName="Params")]
//		public int Stop
//		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADXThreshold", Order=8, GroupName="ADX")]
		public int ADXThreshold
		{ get; set; }
		
		#endregion

	}
}


//if ((Close[0] < Bollinger1.Middle[0])
//					&& RSI_Price[0] <= (100 - OBThreshold)
//					&& ADX1[0] <= ADXThreshold)
//				{
//					EnterLongLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Lower[0], @"LowerBand");
//				}
				
//				// Enter short
//				if ((Close[0] > Bollinger1.Middle[0])
//					&& RSI_Price[0] >= OBThreshold
//					&& ADX1[0] <= ADXThreshold)
//				{
//					EnterShortLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Upper[0], @"UpperBand");
//				} 
				
//				if ((Position.MarketPosition == MarketPosition.Long)
//					&& LongPT != null)
//				{
//					ChangeOrder(LongPT, LongPT.Quantity, Bollinger1.Middle[0], 0);
//				}
					
//				if ((Position.MarketPosition == MarketPosition.Short)
//					&& ShortPT != null)
//				{
//					ChangeOrder(ShortPT, ShortPT.Quantity, Bollinger1.Middle[0], 0);	
//				}	