using System.Runtime.CompilerServices;
using DarkModeControls;
using randz.CustomControls;

namespace AdbFileManager {
    internal static class AppTheme {
        internal static readonly Color Window = ColorTranslator.FromHtml("#0D1117");
        internal static readonly Color Surface = ColorTranslator.FromHtml("#161B22");
        internal static readonly Color SurfaceRaised = ColorTranslator.FromHtml("#21262D");
        internal static readonly Color Border = ColorTranslator.FromHtml("#30363D");
        internal static readonly Color Text = ColorTranslator.FromHtml("#F0F6FC");
        internal static readonly Color MutedText = ColorTranslator.FromHtml("#8B949E");
        internal static readonly Color Accent = ColorTranslator.FromHtml("#2F81F7");
        internal static readonly Color Selection = ColorTranslator.FromHtml("#1F6FEB");
        internal static readonly Color Error = ColorTranslator.FromHtml("#FF7B72");

        private sealed class NativeHook { }
        private static readonly ConditionalWeakTable<Control, NativeHook> nativeHooks = new();

        internal static void Apply(Form form) {
            if (!SettingsManager.settings.DarkMode) return;
            form.SuspendLayout();
            try {
                ApplyControl(form);
                ApplyWindowFrame(form);
            }
            finally { form.ResumeLayout(true); }
        }

        private static void ApplyControl(Control control) {
            switch (control) {
                case Form form:
                    form.BackColor = Window;
                    form.ForeColor = Text;
                    break;
                case DataGridView grid:
                    StyleGrid(grid);
                    break;
                case DarkCommandLink link:
                    link.BackColor = Surface;
                    link.ForeColor = Text;
                    link.MainTextColor = Text;
                    link.NoteTextColor = MutedText;
                    break;
                case FluentButton fluent:
                    fluent.EnableDarkMode();
                    fluent.BackColor = SurfaceRaised;
                    fluent.ForeColor = Text;
                    break;
                case VerticalLabel vertical:
                    vertical.EnableDarkMode();
                    vertical.BackColor = SurfaceRaised;
                    vertical.ForeColor = Text;
                    break;
                case TextBoxBase textBox:
                    textBox.BackColor = Surface;
                    textBox.ForeColor = Text;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case ComboBox combo:
                    combo.BackColor = SurfaceRaised;
                    combo.ForeColor = Text;
                    combo.FlatStyle = FlatStyle.Flat;
                    break;
                case ListBox list:
                    list.BackColor = Surface;
                    list.ForeColor = Text;
                    break;
                case ListView listView:
                    listView.BackColor = Surface;
                    listView.ForeColor = Text;
                    break;
                case TreeView tree:
                    tree.BackColor = Surface;
                    tree.ForeColor = Text;
                    break;
                case CheckBox checkBox:
                    checkBox.BackColor = Color.Transparent;
                    checkBox.ForeColor = Text;
                    checkBox.UseVisualStyleBackColor = false;
                    break;
                case RadioButton radioButton:
                    radioButton.BackColor = Color.Transparent;
                    radioButton.ForeColor = Text;
                    radioButton.UseVisualStyleBackColor = false;
                    break;
                case Button button:
                    button.BackColor = SurfaceRaised;
                    button.ForeColor = Text;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = Border;
                    button.FlatAppearance.BorderSize = 1;
                    button.UseVisualStyleBackColor = false;
                    break;
                case LinkLabel linkLabel:
                    linkLabel.BackColor = Color.Transparent;
                    linkLabel.ForeColor = Text;
                    linkLabel.LinkColor = ColorTranslator.FromHtml("#58A6FF");
                    linkLabel.ActiveLinkColor = ColorTranslator.FromHtml("#79C0FF");
                    linkLabel.VisitedLinkColor = ColorTranslator.FromHtml("#BC8CFF");
                    break;
                case TabControl tabs:
                    StyleTabs(tabs);
                    break;
                case Label label:
                    label.BackColor = Color.Transparent;
                    label.ForeColor = Text;
                    break;
                case TabPage tabPage:
                    tabPage.BackColor = Window;
                    tabPage.ForeColor = Text;
                    tabPage.UseVisualStyleBackColor = false;
                    break;
                case GroupBox group:
                    group.BackColor = Window;
                    group.ForeColor = Text;
                    break;
                case TableLayoutPanel table:
                    table.BackColor = Window;
                    table.ForeColor = Text;
                    break;
                case FlowLayoutPanel flow:
                    flow.BackColor = Window;
                    flow.ForeColor = Text;
                    break;
                case Panel panel:
                    panel.BackColor = Window;
                    panel.ForeColor = Text;
                    break;
                case ToolStrip toolStrip:
                    toolStrip.BackColor = Surface;
                    toolStrip.ForeColor = Text;
                    toolStrip.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
                    break;
                default:
                    control.ForeColor = Text;
                    break;
            }

            HookNativeTheme(control);
            foreach (Control child in control.Controls) ApplyControl(child);
        }

        private static void StyleGrid(DataGridView grid) {
            grid.EnableHeadersVisualStyles = false;
            grid.BackgroundColor = Window;
            grid.GridColor = Border;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = Selection;
            grid.DefaultCellStyle.SelectionForeColor = Text;
            grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceRaised;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = Text;
            grid.ColumnHeadersDefaultCellStyle.BackColor = SurfaceRaised;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceRaised;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;
            grid.RowHeadersDefaultCellStyle.BackColor = SurfaceRaised;
            grid.RowHeadersDefaultCellStyle.ForeColor = Text;
            grid.RowHeadersDefaultCellStyle.SelectionBackColor = Selection;
            grid.RowHeadersDefaultCellStyle.SelectionForeColor = Text;
            grid.CellPainting -= PaintGridCell;
            grid.CellPainting += PaintGridCell;
        }

