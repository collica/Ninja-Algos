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
	public class MTFxSAR : Strategy
	{
		private ParabolicSAR ParabolicSAR_Primary;
		private ParabolicSAR ParabolicSAR_3m;
		private ParabolicSAR ParabolicSAR_5m;
		private ParabolicSAR ParabolicSAR_10m;
		private ParabolicSAR ParabolicSAR_15m;
		
		
		private Order 	LongStop				= null; 
		private Order 	ShortStop				= null; 

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "MTFxSAR";
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
				RealtimeErrorHandling						= RealtimeErrorHandling.IgnoreAllErrors;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				// Bools
				Use3m										= true;
				Use5m										= true;
				Use10m										= true;
				Use15m										= true;
				
			}
			
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Minute, 3); //Index 1
				AddDataSeries(Data.BarsPeriodType.Minute, 5); //Index 2
				AddDataSeries(Data.BarsPeriodType.Minute, 10); //Index 3
				AddDataSeries(Data.BarsPeriodType.Minute, 15); //Index 4
			}
			
			else if (State == State.DataLoaded)
			{				
				ParabolicSAR_Primary						= ParabolicSAR(BarsArray[0], 0.02, 0.2, 0.02);
				ParabolicSAR_3m								= ParabolicSAR(BarsArray[1], 0.02, 0.2, 0.02);
				ParabolicSAR_5m								= ParabolicSAR(BarsArray[2], 0.02, 0.2, 0.02);
				ParabolicSAR_10m							= ParabolicSAR(BarsArray[3], 0.02, 0.2, 0.02);
				ParabolicSAR_15m							= ParabolicSAR(BarsArray[4], 0.02, 0.2, 0.02);
				
				ParabolicSAR_Primary.Plots[0].Brush 		= Brushes.Goldenrod;
				AddChartIndicator(ParabolicSAR_Primary);
			}
			
		}

		protected override void OnBarUpdate()
		{	
			if (CurrentBars[0] < BarsRequiredToTrade)
				return;
			
			if (CurrentBars[0] < 1 || CurrentBars[1] < 1 || CurrentBars[2] < 1 || CurrentBars[3] < 1 || CurrentBars[4] < 1)
				return;
			
			// Active hours
			if (ToTime(Time[0]) > 170000 || ToTime(Time[0]) < 145900)
			{	
					
				// // // // Enter long // // // //
				// 15 10 5 3 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] 
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] 
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] 
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
					Print(DateTime.Now.ToString("hh:mm:ss") + "  **ENTER LONG**  ");
					Print("SAR_15m -- " + ParabolicSAR_15m[0]);
					Print("SAR_10m -- " + ParabolicSAR_10m[0]);
					Print("SAR_5m -- " + ParabolicSAR_5m[0]);
					Print("SAR_3m -- " + ParabolicSAR_3m[0]);
				}
				
				// 15 10 5  
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] 
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] 
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 15 10 3 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] 
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] 
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 15 5 3  
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] 
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] 
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 15 5 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] 
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 15 3
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] 
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 15 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 10 5 3 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] 
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] 
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 10 5  
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] 
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 10 3  
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] 
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 10
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 5 3
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] 
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 5 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 3
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] 
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				
				// // // // Enter Short // // // //
				// 15 10 5 3 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] 
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] 
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] 
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
					Print(DateTime.Now.ToString("hh:mm:ss") + "  **ENTER SHORT**  ");
					Print("SAR_15m -- " + ParabolicSAR_15m[0]);
					Print("SAR_10m -- " + ParabolicSAR_10m[0]);
					Print("SAR_5m -- " + ParabolicSAR_5m[0]);
					Print("SAR_3m -- " + ParabolicSAR_3m[0]);
				}
				
				// 15 10 5  
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] 
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] 
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 15 10 3 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] 
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] 
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 15 5 3  
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] 
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] 
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 15 5 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] 
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 15 3
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] 
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 15 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 10 5 3 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] 
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] 
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 10 5  
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] 
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 10 3  
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] 
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 10
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 5 3
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] 
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 5 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 3
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0]
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
					

				// // // // 
				
				
//				// Update long stop
//				if ((Position.MarketPosition == MarketPosition.Long)
//					&& (LongStop != null)
//					&& (LongStop.OrderState == OrderState.Accepted))
//				{
//					ChangeOrder(LongStop, LongStop.Quantity, 0, ParabolicSAR_Primary[0]);
//				}
				
