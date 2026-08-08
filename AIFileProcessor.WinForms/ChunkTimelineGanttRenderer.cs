//-----------------------------------------------------------------------
// <copyright file="ChunkTimelineGanttRenderer.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor
{
    using System;
    using System.Drawing;
    using System.Linq;
    using AIFileProcessor.Telemetry;

    /// <summary>
    /// Renders a Gantt-style timeline of per-chunk processing onto a <see cref="Graphics"/>
    /// surface. Each chunk is one row; the light segment is time spent queued on the
    /// concurrency throttle and the solid segment is the API call itself (start to
    /// completion). The same renderer is used for on-screen display and for producing
    /// the savable PNG, so the saved image matches what the user sees.
    /// </summary>
    internal static class ChunkTimelineGanttRenderer
    {
        private const int LabelWidth = 110;
        private const int RightPadding = 70;
        private const int TitleHeight = 48;
        private const int AxisHeight = 50;
        private const int BarTopGap = 16;
        private const int RowHeight = 24;
        private const int BarHeight = 14;
        private const int MinGraphWidth = 200;

        private static readonly Color QueuedColor = Color.FromArgb(255, 205, 145);
        private static readonly Color ActiveColor = Color.FromArgb(66, 133, 244);
        private static readonly Color AxisColor = Color.FromArgb(120, 120, 120);

        /// <summary>
        /// Computes the pixel size needed to render the full chart for the given metrics.
        /// </summary>
        /// <param name="metrics">The processing metrics to chart.</param>
        /// <returns>The required image size, never smaller than 1x1.</returns>
        public static Size GetRequiredSize(ProcessingMetrics metrics)
        {
            ArgumentNullException.ThrowIfNull(metrics);
            int rows = Math.Max(1, metrics.Chunks.Count);
            int height = TitleHeight + AxisHeight + BarTopGap + (rows * RowHeight) + 8;
            return new Size(900, height);
        }

        /// <summary>
        /// Draws the chart into the given bounds of a graphics surface.
        /// </summary>
        /// <param name="g">The target graphics surface.</param>
        /// <param name="metrics">The processing metrics to chart.</param>
        /// <param name="bounds">The rectangle to draw within.</param>
        /// <param name="titleSuffix">Optional extra context appended to the chart title
        /// (e.g. the model display name), so a saved PNG is self-describing.</param>
        public static void Draw(Graphics g, ProcessingMetrics metrics, Rectangle bounds, string? titleSuffix = null)
        {
            ArgumentNullException.ThrowIfNull(g);
            ArgumentNullException.ThrowIfNull(metrics);

            g.Clear(Color.White);

            var chunks = metrics.Chunks.OrderBy(c => c.ChunkIndex).ToList();
            if (chunks.Count == 0)
            {
                return;
            }

            DateTime origin = metrics.StartUtc;
            double totalMs = Math.Max(1.0, (metrics.EndUtc - origin).TotalMilliseconds);

            int graphWidth = Math.Max(MinGraphWidth, bounds.Width - LabelWidth - RightPadding);

            using var labelFont = new Font("Segoe UI", 9F);
            using var axisFont = new Font("Segoe UI", 8F);
            using var titleFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var labelBrush = new SolidBrush(Color.Black);
            using var axisBrush = new SolidBrush(AxisColor);
            using var queuedBrush = new SolidBrush(QueuedColor);
            using var activeBrush = new SolidBrush(ActiveColor);
            using var gridPen = new Pen(Color.FromArgb(230, 230, 230));
            using var axisPen = new Pen(AxisColor);
            using var barBorderPen = new Pen(Color.FromArgb(80, 80, 80));

            // Chart title: name of the view plus model/context so a saved PNG stands alone.
            string title = "Chunk processing timeline";
            if (!string.IsNullOrWhiteSpace(titleSuffix))
            {
                title += $" — {titleSuffix}";
            }

            title += $" · {metrics.Chunks.Count} chunk{(metrics.Chunks.Count == 1 ? string.Empty : "s")} · {metrics.ElapsedMs / 1000.0:n1} s";
            g.DrawString(title, titleFont, labelBrush, bounds.Left + 4, bounds.Top + 2);

            // Header legend, on its own line below the title to avoid overlap.
            int legendTop = bounds.Top + 26;
            g.FillRectangle(queuedBrush, bounds.Left + LabelWidth, legendTop, 12, 12);
            g.DrawString("Queued", labelFont, labelBrush, bounds.Left + LabelWidth + 16, legendTop - 1);
            g.FillRectangle(activeBrush, bounds.Left + LabelWidth + 86, legendTop, 12, 12);
            g.DrawString("API call", labelFont, labelBrush, bounds.Left + LabelWidth + 102, legendTop - 1);

            // Time axis gridlines and labels (5 divisions).
            int axisTop = bounds.Top + TitleHeight;
            int axisLeft = bounds.Left + LabelWidth;
            int axisBottomY = axisTop + AxisHeight - 8;
            for (int i = 0; i <= 5; i++)
            {
                double ms = totalMs * i / 5.0;
                int x = axisLeft + (int)(ms / totalMs * graphWidth);
                g.DrawLine(gridPen, x, axisTop + AxisHeight - 6, x, bounds.Bottom - 2);
                string label = $"{ms:n0} ms";
                var labelSize = g.MeasureString(label, axisFont);
                g.DrawString(label, axisFont, axisBrush, x - (labelSize.Width / 2), axisBottomY);
            }

            // One row per chunk.
            for (int row = 0; row < chunks.Count; row++)
            {
                var chunk = chunks[row];
                int rowTop = axisTop + AxisHeight + BarTopGap + (row * RowHeight);
                int barTop = rowTop + ((RowHeight - BarHeight) / 2);

                g.DrawString($"Chunk {chunk.ChunkIndex}", labelFont, labelBrush, bounds.Left + 4, barTop);

                double startMs = Math.Max(0, (chunk.Telemetry.StartUtc - origin).TotalMilliseconds);
                double endMs = Math.Max(startMs, (chunk.Telemetry.EndUtc - origin).TotalMilliseconds);
                double queueMs = Math.Min(chunk.QueueDelayMs, endMs - startMs);

                int xStart = axisLeft + (int)(startMs / totalMs * graphWidth);
                int xQueuedEnd = xStart + (int)(queueMs / totalMs * graphWidth);
                int xEnd = axisLeft + (int)(endMs / totalMs * graphWidth);
                if (xEnd <= xStart)
                {
                    xEnd = xStart + 2;
                }

                if (xQueuedEnd > xStart)
                {
                    g.FillRectangle(queuedBrush, xStart, barTop, xQueuedEnd - xStart, BarHeight);
                }

                g.FillRectangle(activeBrush, xQueuedEnd, barTop, xEnd - xQueuedEnd, BarHeight);
                g.DrawRectangle(barBorderPen, xStart, barTop, xEnd - xStart, BarHeight);

                // Duration label right of the bar when it fits.
                string duration = $"{chunk.Telemetry.ElapsedMs:n0} ms";
                if (xEnd + 4 + 60 < bounds.Right)
                {
                    g.DrawString(duration, axisFont, axisBrush, xEnd + 4, barTop);
                }
            }

            g.DrawLine(axisPen, axisLeft, axisTop + AxisHeight - 6, axisLeft + graphWidth, axisTop + AxisHeight - 6);
        }
    }
}
