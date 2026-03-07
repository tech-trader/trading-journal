// ============================================================
//  MarketProfileLevelsTable.cs
//  ATAS Custom Indicator
//
//  Displays an on-chart table containing:
//    • Previous Day RTH       – VAH, VAL, VPOC, High, Low, Close
//    • Globex (overnight)     – High, Low
//    • Today Asian Session    – High, Low
//    • Today London Session   – High, Low
//    • Previous Asian Session – High, Low
//    • Previous London Session– High, Low
//    • Previous Week          – VAH, VAL, VPOC, High, Low
//    • Current Week           – VAH, VAL, VPOC, High, Low
//
//  Volume profile (VAH / VAL / VPOC) is calculated by
//  distributing each bar's volume proportionally across its
//  high-low range using the instrument's tick size.
// ============================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using ATAS.Indicators;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;

namespace ATAS.Indicators.Custom
{
    [DisplayName("Market Profile Levels Table")]
    [Category("Personal")]
    public class MarketProfileLevelsTable : Indicator
    {
        // ─────────────────────────────────────────────────────────────────────
        //  Inner types
        // ─────────────────────────────────────────────────────────────────────

        private class TableSection
        {
            public string      Title       { get; set; } = string.Empty;
            public Color       HeaderColor { get; set; }
            public List<TableRow> Rows     { get; set; } = new();
        }

