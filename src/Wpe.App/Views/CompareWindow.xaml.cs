using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WPELibrary.Lib;

namespace Wpe.App.Views
{
    public partial class CompareWindow : Window
    {
        private static readonly Brush DiffBrush = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));
        private static readonly Brush DiffBackBrush = new SolidColorBrush(Color.FromRgb(0xFD, 0xE8, 0xE8));

        public CompareWindow(Socket_PacketInfo spi)
        {
            InitializeComponent();

            if (spi != null)
                Render(spi);
        }

        private void Render(Socket_PacketInfo spi)
        {
            byte[] raw = spi.RawBuffer ?? Array.Empty<byte>();
            byte[] mod = spi.PacketBuffer ?? Array.Empty<byte>();

            RawTitle.Text = string.Format("原始数据（{0} 字节）", raw.Length);
            ModTitle.Text = string.Format("修改数据（{0} 字节）", mod.Length);

            BuildDump(RawDump, raw, mod);
            BuildDump(ModDump, mod, raw);
        }

        /// <summary>渲染 data 的十六进制转储，与 other 逐字节比较，差异字节高亮。</summary>
        private static void BuildDump(TextBlock target, byte[] data, byte[] other)
        {
            target.Inlines.Clear();

            for (int i = 0; i < data.Length; i += 16)
            {
                target.Inlines.Add(new Run(i.ToString("X8") + "  "));

                int lineLen = Math.Min(16, data.Length - i);
                for (int j = 0; j < 16; j++)
                {
                    if (j < lineLen)
                    {
                        int idx = i + j;
                        bool diff = idx >= other.Length || other[idx] != data[idx];
                        var run = new Run(data[idx].ToString("X2") + " ");
                        if (diff)
                        {
                            run.Foreground = DiffBrush;
                            run.Background = DiffBackBrush;
                            run.FontWeight = FontWeights.Bold;
                        }
                        target.Inlines.Add(run);
                    }
                    else
                    {
                        target.Inlines.Add(new Run("   "));
                    }
                    if (j == 7) target.Inlines.Add(new Run(" "));
                }

                target.Inlines.Add(new Run(" "));
                for (int j = 0; j < lineLen; j++)
                {
                    int idx = i + j;
                    byte b = data[idx];
                    bool diff = idx >= other.Length || other[idx] != data[idx];
                    var run = new Run((b >= 0x20 && b < 0x7F ? (char)b : '.').ToString());
                    if (diff)
                    {
                        run.Foreground = DiffBrush;
                        run.FontWeight = FontWeights.Bold;
                    }
                    target.Inlines.Add(run);
                }

                target.Inlines.Add(new LineBreak());
            }
        }

        private void OnClose(object sender, RoutedEventArgs e) => Close();
    }
}