        private static void PaintGridCell(object? sender, DataGridViewCellPaintingEventArgs e) {
            if (sender is not DataGridView grid) return;

            if (e.RowIndex == -1 && e.ColumnIndex >= 0) {
                using var background = new SolidBrush(SurfaceRaised);
                using var border = new Pen(Border);
                e.Graphics.FillRectangle(background, e.CellBounds);

                DataGridViewColumn column = grid.Columns[e.ColumnIndex];
                SortOrder sortOrder = column.HeaderCell.SortGlyphDirection;
                int glyphSpace = sortOrder == SortOrder.None ? 0 : 16;
                bool narrowHeader = e.CellBounds.Width < 50;
                int horizontalPadding = narrowHeader ? 1 : 8;
                Rectangle textBounds = new(
                    e.CellBounds.Left + horizontalPadding,
                    e.CellBounds.Top,
                    Math.Max(0, e.CellBounds.Width - horizontalPadding * 2 - glyphSpace),
                    e.CellBounds.Height);
                TextFormatFlags textFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix |
                    (narrowHeader
                        ? TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding
                        : TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
                TextRenderer.DrawText(e.Graphics, Convert.ToString(e.FormattedValue) ?? string.Empty,
                    e.CellStyle.Font ?? grid.Font, textBounds, Text, textFlags);

                if (sortOrder != SortOrder.None) {
                    int centerX = e.CellBounds.Right - 10;
                    int centerY = e.CellBounds.Top + e.CellBounds.Height / 2;
                    Point[] glyph = sortOrder == SortOrder.Ascending
                        ? new[] { new Point(centerX - 4, centerY + 2), new Point(centerX + 4, centerY + 2), new Point(centerX, centerY - 3) }
                        : new[] { new Point(centerX - 4, centerY - 2), new Point(centerX + 4, centerY - 2), new Point(centerX, centerY + 3) };
                    using var glyphBrush = new SolidBrush(MutedText);
                    e.Graphics.FillPolygon(glyphBrush, glyph);
                }

                e.Graphics.DrawLine(border, e.CellBounds.Right - 1, e.CellBounds.Top,
                    e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                e.Graphics.DrawLine(border, e.CellBounds.Left, e.CellBounds.Bottom - 1,
                    e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                e.Handled = true;
                return;
            }

            if (e.RowIndex >= 0 && (e.State & DataGridViewElementStates.Selected) != 0) {
                using var selection = new SolidBrush(Selection);
                e.Graphics.FillRectangle(selection, e.CellBounds);
                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
        }

        private static void StyleTabs(TabControl tabs) {
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.SizeMode = TabSizeMode.Normal;
            tabs.Padding = new Point(14, 5);
            tabs.DrawItem -= DrawTab;
            tabs.DrawItem += DrawTab;
        }

        private static void DrawTab(object? sender, DrawItemEventArgs e) {
            if (sender is not TabControl tabs || e.Index < 0 || e.Index >= tabs.TabPages.Count) return;
            Rectangle bounds = tabs.GetTabRect(e.Index);
            bool selected = e.Index == tabs.SelectedIndex;
            using var background = new SolidBrush(selected ? SurfaceRaised : Window);
            e.Graphics.FillRectangle(background, bounds);
            TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text, tabs.Font, bounds, Text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            using var border = new Pen(Border);
            e.Graphics.DrawRectangle(border, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
        }

        private static void ApplyWindowFrame(Form form) {
            if (form.IsHandleCreated) DarkModeStartup.ApplyToWindow(form.Handle);
            form.HandleCreated -= WindowHandleCreated;
            form.HandleCreated += WindowHandleCreated;
        }

        private static void WindowHandleCreated(object? sender, EventArgs e) {
            if (sender is Form form && SettingsManager.settings.DarkMode)
                DarkModeStartup.ApplyToWindow(form.Handle);
        }

        private static void HookNativeTheme(Control control) {
            if (!nativeHooks.TryGetValue(control, out _)) {
                nativeHooks.Add(control, new NativeHook());
                control.HandleCreated += (_, _) => {
                    if (SettingsManager.settings.DarkMode)
                        DarkModeStartup.ApplyToControl(control.Handle);
                };
            }
            if (control.IsHandleCreated) DarkModeStartup.ApplyToControl(control.Handle);
        }

        private sealed class DarkColorTable : ProfessionalColorTable {
            public override Color ToolStripDropDownBackground => Surface;
            public override Color MenuBorder => Border;
            public override Color MenuItemBorder => Border;
            public override Color MenuItemSelected => SurfaceRaised;
            public override Color ImageMarginGradientBegin => Surface;
            public override Color ImageMarginGradientMiddle => Surface;
            public override Color ImageMarginGradientEnd => Surface;
            public override Color ButtonSelectedBorder => Border;
            public override Color ButtonSelectedGradientBegin => SurfaceRaised;
            public override Color ButtonSelectedGradientEnd => SurfaceRaised;
            public override Color ButtonPressedGradientBegin => Selection;
            public override Color ButtonPressedGradientEnd => Selection;
        }
    }
}