        private class TableRow
        {
            public string  Label      { get; set; } = string.Empty;
            public decimal Value      { get; set; }
            public Color   ValueColor { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Cached level values
        // ─────────────────────────────────────────────────────────────────────

        // Previous day RTH
        private decimal _pdVah, _pdVal, _pdVpoc, _pdHigh, _pdLow, _pdClose;

        // Previous sessions
        private decimal _asianHigh, _asianLow, _londonHigh, _londonLow;

        // Current day sessions
        private decimal _globexHigh, _globexLow;
        private decimal _curAsianHigh, _curAsianLow;
        private decimal _curLondonHigh, _curLondonLow;

        // Previous week
        private decimal _pwVah, _pwVal, _pwVpoc, _pwHigh, _pwLow;

        // Current week
        private decimal _cwVah, _cwVal, _cwVpoc, _cwHigh, _cwLow;

        // State
        private DateTime     _lastCalcDate = DateTime.MinValue;
        private bool         _calculated;
        private decimal      _tickSize     = 0.25m;
        private TimeZoneInfo? _easternTZ;

        // ─────────────────────────────────────────────────────────────────────
        //  User-configurable properties
        // ─────────────────────────────────────────────────────────────────────

        #region  Display
        [Display(Name = "Table X (pixels from left)", GroupName = "Display", Order = 1)]
        [Range(0, 3000)]
        public int TableX { get; set; } = 12;

        [Display(Name = "Table Y (pixels from top)", GroupName = "Display", Order = 2)]
        [Range(0, 3000)]
        public int TableY { get; set; } = 60;

        [Display(Name = "Font Size", GroupName = "Display", Order = 3)]
        [Range(8, 24)]
        public int FontSize { get; set; } = 11;

        [Display(Name = "Label Column Width (px)", GroupName = "Display", Order = 4)]
        [Range(60, 300)]
        public int LabelColumnWidth { get; set; } = 155;

        [Display(Name = "Value Column Width (px)", GroupName = "Display", Order = 5)]
        [Range(50, 200)]
        public int ValueColumnWidth { get; set; } = 82;
        #endregion

        #region  Colors
        [Display(Name = "Text Color",        GroupName = "Colors", Order = 10)]
        public Color TextColor     { get; set; } = Color.FromArgb(255, 220, 225, 230);

        [Display(Name = "Label Color",       GroupName = "Colors", Order = 11)]
        public Color LabelColor    { get; set; } = Color.FromArgb(255, 140, 170, 200);

        [Display(Name = "Row BG (even)",     GroupName = "Colors", Order = 12)]
        public Color RowBgEven     { get; set; } = Color.FromArgb(210,  12,  15,  20);

        [Display(Name = "Row BG (odd)",      GroupName = "Colors", Order = 13)]
        public Color RowBgOdd      { get; set; } = Color.FromArgb(210,  18,  22,  30);

        [Display(Name = "Border Color",      GroupName = "Colors", Order = 14)]
        public Color BorderColor   { get; set; } = Color.FromArgb(255,  45,  58,  75);

        [Display(Name = "VAH Color",         GroupName = "Colors", Order = 15)]
        public Color VahColor      { get; set; } = Color.FromArgb(255,  90, 200,  90);

        [Display(Name = "VAL Color",         GroupName = "Colors", Order = 16)]
        public Color ValColor      { get; set; } = Color.FromArgb(255, 210,  80,  80);

        [Display(Name = "VPOC Color",        GroupName = "Colors", Order = 17)]
        public Color VpocColor     { get; set; } = Color.FromArgb(255, 255, 210,  50);
        #endregion

        #region  Sessions  (Eastern Time)
        [Display(Name = "RTH Start Hour",          GroupName = "Sessions (ET)", Order = 20)]
        [Range(0, 23)] public int RthStartHour   { get; set; } = 9;

        [Display(Name = "RTH Start Minute",        GroupName = "Sessions (ET)", Order = 21)]
        [Range(0, 59)] public int RthStartMinute  { get; set; } = 30;

        [Display(Name = "RTH End Hour",            GroupName = "Sessions (ET)", Order = 22)]
        [Range(0, 23)] public int RthEndHour      { get; set; } = 16;

        [Display(Name = "RTH End Minute",          GroupName = "Sessions (ET)", Order = 23)]
        [Range(0, 59)] public int RthEndMinute    { get; set; } = 0;

        [Display(Name = "Asian Session Start (ET Hour)", GroupName = "Sessions (ET)", Order = 24)]
        [Range(0, 23)] public int AsianStartHour  { get; set; } = 18;

        [Display(Name = "Asian Session End (ET Hour)",   GroupName = "Sessions (ET)", Order = 25)]
        [Range(0, 23)] public int AsianEndHour    { get; set; } = 1;

        [Display(Name = "London Session Start (ET Hour)",GroupName = "Sessions (ET)", Order = 26)]
        [Range(0, 23)] public int LondonStartHour { get; set; } = 2;

        [Display(Name = "London Session End (ET Hour)",  GroupName = "Sessions (ET)", Order = 27)]
        [Range(0, 23)] public int LondonEndHour   { get; set; } = 8;

        [Display(Name = "London Session End (ET Min)",   GroupName = "Sessions (ET)", Order = 28)]
        [Range(0, 59)] public int LondonEndMinute { get; set; } = 30;

        [Display(Name = "Globex Start (ET Hour)",        GroupName = "Sessions (ET)", Order = 29)]
        [Range(0, 23)] public int GlobexStartHour { get; set; } = 18;
        #endregion

        #region  Volume Profile
        [Display(Name = "Value Area %", GroupName = "Volume Profile", Order = 30)]
        [Range(50, 99)]
        public int ValueAreaPercent { get; set; } = 70;

        [Display(Name = "Max Bars Lookback", GroupName = "Volume Profile", Order = 31)]
        [Range(1000, 50000)]
        public int MaxLookbackBars { get; set; } = 15000;
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        //  Constructor
        // ─────────────────────────────────────────────────────────────────────

        public MarketProfileLevelsTable() : base(true)
        {
            DenyToChangePanel = true;
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Indicator lifecycle
        // ─────────────────────────────────────────────────────────────────────

        protected override void OnRecalculate()
        {
            _lastCalcDate = DateTime.MinValue;
            _calculated   = false;
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            // Heavy calculation only needed on the most recent bar
            if (bar != CurrentBar - 1)
                return;

            // Refresh tick size
            if (InstrumentInfo != null && InstrumentInfo.TickSize > 0)
                _tickSize = InstrumentInfo.TickSize;

            var candle  = GetCandle(bar);
            var etNow   = ToET(candle.Time);
            var today   = etNow.Date;

            // Skip if already calculated for today (avoids re-run on every tick)
            if (_calculated && _lastCalcDate == today)
                return;

            CalculateAll(bar, today);

            _lastCalcDate = today;
            _calculated   = true;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Main calculation entry point
        // ─────────────────────────────────────────────────────────────────────

        private void CalculateAll(int currentBar, DateTime today)
        {
            ResetLevels();

            // Collect bars within lookback window; skip bars older than 20 calendar days
            int startBar        = Math.Max(0, currentBar - MaxLookbackBars);
            var cutoffDate      = today.AddDays(-20);

            var bars = new List<(DateTime et, IndicatorCandle c)>(MaxLookbackBars);
            for (int i = startBar; i <= currentBar; i++)
            {
                var c  = GetCandle(i);
                var et = ToET(c.Time);
                if (et.Date < cutoffDate)
                    continue;
                bars.Add((et, c));
            }

            if (bars.Count == 0)
                return;

            CalcPrevDayRth(bars, today);
            CalcPrevSessions(bars, today);
            CalcCurrentDaySessions(bars, today);
            CalcWeekly(bars, today);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Previous day RTH
        // ─────────────────────────────────────────────────────────────────────

        private void CalcPrevDayRth(List<(DateTime et, IndicatorCandle c)> bars, DateTime today)
        {
            var prevDay  = PrevTradingDay(today);
            var rthStart = new TimeSpan(RthStartHour, RthStartMinute, 0);
            var rthEnd   = new TimeSpan(RthEndHour,   RthEndMinute,   0);

            var rthBars = bars
                .Where(b => b.et.Date == prevDay
                         && b.et.TimeOfDay >= rthStart
                         && b.et.TimeOfDay <  rthEnd)
                .ToList();

            if (rthBars.Count == 0)
                return;

            _pdHigh  = rthBars.Max(b => b.c.High);
            _pdLow   = rthBars.Min(b => b.c.Low);
            _pdClose = rthBars[rthBars.Count - 1].c.Close;

            (_pdVah, _pdVal, _pdVpoc) = VolumeProfile(rthBars.Select(b => b.c));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Previous Asian + London sessions
        // ─────────────────────────────────────────────────────────────────────

        private void CalcPrevSessions(List<(DateTime et, IndicatorCandle c)> bars, DateTime today)
        {
            var prevDay     = PrevTradingDay(today);
            var dayBefore   = PrevTradingDay(prevDay);

            // Asian: dayBefore 18:00 ET  →  prevDay 01:00 ET  (crosses midnight)
            var asianBars = bars.Where(b =>
                (b.et.Date == dayBefore  && b.et.Hour >= AsianStartHour) ||
                (b.et.Date == prevDay    && b.et.Hour <  AsianEndHour)
            ).ToList();

            if (asianBars.Count > 0)
            {
                _asianHigh = asianBars.Max(b => b.c.High);
                _asianLow  = asianBars.Min(b => b.c.Low);
            }

            // London: prevDay LondonStart ET  →  prevDay LondonEnd ET
            var londonStart = new TimeSpan(LondonStartHour, 0,             0);
            var londonEnd   = new TimeSpan(LondonEndHour,   LondonEndMinute, 0);

            var londonBars = bars.Where(b =>
                b.et.Date         == prevDay    &&
                b.et.TimeOfDay    >= londonStart &&
                b.et.TimeOfDay    <  londonEnd
            ).ToList();

            if (londonBars.Count > 0)
            {
                _londonHigh = londonBars.Max(b => b.c.High);
                _londonLow  = londonBars.Min(b => b.c.Low);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Current day sessions  (Globex, Asian, London — for today's trade date)
        // ─────────────────────────────────────────────────────────────────────

        private void CalcCurrentDaySessions(List<(DateTime et, IndicatorCandle c)> bars, DateTime today)
        {
            // The overnight/globex window starts on the previous *calendar* day at
            // GlobexStartHour and runs until today's RTH open.  We use the calendar
            // day (today.AddDays(-1)) rather than PrevTradingDay so that Sunday
            // evening futures data is captured for Monday's trade date.
            var prevCalDay  = today.AddDays(-1);
            var rthOpen     = new TimeSpan(RthStartHour,   RthStartMinute, 0);
            var londonStart = new TimeSpan(LondonStartHour, 0,              0);
            var londonEnd   = new TimeSpan(LondonEndHour,   LondonEndMinute, 0);

            // ── Globex (overnight): prevCalDay GlobexStart → today RTH open ──
            var globexBars = bars.Where(b =>
                (b.et.Date == prevCalDay && b.et.Hour >= GlobexStartHour) ||
                (b.et.Date == today      && b.et.TimeOfDay < rthOpen)
            ).ToList();

            if (globexBars.Count > 0)
            {
                _globexHigh = globexBars.Max(b => b.c.High);
                _globexLow  = globexBars.Min(b => b.c.Low);
            }

            // ── Current day Asian: prevCalDay AsianStart → today AsianEnd ──
            var curAsianBars = bars.Where(b =>
                (b.et.Date == prevCalDay && b.et.Hour >= AsianStartHour) ||
                (b.et.Date == today      && b.et.Hour <  AsianEndHour)
            ).ToList();

            if (curAsianBars.Count > 0)
            {
                _curAsianHigh = curAsianBars.Max(b => b.c.High);
                _curAsianLow  = curAsianBars.Min(b => b.c.Low);
            }

            // ── Current day London: today LondonStart → today LondonEnd ──
            var curLondonBars = bars.Where(b =>
                b.et.Date      == today      &&
                b.et.TimeOfDay >= londonStart &&
                b.et.TimeOfDay <  londonEnd
            ).ToList();

            if (curLondonBars.Count > 0)
            {
                _curLondonHigh = curLondonBars.Max(b => b.c.High);
                _curLondonLow  = curLondonBars.Min(b => b.c.Low);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Previous week + current week
        // ─────────────────────────────────────────────────────────────────────

        private void CalcWeekly(List<(DateTime et, IndicatorCandle c)> bars, DateTime today)
        {
            // ISO-style: week starts Monday
            int daysFromMon    = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var thisWeekMon    = today.AddDays(-daysFromMon);
            var prevWeekMon    = thisWeekMon.AddDays(-7);
            var prevWeekFri    = thisWeekMon.AddDays(-1);  // inclusive

            var rthStart = new TimeSpan(RthStartHour, RthStartMinute, 0);
            var rthEnd   = new TimeSpan(RthEndHour,   RthEndMinute,   0);

            // Previous week RTH bars
            var pwBars = bars.Where(b =>
                b.et.Date      >= prevWeekMon &&
                b.et.Date      <= prevWeekFri &&
                b.et.TimeOfDay >= rthStart    &&
                b.et.TimeOfDay <  rthEnd
            ).ToList();

            if (pwBars.Count > 0)
            {
                _pwHigh = pwBars.Max(b => b.c.High);
                _pwLow  = pwBars.Min(b => b.c.Low);
                (_pwVah, _pwVal, _pwVpoc) = VolumeProfile(pwBars.Select(b => b.c));
            }

            // Current week RTH bars (Mon → today)
            var cwBars = bars.Where(b =>
                b.et.Date      >= thisWeekMon &&
                b.et.Date      <= today       &&
                b.et.TimeOfDay >= rthStart    &&
                b.et.TimeOfDay <  rthEnd
            ).ToList();

            if (cwBars.Count > 0)
            {
                _cwHigh = cwBars.Max(b => b.c.High);
                _cwLow  = cwBars.Min(b => b.c.Low);
                (_cwVah, _cwVal, _cwVpoc) = VolumeProfile(cwBars.Select(b => b.c));
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Volume profile core
        //  Distributes each bar's volume evenly across its price range at
        //  tick-size granularity, then finds VPOC and expands outward until
        //  ValueAreaPercent% of total volume is captured.
        // ─────────────────────────────────────────────────────────────────────

        private (decimal vah, decimal val, decimal vpoc) VolumeProfile(IEnumerable<IndicatorCandle> candles)
        {
            var profile      = new Dictionary<decimal, decimal>(4096);
            decimal totalVol = 0m;

            foreach (var c in candles)
            {
                if (c.Volume <= 0m || c.High <= c.Low)
                    continue;

                decimal ticks      = Math.Max(1m, Math.Round((c.High - c.Low) / _tickSize));
                decimal volPerTick = c.Volume / ticks;

                for (decimal p = c.Low; p <= c.High + _tickSize * 0.01m; p += _tickSize)
                {
                    decimal level = RoundToTick(p);
                    profile.TryGetValue(level, out var existing);
                    profile[level] = existing + volPerTick;
                }

                totalVol += c.Volume;
            }

            if (profile.Count == 0 || totalVol <= 0m)
                return (0m, 0m, 0m);

            // VPOC
            var vpoc = profile.MaxBy(kv => kv.Value).Key;

            // Value Area expansion
            var levels     = profile.Keys.OrderBy(k => k).ToList();
            int vpocIdx    = levels.IndexOf(vpoc);
            int upper      = vpocIdx;
            int lower      = vpocIdx;
            decimal vaVol  = profile[vpoc];
            decimal target = totalVol * (ValueAreaPercent / 100m);

            while (vaVol < target)
            {
                bool canUp   = upper < levels.Count - 1;
                bool canDown = lower > 0;
                if (!canUp && !canDown) break;

                decimal upVol   = canUp   ? profile[levels[upper + 1]] : -1m;
                decimal downVol = canDown ? profile[levels[lower - 1]] : -1m;

                if (canUp && upVol >= downVol)
                {
                    upper++;
                    vaVol += profile[levels[upper]];
                }
                else
                {
                    lower--;
                    vaVol += profile[levels[lower]];
                }
            }

            return (levels[upper], levels[lower], vpoc);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Rendering
        // ─────────────────────────────────────────────────────────────────────

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            if (!_calculated)
                return;

            DrawTable(context);
        }

        private void DrawTable(RenderContext context)
        {
            var font     = new RenderFont("Consolas", FontSize);
            var boldFont = new RenderFont("Consolas", FontSize, FontStyle.Bold);

            int rowH       = FontSize + 9;
            int padX       = 6;
            int tableW     = LabelColumnWidth + ValueColumnWidth;
            int x          = TableX;
            int y          = TableY;

            var sections   = BuildSections();

            // Measure total height
            int totalH = sections.Sum(s => rowH + s.Rows.Count * rowH) + 2;

            // Outer background + border
            var outerRect = new Rectangle(x, y, tableW, totalH);
            context.FillRectangle(Color.FromArgb(215, 8, 10, 16), outerRect);
            context.DrawRectangle(new RenderPen(BorderColor, 1f), outerRect);

            int curY = y + 1;

            foreach (var section in sections)
            {
                // Section header
                var headerRect = new Rectangle(x + 1, curY, tableW - 2, rowH);
                context.FillRectangle(section.HeaderColor, headerRect);
                context.DrawString(section.Title, boldFont, TextColor,
                    x + padX, curY + (rowH - FontSize) / 2);
                curY += rowH;

                // Separator under header
                context.DrawLine(new RenderPen(BorderColor, 1f),
                    x + 1, curY - 1, x + tableW - 2, curY - 1);

                // Data rows
                bool alt = false;
                foreach (var row in section.Rows)
                {
                    var rowRect = new Rectangle(x + 1, curY, tableW - 2, rowH);
                    context.FillRectangle(alt ? RowBgOdd : RowBgEven, rowRect);

                    // Label
                    context.DrawString(row.Label, font, LabelColor,
                        x + padX, curY + (rowH - FontSize) / 2);

                    // Value
                    string valStr  = row.Value > 0m ? row.Value.ToString("F2") : "—";
                    Color  valCol  = row.ValueColor != Color.Empty ? row.ValueColor : TextColor;
                    context.DrawString(valStr, font, valCol,
                        x + LabelColumnWidth + padX, curY + (rowH - FontSize) / 2);

                    curY += rowH;
                    alt   = !alt;
                }

                // Separator between sections
                context.DrawLine(new RenderPen(BorderColor, 1f),
                    x + 1, curY, x + tableW - 2, curY);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Table data builder
        // ─────────────────────────────────────────────────────────────────────

        private List<TableSection> BuildSections() => new()
        {
            new TableSection
            {
                Title       = "◈  PREV DAY RTH",
                HeaderColor = Color.FromArgb(200, 18, 52, 72),
                Rows        = new List<TableRow>
                {
                    new() { Label = "VAH",   Value = _pdVah,   ValueColor = VahColor  },
                    new() { Label = "VAL",   Value = _pdVal,   ValueColor = ValColor  },
                    new() { Label = "VPOC",  Value = _pdVpoc,  ValueColor = VpocColor },
                    new() { Label = "High",  Value = _pdHigh,  ValueColor = Color.Empty },
                    new() { Label = "Low",   Value = _pdLow,   ValueColor = Color.Empty },
                    new() { Label = "Close", Value = _pdClose, ValueColor = Color.Empty },
                }
            },
            new TableSection
            {
                Title       = "◈  GLOBEX  (18:00–09:30 ET)",
                HeaderColor = Color.FromArgb(200, 45, 20, 65),
                Rows        = new List<TableRow>
                {
                    new() { Label = "High", Value = _globexHigh, ValueColor = Color.Empty },
                    new() { Label = "Low",  Value = _globexLow,  ValueColor = Color.Empty },
                }
            },
            new TableSection
            {
                Title       = "◈  TODAY ASIAN  (18:00–01:00 ET)",
                HeaderColor = Color.FromArgb(200, 10, 50, 90),
                Rows        = new List<TableRow>
                {
                    new() { Label = "High", Value = _curAsianHigh, ValueColor = Color.Empty },
                    new() { Label = "Low",  Value = _curAsianLow,  ValueColor = Color.Empty },
                }
            },
            new TableSection
            {
                Title       = "◈  TODAY LONDON  (02:00–08:30 ET)",
                HeaderColor = Color.FromArgb(200,  8, 70, 45),
                Rows        = new List<TableRow>
                {
                    new() { Label = "High", Value = _curLondonHigh, ValueColor = Color.Empty },
                    new() { Label = "Low",  Value = _curLondonLow,  ValueColor = Color.Empty },
                }
            },
            new TableSection
            {
                Title       = "◈  PREV ASIAN  (18:00–01:00 ET)",
                HeaderColor = Color.FromArgb(200, 10, 40, 72),
                Rows        = new List<TableRow>
                {
                    new() { Label = "High", Value = _asianHigh, ValueColor = Color.Empty },
                    new() { Label = "Low",  Value = _asianLow,  ValueColor = Color.Empty },
                }
            },
            new TableSection
            {
                Title       = "◈  PREV LONDON  (02:00–08:30 ET)",
                HeaderColor = Color.FromArgb(200,  8, 55, 38),
                Rows        = new List<TableRow>
                {
                    new() { Label = "High", Value = _londonHigh, ValueColor = Color.Empty },
                    new() { Label = "Low",  Value = _londonLow,  ValueColor = Color.Empty },
                }
            },
            new TableSection
            {
                Title       = "◈  PREV WEEK",
                HeaderColor = Color.FromArgb(200, 50, 24, 72),
                Rows        = new List<TableRow>
                {
                    new() { Label = "VAH",  Value = _pwVah,  ValueColor = VahColor  },
                    new() { Label = "VAL",  Value = _pwVal,  ValueColor = ValColor  },
                    new() { Label = "VPOC", Value = _pwVpoc, ValueColor = VpocColor },
                    new() { Label = "High", Value = _pwHigh, ValueColor = Color.Empty },
                    new() { Label = "Low",  Value = _pwLow,  ValueColor = Color.Empty },
                }
            },
            new TableSection
            {
                Title       = "◈  CURRENT WEEK",
                HeaderColor = Color.FromArgb(200, 55, 45, 10),
                Rows        = new List<TableRow>
                {
                    new() { Label = "VAH",  Value = _cwVah,  ValueColor = VahColor  },
                    new() { Label = "VAL",  Value = _cwVal,  ValueColor = ValColor  },
                    new() { Label = "VPOC", Value = _cwVpoc, ValueColor = VpocColor },
                    new() { Label = "High", Value = _cwHigh, ValueColor = Color.Empty },
                    new() { Label = "Low",  Value = _cwLow,  ValueColor = Color.Empty },
                }
            },
        };

        // ─────────────────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────────────────

        private DateTime ToET(DateTime time)
        {
            if (_easternTZ is null)
            {
                // Try Windows ID first, fall back to IANA ID (Linux / macOS)
                try         { _easternTZ = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"); }
                catch       { _easternTZ = TimeZoneInfo.FindSystemTimeZoneById("America/New_York"); }
            }

            var utc = DateTime.SpecifyKind(time, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, _easternTZ);
        }

        private static DateTime PrevTradingDay(DateTime date)
        {
            var d = date.AddDays(-1);
            while (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                d = d.AddDays(-1);
            return d;
        }

        private decimal RoundToTick(decimal price) =>
            Math.Round(price / _tickSize) * _tickSize;

        private void ResetLevels()
        {
            _pdVah = _pdVal = _pdVpoc = _pdHigh = _pdLow = _pdClose = 0m;
            _asianHigh = _asianLow = _londonHigh = _londonLow = 0m;
            _globexHigh = _globexLow = 0m;
            _curAsianHigh = _curAsianLow = 0m;
            _curLondonHigh = _curLondonLow = 0m;
            _pwVah = _pwVal = _pwVpoc = _pwHigh = _pwLow = 0m;
            _cwVah = _cwVal = _cwVpoc = _cwHigh = _cwLow = 0m;
        }
    }
}