//				// Update short stop
//				if ((Position.MarketPosition == MarketPosition.Short)
//					&& (ShortStop != null)
//					&& (ShortStop.OrderState == OrderState.Accepted))
//				{
//					ChangeOrder(ShortStop, ShortStop.Quantity, 0, ParabolicSAR_Primary[0]);
//				}
				
				//Exit long (saftey)
				if ((Position.MarketPosition == MarketPosition.Long)
					&& (Lows[0][0] <= ParabolicSAR_Primary[0]))
				{
					ExitLong();
				}
				
				//Exit short (saftey)
				if ((Position.MarketPosition == MarketPosition.Short)
					&& (Highs[0][0] >= ParabolicSAR_Primary[0]))
				{
					ExitShort();
				}
				
			}
			
			// Close only
			else if (ToTime(Time[0]) > 145900 && ToTime(Time[0]) < 170000)
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
			
			// Highlighting	
			if (Position.MarketPosition == MarketPosition.Long)
			{
				BackBrushAll = Brushes.MediumAquamarine;
			}
			
			if (Position.MarketPosition == MarketPosition.Short)
			{
				BackBrushAll = Brushes.Thistle;
			}
			
		}
		
//		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
//		{
//			if (Position.MarketPosition == MarketPosition.Long)
//			{
//				ExitLongStopMarket(0, true, Convert.ToInt32(DefaultQuantity), ParabolicSAR_Primary[0], "LongStop", "Long");
//			}
				
//			else if (Position.MarketPosition == MarketPosition.Short)
//			{
//				ExitShortStopMarket(0, true, Convert.ToInt32(DefaultQuantity), ParabolicSAR_Primary[0], "ShortStop", "Short");
//			}
			
//		}
		
//		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
//		{
	  	
//			if (order.Name == "LongStop")
//		      	LongStop = order;
//			if (order.Name == "ShortStop")
//				ShortStop = order;

//			if (LongStop != null
//				&& LongStop.OrderState == OrderState.Rejected)
//			{
//				ExitLong();
//			}
			
//			if (ShortStop != null
//				&& ShortStop.OrderState == OrderState.Rejected)
//			{
//			    ExitShort();
//			}
			  
//		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Use3m", Order=2, GroupName="Parameters")]
		public bool Use3m
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use5m", Order=3, GroupName="Parameters")]
		public bool Use5m
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use10m", Order=4, GroupName="Parameters")]
		public bool Use10m
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use15m", Order=5, GroupName="Parameters")]
		public bool Use15m
		{ get; set; }
		#endregion

	}
}


				
				
				
//				// Enter long
//				if ((Low[0] > ParabolicSAR_Primary[0])
					
//					&& 
					
//					  ((((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0])) 
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0])) 
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0])) 
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0]))
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]))
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0]))
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]))
//					|| (((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0])) 
//					|| (((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0])) 
//					|| (((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0])) 
//					|| (((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]))
//					|| (((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0])) 
//					|| (((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]))
//					|| (((Use3m) && Lows[1][0] > ParabolicSAR_3m[0]))))
					

//				{
//					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
//				}
				
//				// Enter short
//				if ((High[0] < ParabolicSAR_Primary[0])
					
//					&& 
					
//					  ((((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0])) 
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0])) 
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0])) 
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0]))
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0]))
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0]))
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]))
//					|| (((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0])) 
//					|| (((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0])) 
//					|| (((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0])) 
//					|| (((Use10m) && High[0] < ParabolicSAR_10m[0]))
//					|| (((Use5m) && High[0] < ParabolicSAR_5m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0])) 
//					|| (((Use5m) && High[0] < ParabolicSAR_5m[0]))
//					|| (((Use3m) && High[0] < ParabolicSAR_3m[0]))))
					

//				{
//					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
//				}
				
				
				
				// Enter long
//				if ((Low[0] > ParabolicSAR_Primary[0])
//					&& (Lows[4][0] > ParabolicSAR_15m[0])
//					&& (Lows[3][0] > ParabolicSAR_10m[0])
//					&& (Lows[2][0] > ParabolicSAR_5m[0])
//					&& (Lows[1][0] > ParabolicSAR_3m[0]))
//				{
//					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
//				}
											
				// Enter short
//				if ((High[0] < ParabolicSAR_Primary[0])
//					&& (High[0] < ParabolicSAR_15m[0])
//					&& (High[0] < ParabolicSAR_10m[0])
//					&& (High[0] < ParabolicSAR_5m[0])
//					&& (High[0] < ParabolicSAR_3m[0]))
//				{
//					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
//				}