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
	public class LinRegCrossover : Strategy
	{
		private LinReg LinRegFast;
		private LinReg LinRegSlow;
		
		private double 	AvgPxLong;
		private double 	AvgPxShort;
		
		private Order LongStop = null;
		private Order ShortStop = null;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "LinRegCrossover";
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
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;
				//
				PerFast										= 15;
				PerSlow										= 30;
				TriggerBE									= 5;
				Stop										= 10;
			}
			
			else if (State == State.Configure)
			{
				//Add tick data series for TickReplay
				AddDataSeries(Data.BarsPeriodType.Tick, 1);
			}
			
			else if (State == State.DataLoaded)
			{				
				LinRegFast									= LinReg(Close, Convert.ToInt32(PerFast));
				LinRegSlow									= LinReg(Close, Convert.ToInt32(PerSlow));
				
				LinRegFast.Plots[0].Brush = Brushes.Goldenrod;
				LinRegSlow.Plots[0].Brush = Brushes.MediumAquamarine;
				AddChartIndicator(LinRegFast);
				AddChartIndicator(LinRegSlow);
			}
		}
		
		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;
			
			// Entries
		//	if (Position.MarketPosition == MarketPosition.Flat)
			
			if (CrossAbove(LinRegFast, LinRegSlow, 1))
			{
				EnterLong(1, Convert.ToInt32(DefaultQuantity), @"Long");
			}
			
			else if (CrossBelow(LinRegFast, LinRegSlow, 1))
			{
				EnterShort(1, Convert.ToInt32(DefaultQuantity), @"Short");
			}
		}
			
		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			
				
			// Modify stops	
			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
				
				if ((Position.MarketPosition == MarketPosition.Long)
					&& (marketDataUpdate.Price > (AvgPxLong + (TriggerBE * TickSize))))
				{
					ChangeOrder(LongStop, LongStop.Quantity, 0, AvgPxLong + 1 * TickSize);
				}
				
				else if ((Position.MarketPosition == MarketPosition.Short)
					&& (marketDataUpdate.Price < (AvgPxShort - (TriggerBE * TickSize))))
				{
					ChangeOrder(ShortStop, ShortStop.Quantity, 0, AvgPxShort - 1 * TickSize);
				}
				
		}
		
		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
		{
			if (order.Name == "Long")
				AvgPxLong = order.AverageFillPrice;
			if (order.Name == "Short")
				AvgPxShort = order.AverageFillPrice;
			
			if (order.Name == "LongStop")
		      	LongStop = order;
			if (order.Name == "ShortStop")
				ShortStop = order;
		}
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (Position.MarketPosition == MarketPosition.Long)
			{
				ExitLongStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxLong - (Stop * TickSize), @"LongStop", @"Long");	
			}
			
			else if (Position.MarketPosition == MarketPosition.Short)
			{
				ExitShortStopMarket(1, true, Convert.ToInt32(DefaultQuantity), AvgPxShort + (Stop * TickSize), @"ShortStop", @"Short");	
			}
				
		}
		


		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PerFast", Order=1, GroupName="Parameters")]
		public int PerFast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PerSlow", Order=2, GroupName="Parameters")]
		public int PerSlow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TriggerBE", Order=3, GroupName="Parameters")]
		public int TriggerBE
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stop", Order=4, GroupName="Parameters")]
		public int Stop
		{ get; set; }
		#endregion

	}
}
